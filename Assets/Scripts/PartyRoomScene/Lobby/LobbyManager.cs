using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using Unity.Services.Authentication;
using Unity.Services.Core;
using Unity.Services.Lobbies;
using Unity.Services.Lobbies.Models;
using UnityEngine;
using UnityEngine.Networking;

public class LobbyManager : SceneSingleton<LobbyManager>
{
    public Lobby lobby;
    private Dictionary<string, string> playerNamesCache = new Dictionary<string, string>();

    private Coroutine heartbeatCoroutine;
    private Coroutine refreshLobbyCoroutine;

    public static event Action<Lobby> OnLobbyUpdated;

    private void Start()
    {
        SaveUGSPlayerID(UserData.Instance.UserId, AuthenticationService.Instance.PlayerId);
    }

    private async void SaveUGSPlayerID(string userId, string ugsPlayerId)
    {
        string url = $"{RemoteConfigManager.ServerUrl}/api/save-ugs-playerid";
        SaveUGSPlayerIDRequest requestBody = new SaveUGSPlayerIDRequest
        {
            UserID = userId,
            UGSPlayerID = ugsPlayerId
        };

        string jsonData = JsonUtility.ToJson(requestBody);

        using (UnityWebRequest request = new UnityWebRequest(url, "POST"))
        {
            byte[] bodyRaw = System.Text.Encoding.UTF8.GetBytes(jsonData);
            request.uploadHandler = new UploadHandlerRaw(bodyRaw);
            request.downloadHandler = new DownloadHandlerBuffer();
            request.SetRequestHeader("Content-Type", "application/json");

            await request.SendWebRequestAsync();

            if (request.result == UnityWebRequest.Result.Success)
            {
                Debug.Log("UGS Player ID saved successfully");
            }
            else
            {
                Debug.LogError($"Error saving UGS Player ID: {request.error}");
            }
        }
    }

    public async Task<Lobby> CreateLobby(string lobbyName, int maxPlayers, Dictionary<string, DataObject> lobbyData)
    {
        Dictionary<string, PlayerDataObject> playerDataObjects = new Dictionary<string, PlayerDataObject>();

        if (UserData.Instance != null && UserData.Instance.Character != null)
        {
            playerDataObjects["PlayerName"] = new PlayerDataObject(PlayerDataObject.VisibilityOptions.Member, UserData.Instance.Character.PlayerName);
            Debug.Log($"Setting player data: {UserData.Instance.Character.PlayerName}");
        }

        Player player = new Player(AuthenticationService.Instance.PlayerId, null, playerDataObjects);

        // 추가 데이터 객체 초기화
        lobbyData["GameStart"] = new DataObject(DataObject.VisibilityOptions.Member, "false");
        lobbyData["SceneName"] = new DataObject(DataObject.VisibilityOptions.Member, "");

        CreateLobbyOptions options = new CreateLobbyOptions
        {
            Data = lobbyData,
            IsPrivate = false,
            Player = player
        };

        try
        {
            Lobby createdLobby = await LobbyService.Instance.CreateLobbyAsync(lobbyName, maxPlayers, options);

            if (createdLobby != null)
            {
                Debug.Log($"Lobby created with ID: {createdLobby.Id}");
                this.lobby = createdLobby; // 클래스 수준의 로비 변수에 할당
                StartHeartbeat();
                StartRefreshLobby();
                CachePlayerNames(createdLobby);
                return createdLobby;
            }
            else
            {
                Debug.LogError("Lobby creation returned null");
                return null;
            }
        }
        catch (Exception ex)
        {
            Debug.LogError($"Failed to create lobby: {ex.Message}");
            Debug.LogError($"Stack Trace: {ex.StackTrace}");
            return null;
        }
    }

    //public async Task<Lobby> CreateLobby(string lobbyName, int maxPlayers)
    //{
    //    CreateLobbyOptions options = new CreateLobbyOptions
    //    {
    //        IsPrivate = false,
    //        Data = new Dictionary<string, DataObject>
    //        {
    //            // 예: 초기 데이터를 설정하려면 여기에 추가
    //            // { "exampleKey", new DataObject(DataObject.VisibilityOptions.Member, "exampleValue") }
    //        }
    //    };

    //    try
    //    {
    //        lobby = await LobbyService.Instance.CreateLobbyAsync(lobbyName, maxPlayers, options);
    //        Debug.Log($"Lobby created with ID: {lobby.Id}");

    //        StartHeartbeat();
    //        StartRefreshLobby();

    //        return lobby;
    //    }
    //    catch (Exception ex)
    //    {
    //        Debug.LogError($"Failed to create lobby: {ex.Message}");
    //        return null;
    //    }
    //}



    public async Task<List<Lobby>> GetLobbies()
    {
        try
        {
            var queryResponse = await LobbyService.Instance.QueryLobbiesAsync();
            return queryResponse.Results;
        }
        catch (RequestFailedException ex)
        {

            // 특정 오류 코드 처리
            if (ex.ErrorCode == 401)
            {
                Debug.LogError("Unauthorized access - please check your authentication settings.");
            }
            return new List<Lobby>();
        }
        catch (Exception ex)
        {
            Debug.LogError($"An unexpected error occurred: {ex.Message}");
            return new List<Lobby>();
        }
    }

    public async Task<bool> JoinLobby(string lobbyId, Dictionary<string, PlayerDataObject> playerData)
    {
        var options = new JoinLobbyByIdOptions
        {
            Player = new Player(AuthenticationService.Instance.PlayerId, null, playerData)
        };

        try
        {
            lobby = await LobbyService.Instance.JoinLobbyByIdAsync(lobbyId, options);
            Debug.Log($"Joined lobby with ID: {lobby.Id}");

            StartHeartbeat();
            StartRefreshLobby();

            if (lobby != null)
            {
                CachePlayerNames(lobby);
            }
            else
            {
                Debug.LogError("Lobby is null after joining.");
            }

            return true;
        }
        catch (Exception ex)
        {
            Debug.LogError($"Failed to join lobby: {ex.Message}");
            return false;
        }
    }

    //public async Task<bool> JoinLobby(string lobbyId)
    //{
    //    try
    //    {
    //        Dictionary<string, PlayerDataObject> playerData = new Dictionary<string, PlayerDataObject>();
    //        if (UserData.Instance != null && UserData.Instance.Character != null)
    //        {
    //            playerData["PlayerName"] = new PlayerDataObject(PlayerDataObject.VisibilityOptions.Member, UserData.Instance.Character.PlayerName);
    //        }

    //        var options = new JoinLobbyByIdOptions
    //        {
    //            Player = new Player(AuthenticationService.Instance.PlayerId, null, playerData)
    //        };


    //        lobby = await LobbyService.Instance.JoinLobbyByIdAsync(lobbyId);
    //        Debug.Log($"Joined lobby with ID: {lobby.Id}");

    //        StartHeartbeat();
    //        StartRefreshLobby();

    //        if (lobby != null)
    //        {
    //            CachePlayerNames(lobby);
    //        }
    //        else
    //        {
    //            Debug.LogError("Lobby is null after joining.");
    //        }

    //        return true;
    //    }
    //    catch (Exception ex)
    //    {
    //        Debug.LogError($"Failed to join lobby: {ex.Message}");
    //        return false;
    //    }
    //}

    public async Task<bool> LeaveLobby()
    {
        try
        {
            if (lobby != null)
            {
                await LobbyService.Instance.RemovePlayerAsync(lobby.Id, AuthenticationService.Instance.PlayerId);
                lobby = null;
                StopHeartbeat();
                StopRefreshLobby();
                return true;
            }
            return false;
        }
        catch (Exception ex)
        {
            Debug.LogError($"Failed to leave lobby: {ex.Message}");
            return false;
        }
    }

    private void StartHeartbeat()
    {
        if (heartbeatCoroutine != null)
        {
            StopCoroutine(heartbeatCoroutine);
        }

        if (lobby == null)
        {
            Debug.LogError("Lobby is null in StartHeartbeat.");
            return;
        }

        if (AuthenticationService.Instance.PlayerId == lobby.HostId)
        {
            heartbeatCoroutine = StartCoroutine(HeartbeatLobbyCoroutine(lobby.Id, 15f));
        }
        else
        {
            Debug.LogWarning("Only the host can send heartbeat.");
        }
    }

    private void StopHeartbeat()
    {
        if (heartbeatCoroutine != null)
        {
            StopCoroutine(heartbeatCoroutine);
            heartbeatCoroutine = null;
        }
    }

    private IEnumerator HeartbeatLobbyCoroutine(string lobbyId, float waitTimeSeconds)
    {
        while (true)
        {
            var sendHeartbeatTask = SendHeartbeatPingAsync(lobbyId);
            yield return new WaitUntil(() => sendHeartbeatTask.IsCompleted);

            if (sendHeartbeatTask.IsFaulted)
            {
                Debug.LogError($"Failed to send heartbeat: {sendHeartbeatTask.Exception}");
            }

            yield return new WaitForSecondsRealtime(waitTimeSeconds);
        }
    }

    private async Task SendHeartbeatPingAsync(string lobbyId)
    {
        try
        {
            await LobbyService.Instance.SendHeartbeatPingAsync(lobbyId);
        }
        catch (Exception ex)
        {
            Debug.LogError($"Failed to send heartbeat: {ex.Message}");
        }
    }

    private void StartRefreshLobby()
    {
        if (refreshLobbyCoroutine != null)
        {
            StopCoroutine(refreshLobbyCoroutine);
        }

        if (lobby == null)
        {
            Debug.LogError("Lobby is null in StartRefreshLobby.");
            return;
        }

        refreshLobbyCoroutine = StartCoroutine(RefreshLobbyCoroutine(lobby.Id, 5f));
    }

    private void StopRefreshLobby()
    {
        if (refreshLobbyCoroutine != null)
        {
            StopCoroutine(refreshLobbyCoroutine);
            refreshLobbyCoroutine = null;
        }
    }

    private IEnumerator RefreshLobbyCoroutine(string lobbyId, float waitTimeSeconds)
    {
        while (true)
        {
            yield return new WaitForSecondsRealtime(waitTimeSeconds);
            var refreshTask = RefreshLobbyAsync(lobbyId);
            yield return new WaitUntil(() => refreshTask.IsCompleted);

            if (refreshTask.IsFaulted)
            {
                Debug.LogError($"Failed to refresh lobby: {refreshTask.Exception}");
            }
        }
    }
    private async Task RefreshLobbyAsync(string lobbyId)
    {
        try
        {
            Debug.Log($"Attempting to refresh lobby with ID: {lobbyId}");
            Lobby newLobby = await LobbyService.Instance.GetLobbyAsync(lobbyId);
            Debug.Log("GetLobbyAsync call completed.");

            if (newLobby == null)
            {
                Debug.LogError("Failed to refresh lobby: New lobby is null.");
                return;
            }

            if (newLobby.Data == null)
            {
                Debug.LogError("Failed to refresh lobby: New lobby data is null.");
                return;
            }

            if (newLobby.LastUpdated > lobby.LastUpdated)
            {
                lobby = newLobby;
                OnLobbyUpdated?.Invoke(lobby);
                Debug.Log("Lobby successfully refreshed.");
            }
            else
            {
                Debug.Log("Lobby data is up-to-date. No need to refresh.");
            }
        }
        catch (LobbyServiceException ex)
        {
            if (ex.ErrorCode == 16001)
            {
                Debug.LogWarning("Lobby not found. It might have been deleted or expired.");
            }
            else
            {
                Debug.LogError($"Failed to refresh lobby: {ex.Message} (Error Code: {ex.ErrorCode})");
            }
        }
        catch (Exception ex)
        {
            Debug.LogError($"Failed to refresh lobby: {ex.Message}");
        }
    }


    private void CachePlayerNames(Lobby lobby)
    {
        if (lobby == null)
        {
            Debug.LogError("Lobby is null in CachePlayerNames.");
            return;
        }

        if (lobby.Players == null)
        {
            Debug.LogError("Lobby players are null in CachePlayerNames.");
            return;
        }

        foreach (var player in lobby.Players)
        {
            string playerName = "Unknown";
            if (player.Data != null && player.Data.ContainsKey("PlayerName"))
            {
                playerName = player.Data["PlayerName"].Value;
                Debug.Log($"Player {player.Id} has name: {playerName}");
            }
            else
            {
                Debug.LogWarning($"Player {player.Id} does not have a PlayerName in their data.");
            }

            CachePlayerName(player.Id, playerName);
        }
    }

    public void CachePlayerName(string playerId, string playerName)
    {
        if (!playerNamesCache.ContainsKey(playerId))
        {
            playerNamesCache.Add(playerId, playerName);
            Debug.Log($"Cached player name: {playerId} -> {playerName}");
        }
    }

    public string GetCachedPlayerName(string playerId)
    {
        if (playerNamesCache.TryGetValue(playerId, out var playerName))
        {
            return playerName;
        }
        else
        {
            Debug.LogWarning($"Player ID {playerId} not found in cache.");
            return "Unknown";
        }
    }

    private bool ValidateLobbyData(Lobby lobby)
    {
        if (lobby == null)
        {
            Debug.LogError("Lobby is null.");
            return false;
        }

        if (lobby.Data == null)
        {
            Debug.LogError("Lobby data dictionary is null.");
            return false;
        }

        return true;
    }

    public async Task<bool> SetGameStartFlag(string sceneName)
    {
        try
        {
            var updatedData = new Dictionary<string, DataObject>
            {
                ["GameStart"] = new DataObject(lobby.Data["GameStart"].Visibility, "true"),
                ["SceneName"] = new DataObject(lobby.Data["SceneName"].Visibility, sceneName)
            };

            await LobbyService.Instance.UpdateLobbyAsync(lobby.Id, new UpdateLobbyOptions { Data = updatedData });
            return true;
        }
        catch (Exception ex)
        {
            Debug.LogError($"Failed to set game start flag: {ex.Message}");
            return false;
        }
    }

    public async Task<bool> UpdateLobbyData(Dictionary<string, DataObject> updatedData)
    {
        try
        {
            lobby = await LobbyService.Instance.UpdateLobbyAsync(lobby.Id, new UpdateLobbyOptions { Data = updatedData });
            OnLobbyUpdated?.Invoke(lobby);
            return true;
        }
        catch (Exception ex)
        {
            Debug.LogError($"Failed to update lobby data: {ex.Message}");
            return false;
        }
    }


}