using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }
    public NetworkPlayerAnimator playerPrefab;
    private Dictionary<string, NetworkPlayerAnimator> players = new Dictionary<string, NetworkPlayerAnimator>();
    public NetworkPlayerAnimator LocalPlayer { get; private set; }

    private Transform startPoint;
    private List<Transform> spawnPoints = new List<Transform>();
    private int currentSpawnIndex = 0;

    private bool isInitialized = false;
    private Queue<Dictionary<string, object>> pendingMessages = new Queue<Dictionary<string, object>>();

    private bool isRunning = true;

    private bool allPlayersSpawned = false;

    private void OnDestroy()
    {
        isRunning = false;
    }

    private void Awake()
    {
        if (Instance == null) Instance = this;

        // StartPoint 찾기
        startPoint = GameObject.Find("StartPoint")?.transform;
        if (startPoint != null)
        {
            // 자식 오브젝트들을 리스트에 추가
            for (int i = 0; i < startPoint.childCount; i++)
            {
                spawnPoints.Add(startPoint.GetChild(i));
            }
        }
        else
        {
            Debug.Log("StartPoint not found in the scene!");
        }
    }
    private void Start()
    {
        SpawnPlayers();
        _ = ListenNetworkMessages();
    }

    private async void SpawnPlayers()
    {
        try
        {
            Vector3 spawnPosition = GetNextSpawnPosition();

            // 먼저 로컬 플레이어를 생성
            SpawnLocalPlayer(spawnPosition);

            var spawnData = new
            {
                action = "player_spawn",
                playerId = UserData.Instance.UserId,
                position = new { x = spawnPosition.x, y = spawnPosition.y, z = spawnPosition.z },
                maxHealth = UserData.Instance.Character.MaxHealth,
                attackPower = UserData.Instance.Character.AttackPower
            };

            // 서버에 스폰 알림
            string response = await ServerConnector.Instance.SendMessage(JsonConvert.SerializeObject(spawnData));
            Debug.Log($"Spawn response: {response}"); // 디버그용

            if (!string.IsNullOrEmpty(response))
            {
                // 서버가 success를 보냈다면 대기 시작
                await WaitAllPlayers();
            }
            else
            {
                Debug.LogError("Spawn response was empty");
            }
        }
        catch (Exception ex)
        {
            Debug.LogError($"Error in SpawnPlayers: {ex.Message}");
        }
    }


    private async Task WaitAllPlayers()
    {
        Debug.Log("Waiting for all players...");
        // 모든 플레이어가 스폰될 때까지 대기
        //while (!allPlayersSpawned && isRunning)
        //{
        //    await Task.Delay(100);
        //}

        Debug.Log("All players spawned, initializing game...");
        isInitialized = true;
        PendingMessages();
    }

    private void PendingMessages()
    {
        Debug.Log($"Processing {pendingMessages.Count} pending messages");
        while (pendingMessages.Count > 0)
        {
            var message = pendingMessages.Dequeue();

            // 스폰 메시지는 이미 처리되었으므로 다른 메시지들만 처리
            if (message["action"].ToString() != "player_spawn")
            {
                NetworkMessage(message);
            }
        }
    }

    private async Task ListenNetworkMessages()
    {
        while (isRunning)
        {
            try
            {
                string message = await ServerConnector.Instance?.ReadMessage();
                if (!string.IsNullOrEmpty(message))
                {
                    message = message.Trim();
                    if (message.Contains("}{"))
                    {
                        message = message.Split(new[] { "}{" }, StringSplitOptions.None)[0] + "}";
                    }

                    var data = JsonConvert.DeserializeObject<Dictionary<string, object>>(message);

                    if (!isInitialized)
                    {
                        pendingMessages.Enqueue(data);
                    }
                    else
                    {
                        NetworkMessage(data);
                    }
                }
                await Task.Delay(10);
            }
            catch (Exception ex)
            {
                if (isRunning)
                {
                    Debug.LogError($"Error in ListenForNetworkMessages: {ex.Message}");
                    await Task.Delay(100);
                }
            }
        }
    }

    private void NetworkMessage(Dictionary<string, object> data)
    {
        if (data == null || !data.ContainsKey("action")) return;

        string action = data["action"].ToString();
        Debug.Log($"Received network message: {action}"); // 디버그용

        // 스폰 완료 메시지 처리
        if (action == "all_players_ready")
        {
            Debug.Log("Received all players ready signal");
            allPlayersSpawned = true;
            return;
        }

        // 모든 플레이어가 스폰되기 전에는 스폰 메시지만 처리
        if (!allPlayersSpawned)
        {
            if (action == "player_spawn")
            {
                PlayerSpawn(data);
            }
            else
            {
                pendingMessages.Enqueue(data);
            }
            return;
        }

        // 모든 플레이어 스폰 완료 후 모든 메시지 처리
        switch (action)
        {
            case "player_spawn":
                PlayerSpawn(data);
                break;
            case "player_state":
                PlayerState(data);
                break;
            case "player_action":
                PlayerAction(data);
                break;
        }
    }

    private void PlayerSpawn(Dictionary<string, object> data)
    {
        try
        {
            string playerId = data["playerId"].ToString();
            Debug.Log($"Processing spawn for player: {playerId}");

            if (playerId == UserData.Instance.UserId)
            {
                Debug.Log("Ignoring own spawn message");
                return;
            }

            if (players.ContainsKey(playerId))
            {
                Debug.Log($"Player {playerId} already exists, updating position");
                var positionData = JsonConvert.DeserializeObject<Dictionary<string, float>>(data["position"].ToString());
                Vector3 spawnPosition = new Vector3(positionData["x"], positionData["y"], positionData["z"]);
                players[playerId].transform.position = spawnPosition;
                return;
            }

            var newPositionData = JsonConvert.DeserializeObject<Dictionary<string, float>>(data["position"].ToString());
            Vector3 newSpawnPosition = new Vector3(newPositionData["x"], newPositionData["y"], newPositionData["z"]);

            Debug.Log($"Spawning new player at position: {newSpawnPosition}");
            var player = Instantiate(playerPrefab, newSpawnPosition, Quaternion.identity);
            player.Initialize(playerId, false);
            players[playerId] = player;

            Debug.Log($"Remote player spawned successfully: {playerId}");
        }
        catch (Exception ex)
        {
            Debug.LogError($"Error spawning player: {ex.Message}");
        }
    }

    private void SpawnLocalPlayer(Vector3 position)
    {
        if (players.ContainsKey(UserData.Instance.UserId))
        {
            Debug.LogWarning("Local player already exists!");
            return;
        }

        var player = Instantiate(playerPrefab, position, Quaternion.identity);
        player.Initialize(UserData.Instance.UserId, true);
        players[UserData.Instance.UserId] = player;
        LocalPlayer = player;

        StartCoroutine(SendPlayerState());
        Debug.Log($"Local player spawned: {UserData.Instance.UserId}");
    }


    private Vector3 GetNextSpawnPosition()
    {
        if (spawnPoints.Count == 0) return Vector3.zero;
        Vector3 position = spawnPoints[currentSpawnIndex].position;
        currentSpawnIndex = (currentSpawnIndex + 1) % spawnPoints.Count;
        return position;
    }

    private IEnumerator SendPlayerState()
    {
        while (true)
        {
            if (LocalPlayer != null)
            {
                var position = LocalPlayer.transform.position;
                var rotation = LocalPlayer.transform.rotation;

                var stateData = new
                {
                    action = "player_state",
                    playerId = UserData.Instance.UserId,
                    position = new { x = position.x, y = position.y, z = position.z },
                    rotation = new { x = rotation.x, y = rotation.y, z = rotation.z, w = rotation.w },
                    isRunning = LocalPlayer._isRunning,
                    isAction = LocalPlayer.isAction,
                    currentHealth = LocalPlayer._hp,
                    maxHealth = UserData.Instance.Character.MaxHealth,
                    attackPower = LocalPlayer._str
                };

                _ = ServerConnector.Instance.SendMessage(JsonConvert.SerializeObject(stateData));
            }
            yield return new WaitForSeconds(0.1f);
        }
    }

    private void PlayerState(Dictionary<string, object> data)
    {
        string playerId = data["playerId"].ToString();
        if (playerId == UserData.Instance.UserId) return;

        // 아직 플레이어가 생성되지 않았다면 생성
        if (!players.ContainsKey(playerId))
        {
            PlayerSpawn(data); // 플레이어를 먼저 생성
        }

        // 플레이어 상태 업데이트
        if (players.TryGetValue(playerId, out NetworkPlayerAnimator player))
        {
            var positionData = JsonConvert.DeserializeObject<Dictionary<string, float>>(data["position"].ToString());
            var rotationData = JsonConvert.DeserializeObject<Dictionary<string, float>>(data["rotation"].ToString());

            Vector3 position = new Vector3(positionData["x"], positionData["y"], positionData["z"]);
            Quaternion rotation = new Quaternion(rotationData["x"], rotationData["y"], rotationData["z"], rotationData["w"]);

            bool isRunning = (bool)data["isRunning"];
            bool isAction = (bool)data["isAction"];
            int currentHealth = Convert.ToInt32(data["currentHealth"]);
            int maxHealth = Convert.ToInt32(data["maxHealth"]);
            int attackPower = Convert.ToInt32(data["attackPower"]);

            player.UpdateState(position, rotation, isRunning, isAction, currentHealth, maxHealth, attackPower);
        }

        
    }

    private void PlayerAction(Dictionary<string, object> data)
    {
        string playerId = data["playerId"].ToString();
        if (playerId == UserData.Instance.UserId || !players.ContainsKey(playerId)) return;

        string actionName = data["actionName"].ToString();
        players[playerId].ExecuteAction(actionName);
    }
}
