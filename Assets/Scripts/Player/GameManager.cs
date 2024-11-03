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
    private async void Start()
    {
        Debug.Log("[GameManager] Scene 시작 - 플레이어 생성 요청");

        // 플레이어 스폰 요청
        var spawnRequest = new
        {
            action = "player_spawn",
            playerId = UserData.Instance.UserId,
            maxHealth = UserData.Instance.Character.MaxHealth,
            attackPower = UserData.Instance.Character.AttackPower
        };

        try
        {
            string jsonRequest = JsonConvert.SerializeObject(spawnRequest);
            Debug.Log($"[GameManager] 스폰 요청 전송: {jsonRequest}");
            await ServerConnector.Instance.SendMessage(jsonRequest);

            isInitialized = true;

            // 네트워크 메시지 리스닝 시작
            _ = ListenNetworkMessages();
        }
        catch (Exception ex)
        {
            Debug.LogError($"[GameManager] 플레이어 생성 요청 실패: {ex.Message}");
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
            Debug.Log($"Spawning player: {playerId} at index: {spawnIndex}");

            if (spawnIndex >= spawnPoints.Count)
            {
                Debug.LogError($"Invalid spawn index: {spawnIndex}");
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
              //  return;
            }

            // 다른 플레이어의 캐릭터인 경우
            if (players.ContainsKey(playerId))
            {
                players[playerId].transform.position = spawnPosition;
                Debug.Log($"Updated existing player {playerId} position");
              //  return;
            }

            // 새로운 다른 플레이어 생성
            var playerObject = Instantiate(playerPrefab, spawnPosition, Quaternion.identity);
            var networkPlayer = playerObject.GetComponent<NetworkPlayerAnimator>();

            // 서버에서 받은 정확한 능력치 값 사용
            int maxHealth = Convert.ToInt32(data["maxHealth"]);
            int attackPower = Convert.ToInt32(data["attackPower"]);

            networkPlayer.Initialize(playerId, false);
            networkPlayer._hp = maxHealth;  // 초기 HP를 maxHealth로 설정
            networkPlayer._str = attackPower;
            players[playerId] = networkPlayer;

            Debug.Log($"Spawned new player {playerId} at index {spawnIndex} with MaxHealth: {maxHealth}, AttackPower: {attackPower}");
        }
        catch (Exception ex)
        {
            Debug.LogError($"Error in PlayerSpawn: {ex.Message}\nStack trace: {ex.StackTrace}");
        }
    }

    private void SpawnLocalPlayer(Vector3 position)
    {
        try
        {
            Debug.Log($"Spawning local player at {position}");

            if (players.ContainsKey(UserData.Instance.UserId))
            {
                Debug.Log("Local player already exists");
                return;
            }

            var playerObject = Instantiate(playerPrefab, position, Quaternion.identity);
            var networkPlayer = playerObject.GetComponent<NetworkPlayerAnimator>();
            networkPlayer.Initialize(UserData.Instance.UserId, true);

            // UserData의 능력치 값 사용
            networkPlayer._hp = UserData.Instance.Character.MaxHealth;
            networkPlayer._str = UserData.Instance.Character.AttackPower;

            players[UserData.Instance.UserId] = networkPlayer;
            LocalPlayer = networkPlayer;

            OnPlayerSpawnCompleted?.Invoke();
            Debug.Log($"Local player spawn completed with MaxHealth: {UserData.Instance.Character.MaxHealth}, AttackPower: {UserData.Instance.Character.AttackPower}");
        }
        catch (Exception ex)
        {
            Debug.LogError($"Error in SpawnLocalPlayer: {ex.Message}\nStack trace: {ex.StackTrace}");
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

        WaitForSeconds wait = new WaitForSeconds(0.25f);

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
        try
        {
            if (!data.ContainsKey("playerId"))
            {
                Debug.LogError("State data missing playerId");
                return;
            }

            string playerId = data["playerId"].ToString();
            if (playerId == UserData.Instance.UserId) return;

            if (!players.ContainsKey(playerId))
            {
                Debug.LogWarning($"Received state for non-existent player: {playerId}");
                return;
            }

            var player = players[playerId];
            if (player == null) return;

            // Position과 Rotation 파싱
            var positionData = JsonConvert.DeserializeObject<Dictionary<string, float>>(
                JsonConvert.SerializeObject(data["position"]));
            var rotationData = JsonConvert.DeserializeObject<Dictionary<string, float>>(
                JsonConvert.SerializeObject(data["rotation"]));

            Vector3 position = new Vector3(positionData["x"], positionData["y"], positionData["z"]);
            Quaternion rotation = new Quaternion(rotationData["x"], rotationData["y"], rotationData["z"], rotationData["w"]);

            // State 파싱
            bool isRunning = data.ContainsKey("isRunning") ? Convert.ToBoolean(data["isRunning"]) : false;
            bool isAction = data.ContainsKey("isAction") ? Convert.ToBoolean(data["isAction"]) : false;

            // Stats 파싱 - 서버에서 받은 정확한 값 사용
            int currentHealth = Convert.ToInt32(data["currentHealth"]);
            int maxHealth = Convert.ToInt32(data["maxHealth"]);
            int attackPower = Convert.ToInt32(data["attackPower"]);

            // Animation 데이터 처리
            if (data.ContainsKey("animation"))
            {
                var animationData = JsonConvert.DeserializeObject<Dictionary<string, object>>(
                    JsonConvert.SerializeObject(data["animation"]));
                if (animationData.ContainsKey("currentTrigger"))
                {
                    string currentTrigger = animationData["currentTrigger"].ToString();
                    if (!string.IsNullOrEmpty(currentTrigger))
                    {
                        player.ExecuteAction(currentTrigger);
                    }
                }
            }

            player.UpdateState(position, rotation, isRunning, isAction, currentHealth, maxHealth, attackPower);
        }
        catch (Exception ex)
        {
            Debug.LogError($"Error in PlayerState: {ex.Message}\nStack trace: {ex.StackTrace}");
        }
    }

    private void PlayerAction(Dictionary<string, object> data)
    {
        try
        {
            string playerId = data["playerId"].ToString();
            if (playerId == UserData.Instance.UserId || !players.ContainsKey(playerId))
            {
                return;
            }

            var player = players[playerId];

            // 위치 정보 업데이트
            if (data.ContainsKey("position") && data.ContainsKey("rotation"))
            {
                var positionData = JsonConvert.DeserializeObject<Dictionary<string, float>>(
                    JsonConvert.SerializeObject(data["position"]));
                var rotationData = JsonConvert.DeserializeObject<Dictionary<string, float>>(
                    JsonConvert.SerializeObject(data["rotation"]));

                Vector3 position = new Vector3(positionData["x"], positionData["y"], positionData["z"]);
                Quaternion rotation = new Quaternion(rotationData["x"], rotationData["y"], rotationData["z"], rotationData["w"]);

                player.transform.position = position;
                player.transform.rotation = rotation;
            }

            string actionName = data["actionName"].ToString();
            player.ExecuteAction(actionName); 

            Debug.Log($"Executed action {actionName} for player {playerId}");
        }
        catch (Exception ex)
        {
            Debug.LogError($"Error in PlayerAction: {ex.Message}\nStack trace: {ex.StackTrace}");
        }
    }
}
