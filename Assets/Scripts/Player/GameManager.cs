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

    public static event Action OnPlayerSpawnCompleted;

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

    private void SpawnPlayers()
    {
        try
        {
            Debug.Log($"[SpawnPlayers] Starting spawn process for player: {UserData.Instance.UserId}");

            var spawnData = new
            {
                action = "player_spawn",
                playerId = UserData.Instance.UserId,
                position = new { x = 0, y = 0, z = 0 }, // 임시 위치, 서버에서 할당된 인덱스로 실제 위치 결정
                maxHealth = UserData.Instance.Character.MaxHealth,
                attackPower = UserData.Instance.Character.AttackPower
            };

            string jsonData = JsonConvert.SerializeObject(spawnData);
            Debug.Log($"[SpawnPlayers] Sending spawn data: {jsonData}");

            _ = ServerConnector.Instance.SendMessage(jsonData);
            isInitialized = true;
        }
        catch (Exception ex)
        {
            Debug.LogError($"[SpawnPlayers] Error: {ex.Message}\n{ex.StackTrace}");
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
        string status = data["status"]?.ToString();

        Debug.Log($"[NetworkMessage] Received: {action}, Status: {status}");
        Debug.Log($"[NetworkMessage] Full data: {JsonConvert.SerializeObject(data)}");

        if (status != "success")
        {
            Debug.LogError($"[NetworkMessage] Failed response: {data["message"]}");
            return;
        }

        try
        {
            switch (action)
            {
                case "player_spawn":
                    if (!data.ContainsKey("playerId") || !data.ContainsKey("spawnIndex"))
                    {
                        Debug.LogError("[NetworkMessage] Missing required spawn data");
                        Debug.Log($"[NetworkMessage] Available keys: {string.Join(", ", data.Keys)}");
                        return;
                    }
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
        catch (Exception ex)
        {
            Debug.LogError($"[NetworkMessage] Error processing {action}: {ex.Message}\n{ex.StackTrace}");
        }
    }

    private void PlayerSpawn(Dictionary<string, object> data)
    {
        try
        {
            string playerId = data["playerId"].ToString();
            int spawnIndex = Convert.ToInt32(data["spawnIndex"]);
            Debug.Log($"[PlayerSpawn] Processing for player: {playerId}, spawn index: {spawnIndex}");

            if (spawnIndex >= spawnPoints.Count)
            {
                Debug.LogError($"[PlayerSpawn] Invalid spawn index: {spawnIndex}");
                return;
            }

            Vector3 spawnPosition = spawnPoints[spawnIndex].position;

            // 자신의 캐릭터인 경우
            if (playerId == UserData.Instance.UserId)
            {
                if (!players.ContainsKey(playerId))
                {
                    SpawnLocalPlayer(spawnPosition);
                }
                return;
            }

            // 다른 플레이어의 캐릭터인 경우
            if (players.ContainsKey(playerId))
            {
                players[playerId].transform.position = spawnPosition;
                Debug.Log($"[PlayerSpawn] Updated existing player {playerId} position");
                return;
            }

            // 새로운 다른 플레이어 생성
            var player = Instantiate(playerPrefab, spawnPosition, Quaternion.identity);
            int maxHealth = Convert.ToInt32(data["maxHealth"]);
            int attackPower = Convert.ToInt32(data["attackPower"]);

            player.Initialize(playerId, false);
            players[playerId] = player;
            Debug.Log($"[PlayerSpawn] Spawned new player {playerId} at index {spawnIndex}");
        }
        catch (Exception ex)
        {
            Debug.LogError($"[PlayerSpawn] Error: {ex.Message}\nData: {JsonConvert.SerializeObject(data)}");
        }
    }

    private void SpawnLocalPlayer(Vector3 position)
    {
        try
        {
            Debug.Log($"[SpawnLocalPlayer] Spawning at {position}");

            if (players.ContainsKey(UserData.Instance.UserId))
            {
                Debug.Log("[SpawnLocalPlayer] Local player already exists");
                return;
            }

            var player = Instantiate(playerPrefab, position, Quaternion.identity);
            player.Initialize(UserData.Instance.UserId, true);
            players[UserData.Instance.UserId] = player;
            LocalPlayer = player;

            OnPlayerSpawnCompleted?.Invoke();

            StartCoroutine(SendPlayerState());
            Debug.Log("[SpawnLocalPlayer] Successfully spawned");
        }
        catch (Exception ex)
        {
            Debug.LogError($"[SpawnLocalPlayer] Error: {ex.Message}\n{ex.StackTrace}");
        }
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
        Debug.Log("[SendPlayerState] Starting state broadcast");

        while (isRunning && LocalPlayer != null)
        {
            try
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
            catch (Exception ex)
            {
                Debug.LogError($"[SendPlayerState] Error: {ex.Message}");
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
