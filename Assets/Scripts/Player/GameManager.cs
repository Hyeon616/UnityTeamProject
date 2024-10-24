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

    private void Awake()
    {
        if (Instance == null) Instance = this;
    }

    private void Start()
    {
        SpawnLocalPlayer();
        _ = ListenForNetworkMessages();
    }

    private async void SpawnLocalPlayer()
    {
        var player = Instantiate(playerPrefab, Vector3.zero, Quaternion.identity);
        player.Initialize(UserData.Instance.UserId, true);
        players[UserData.Instance.UserId] = player;

        var spawnData = new
        {
            action = "player_spawn",
            playerId = UserData.Instance.UserId,
            position = Vector3.zero
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
                        HandlePlayerSpawn(data);
                        break;
                    case "player_state":
                        HandlePlayerState(data);
                        break;
                    case "player_action":
                        HandlePlayerAction(data);
                        break;
                }
            }
        }
    }

    private void HandlePlayerSpawn(Dictionary<string, object> data)
    {
        string playerId = data["playerId"].ToString();
        if (playerId == UserData.Instance.UserId || players.ContainsKey(playerId)) return;

        Vector3 position = JsonConvert.DeserializeObject<Vector3>(data["position"].ToString());
        var player = Instantiate(playerPrefab, position, Quaternion.identity);
        player.Initialize(playerId, false);
        players[playerId] = player;
    }

    private void HandlePlayerState(Dictionary<string, object> data)
    {
        string playerId = data["playerId"].ToString();
        if (playerId == UserData.Instance.UserId || !players.ContainsKey(playerId)) return;

        var player = players[playerId];
        Vector3 position = JsonConvert.DeserializeObject<Vector3>(data["position"].ToString());
        Quaternion rotation = JsonConvert.DeserializeObject<Quaternion>(data["rotation"].ToString());
        bool isRunning = (bool)data["isRunning"];
        bool isAction = (bool)data["isAction"];
        int hp = Convert.ToInt32(data["hp"]);

        player.UpdateRemoteState(position, rotation, isRunning, isAction, hp);
    }

    private void HandlePlayerAction(Dictionary<string, object> data)
    {
        string playerId = data["playerId"].ToString();
        if (playerId == UserData.Instance.UserId || !players.ContainsKey(playerId)) return;

        string actionName = data["actionName"].ToString();
        players[playerId].ExecuteRemoteAction(actionName);
    }
}
