using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }
    public GameObject playerPrefab;
    private Dictionary<string, NetworkPlayerAnimator> players = new Dictionary<string, NetworkPlayerAnimator>();
    public NetworkPlayerAnimator LocalPlayer { get; private set; }

    private Transform startPoint;
    private List<NetworkPlayerAnimator> playerObjects = new List<NetworkPlayerAnimator>();
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
    }
    private async void Start()
    {
        if (SceneManager.GetActiveScene().name == "WaveField")
        {
            GameObject waveFieldMonster = Resources.Load("Monsters/WaveFieldMonsters") as GameObject;
            GameObject monster = Instantiate(waveFieldMonster);
        }
        else if (SceneManager.GetActiveScene().name == "BossField")
        {
            GameObject bossFieldMonster = Resources.Load("Monsters/BossFieldMonsters") as GameObject;
            GameObject monster = Instantiate(bossFieldMonster);

        }
            
        try
        {
            // Scene 시작 시 4개의 프리팹 미리 생성
            for (int i = 0; i < spawnPoints.Count; i++)
            {
                Vector3 spawnPosition = spawnPoints[i].position;
                GameObject playerObject = Instantiate(playerPrefab, spawnPosition, Quaternion.identity);
                var networkPlayer = playerObject.GetComponent<NetworkPlayerAnimator>();
                playerObject.SetActive(false);
                playerObjects.Add(networkPlayer);
            }

            isInitialized = true;
            _ = ListenNetworkMessages();

            // 자신의 스폰 요청을 먼저 보냄
            var spawnRequest = new
            {
                action = "player_spawn",
                playerId = UserData.Instance.UserId,
                maxHealth = UserData.Instance.Character.MaxHealth,
                attackPower = UserData.Instance.Character.AttackPower
            };

            string jsonRequest = JsonConvert.SerializeObject(spawnRequest);
            await ServerConnector.Instance.SendMessage(jsonRequest);

            //var playersInRoom = ServerConnector.Instance.GetPlayersInRoom();
            //foreach (string playerId in playersInRoom)
            //{
                
            //    if (playerId == UserData.Instance.UserId) continue;

            //    var otherSpawnRequest = new
            //    {
            //        action = "player_spawn",
            //        playerId = playerId,
            //        maxHealth = UserData.Instance.Character.MaxHealth,
            //        attackPower = UserData.Instance.Character.AttackPower
            //    };

            //    string otherJsonRequest = JsonConvert.SerializeObject(otherSpawnRequest);
            //    await ServerConnector.Instance.SendMessage(otherJsonRequest);
            //}
        }
        catch (Exception ex)
        {
            Debug.LogError($"[GameManager] Start error: {ex.Message}");
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
            Debug.Log($"[PlayerSpawn] Processing spawn for player: {playerId} at index: {spawnIndex}");

            if (spawnIndex >= playerObjects.Count)
            {
                Debug.LogError($"[PlayerSpawn] Invalid spawn index: {spawnIndex}");
                return;
            }

            if (players.ContainsKey(playerId))
            {
                Debug.Log($"[PlayerSpawn] Player {playerId} already spawned");
                return;
            }

            // 해당 인덱스의 프리팹 활성화 및 초기화
            NetworkPlayerAnimator networkPlayer = playerObjects[spawnIndex];
            networkPlayer.gameObject.SetActive(true);

            int maxHealth = Convert.ToInt32(data["maxHealth"]);
            int attackPower = Convert.ToInt32(data["attackPower"]);

            bool isLocalPlayer = (playerId == UserData.Instance.UserId);
            networkPlayer.Initialize(playerId, isLocalPlayer);
            networkPlayer._hp = maxHealth;
            networkPlayer._str = attackPower;

            players[playerId] = networkPlayer;
            Debug.Log(ServerConnector.Instance.GetPlayersInRoom().Count);
            Debug.Log($"UserData.Instance.UserId : {UserData.Instance.UserId}");
            Debug.Log($"playerId : {playerId}");
            Debug.Log($"isLocalPlayer : {isLocalPlayer}");
            Debug.Log($"playerId : {playerId}");

            if (isLocalPlayer)
            {
                LocalPlayer = networkPlayer;
                StartCoroutine(SendPlayerState());
                OnPlayerSpawnCompleted?.Invoke();
            }

        }
        catch (Exception ex)
        {
            Debug.LogError($"[PlayerSpawn] Error: {ex.Message}\nStack trace: {ex.StackTrace}");
        }
    }

    private IEnumerator SendPlayerState()
    {

        WaitForSeconds wait = new WaitForSeconds(0.1f);

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
           // Debug.Log($"[PlayerState] Full data: {JsonConvert.SerializeObject(data)}");

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

           // Debug.Log($"Rotation data received: {JsonConvert.SerializeObject(rotationData)}");

            Vector3 position = new Vector3(positionData["x"], positionData["y"], positionData["z"]);

            // 회전값 명시적으로 처리
            Quaternion rotation = new Quaternion(
                rotationData.ContainsKey("x") ? rotationData["x"] : 0f,
                rotationData.ContainsKey("y") ? rotationData["y"] : 0f,
                rotationData.ContainsKey("z") ? rotationData["z"] : 0f,
                rotationData.ContainsKey("w") ? rotationData["w"] : 1f
            );

            // State 파싱
            bool isRunning = data.ContainsKey("isRunning") ? Convert.ToBoolean(data["isRunning"]) : false;
            bool isAction = data.ContainsKey("isAction") ? Convert.ToBoolean(data["isAction"]) : false;

            // Stats 파싱
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

          //  Debug.Log($"Updated player {playerId} rotation to: {rotation.eulerAngles}");
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
            Debug.Log($"[PlayerAction] Full data: {JsonConvert.SerializeObject(data)}");

            string playerId = data["playerId"].ToString();
            if (playerId == UserData.Instance.UserId || !players.ContainsKey(playerId))
            {
                return;
            }

            var player = players[playerId];

            string actionName = data["actionName"].ToString();
            Debug.Log($"[PlayerAction] actionName : {actionName}");

            player.ExecuteAction(actionName);

        }
        catch (Exception ex)
        {
            Debug.LogError($"Error in PlayerAction: {ex.Message}\nStack trace: {ex.StackTrace}");
        }
    }
}
