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
        _ = ListenForNetworkMessages();
    }

    private async void SpawnPlayers()
    {
        // 로컬 플레이어 스폰
        Vector3 spawnPosition = GetNextSpawnPosition();
        SpawnLocalPlayer(spawnPosition);

        // 서버에 스폰 알림
        var spawnData = new
        {
            action = "player_spawn",
            playerId = UserData.Instance.UserId,
            position = new { x = spawnPosition.x, y = spawnPosition.y, z = spawnPosition.z },
            maxHealth = UserData.Instance.Character.MaxHealth,
            attackPower = UserData.Instance.Character.AttackPower
        };

        await ServerConnector.Instance.SendMessage(JsonConvert.SerializeObject(spawnData));
    }

    private async Task ListenForNetworkMessages()
    {
        while (true)
        {
            string message = await ServerConnector.Instance.ReadMessage();
            if (!string.IsNullOrEmpty(message))
            {
                var data = JsonConvert.DeserializeObject<Dictionary<string, object>>(message);
                switch (data["action"].ToString())
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
            await Task.Delay(10);
        }
    }

    private void PlayerSpawn(Dictionary<string, object> data)
    {
        string playerId = data["playerId"].ToString();
        if (playerId == UserData.Instance.UserId || players.ContainsKey(playerId)) return;

        var positionData = JsonConvert.DeserializeObject<Dictionary<string, float>>(data["position"].ToString());
        Vector3 position = new Vector3(positionData["x"], positionData["y"], positionData["z"]);

        var player = Instantiate(playerPrefab, position, Quaternion.identity);
        player.Initialize(playerId, false);
        players[playerId] = player;

        Debug.Log($"Remote player spawned: {playerId}");
    }

    private void SpawnLocalPlayer(Vector3 position)
    {
        var player = Instantiate(playerPrefab, position, Quaternion.identity);
        player.Initialize(UserData.Instance.UserId, true);
        players[UserData.Instance.UserId] = player;
        LocalPlayer = player;

        StartCoroutine(SendPlayerState());
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
        if (playerId == UserData.Instance.UserId || !players.ContainsKey(playerId)) return;

        var player = players[playerId];
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

    private void PlayerAction(Dictionary<string, object> data)
    {
        string playerId = data["playerId"].ToString();
        if (playerId == UserData.Instance.UserId || !players.ContainsKey(playerId)) return;

        string actionName = data["actionName"].ToString();
        players[playerId].ExecuteAction(actionName);
    }
}
