using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using TMPro;
using Unity.Services.Core;
using Unity.Services.Lobbies;
using Unity.Services.Lobbies.Models;
using UnityEngine;
using UnityEngine.UI;

public class LobbyController : MonoBehaviour
{
    [Header("Main")]
    [SerializeField] private GameObject mainLobbyUI;
    [Header("Status")]
    [SerializeField] private GameObject statusEnhancementUI;
    [Header("Equip")]
    [SerializeField] private GameObject equipEnhancementUI;
    [Header("Stat")]
    [SerializeField] private GameObject statUI;
    [Header("Setting")]
    [SerializeField] private GameObject settingUI;

    [Header("GameMenu")]
    [SerializeField] private GameObject gameStartUI;

    [Header("PlayerName")]
    [SerializeField] private Button playerNameButton;
    [SerializeField] private TextMeshProUGUI nameText;
    private bool isName = true;

    [Header("Main Button")]
    [SerializeField] private Button backToMainButton;
    [SerializeField] private Button equipButton;
    [SerializeField] private Button statButton;
    [SerializeField] private Button gameStartButton;

    [Header("Game Start")]
    [SerializeField] private Button closeGameStartUIButton;
    [SerializeField] private Button createRoomButton;
    [SerializeField] private Button joinRoomButton;

    [Header("Lobby")]
    [SerializeField] private Button backToGameStart;
    [SerializeField] private GameObject JoinMenuUI;
    [SerializeField] private GameObject lobbyRoomUI;
    [SerializeField] private Button lobbyRoomCodeSubmit;
    [SerializeField] private Button lobbyRoomConnect;
    [SerializeField] private TMP_InputField lobbyRoomCodeInputField;

    [Header("Equip Enhancement")]
    [SerializeField] private Button weaponEnhancement;
    [SerializeField] private Button weaponDiminishment;
    [SerializeField] private Button armorEnhancement;
    [SerializeField] private Button armorDiminishment;

    [Header("Stat Enhancement")]
    [SerializeField] private Button attackEnhancement;
    [SerializeField] private Button attackDiminishment;
    [SerializeField] private Button healthEnhancement;
    [SerializeField] private Button healthDiminishment;

    [Header("Map")]
    [SerializeField] private Image mapImage;
    [SerializeField] private Button leftButton;
    [SerializeField] private Button rightButton;
    [SerializeField] private TextMeshProUGUI mapName;
    [SerializeField] private MapSelectionData mapSelectionData;

    [Header("Room")]
    [SerializeField] private Transform LobbyRoomListContent;
    [SerializeField] private Transform LobbyRoomPlayerListContent;
    [SerializeField] private GameObject LobbyRoomListPrefab;
    [SerializeField] private GameObject LobbyPlayerNamePrefab;
    [SerializeField] private Button leaveRoomButton;
    [SerializeField] private Button startSceneButton;
    [SerializeField] private Button Button_JoinRoom;


    private string selectedLobbyId;
    private int currentMapIndex = 0;

    private void OnEnable()
    {

        RegisterListeners();
        LobbyEvents.OnLobbyUpdated += OnLobbyUpdated;
    }

    private void OnDisable()
    {
        UnregisterListeners();
        LobbyEvents.OnLobbyUpdated -= OnLobbyUpdated;
    }


    private void RegisterListeners()
    {
        // GameStart
        AddListener(gameStartButton, GameStartEnter);

        // GameStartUI
        AddListener(closeGameStartUIButton, OnClickGameStartExit);
        AddListener(createRoomButton, OnClickedCreateRoomUIButton);
        AddListener(joinRoomButton, OnClickedJoinRoomUIButton);

        // Lobby
        AddListener(backToGameStart, BackToGameStartUI);
        AddListener(lobbyRoomCodeSubmit, RoomCodeSubmit);
        AddListener(lobbyRoomConnect, LobbyRoomConnect);


        // Enhancement
        AddListener(equipButton, EquipEnhancementUI);
        AddListener(statButton, StatusEnhancementUI);

        // Back To Main
        AddListener(backToMainButton, BackToMainUI);

        // PlayerName
        AddListener(playerNameButton, ChangePlayerNameToPlayerID);

        // Equip Enhancement
        AddListener(weaponEnhancement, JewelUpGradeATK);
        AddListener(weaponDiminishment, JewelDownGradeATK);
        AddListener(armorEnhancement, JewelUpGradeHP);
        AddListener(armorDiminishment, JewelDownGradeHP);

        // Stat Enhancement
        AddListener(attackEnhancement, CoinUpGradeATK);
        AddListener(attackDiminishment, CoinDownGradeATK);
        AddListener(healthEnhancement, CoinUpGradeHP);
        AddListener(healthDiminishment, CoinDownGradeHP);

        // Map
        AddListener(leftButton, OnLeftButtonClicked);
        AddListener(rightButton, OnRightButtonClicked);

        // Room
        AddListener(leaveRoomButton, OnClickedLeaveRoomButton);
        AddListener(startSceneButton, OnStartGame);
        AddListener(Button_JoinRoom, JoinRoomButtonClicked);

    }

    private void UnregisterListeners()
    {
        // GameStart
        RemoveListener(gameStartButton, GameStartEnter);

        // GameStartUI
        RemoveListener(closeGameStartUIButton, OnClickGameStartExit);
        RemoveListener(createRoomButton, OnClickedCreateRoomUIButton);
        RemoveListener(joinRoomButton, OnClickedJoinRoomUIButton);

        // Lobby
        RemoveListener(backToGameStart, BackToGameStartUI);
        RemoveListener(lobbyRoomCodeSubmit, RoomCodeSubmit);
        RemoveListener(lobbyRoomConnect, LobbyRoomConnect);

        // Enhancement
        RemoveListener(equipButton, EquipEnhancementUI);
        RemoveListener(statButton, StatusEnhancementUI);

        // Back To Main
        RemoveListener(backToMainButton, BackToMainUI);

        // PlayerName
        RemoveListener(playerNameButton, ChangePlayerNameToPlayerID);

        // Equip Enhancement
        RemoveListener(weaponEnhancement, JewelUpGradeATK);
        RemoveListener(weaponDiminishment, JewelDownGradeATK);
        RemoveListener(armorEnhancement, JewelUpGradeHP);
        RemoveListener(armorDiminishment, JewelDownGradeHP);

        // Stat Enhancement
        RemoveListener(attackEnhancement, CoinUpGradeATK);
        RemoveListener(attackDiminishment, CoinDownGradeATK);
        RemoveListener(healthEnhancement, CoinUpGradeHP);
        RemoveListener(armorDiminishment, CoinDownGradeHP);

        // Map

        RemoveListener(leftButton, OnLeftButtonClicked);
        RemoveListener(rightButton, OnRightButtonClicked);

        // Room
        RemoveListener(leaveRoomButton, OnClickedLeaveRoomButton);
        RemoveListener(startSceneButton, OnStartGame);
        RemoveListener(Button_JoinRoom, JoinRoomButtonClicked);

    }

    #region Listener 함수
    private void AddListener(Button button, UnityEngine.Events.UnityAction action)
    {
        if (button != null)
        {
            button.onClick.AddListener(action);
        }
        else
        {
            Debug.LogWarning("Button is not assigned.");
        }
    }

    private void RemoveListener(Button button, UnityEngine.Events.UnityAction action)
    {
        if (button != null)
        {
            button.onClick.RemoveListener(action);
        }
    }
    #endregion

    #region MainUI Methods
    private void EquipEnhancementUI()
    {
        mainLobbyUI.SetActive(false);
        equipEnhancementUI.SetActive(true);
        statUI.SetActive(true);
    }

    private void StatusEnhancementUI()
    {
        mainLobbyUI.SetActive(false);
        statusEnhancementUI.SetActive(true);
        statUI.SetActive(true);
    }

    private void BackToMainUI()
    {
        equipEnhancementUI.SetActive(false);
        statusEnhancementUI.SetActive(false);
        statUI.SetActive(false);
        mainLobbyUI.SetActive(true);
    }

    private void GameStartEnter()
    {
        gameStartUI.SetActive(true);
        settingUI.SetActive(false);
    }

    private async Task CheckForGameStart()
    {
        while (true)
        {
            await Task.Delay(1000);

            if (LobbyManager.Instance.lobby != null && LobbyManager.Instance.lobby.Data.TryGetValue("GameStart", out var gameStartData) && gameStartData.Value == "true")
            {
                if (LobbyManager.Instance.lobby.Data.TryGetValue("SceneName", out var sceneNameData))
                {
                    string sceneName = sceneNameData.Value;
                    SceneLoader.Instance.LoadSceneAsync(sceneName);
                }
            }
        }
    }

    private void OnClickGameStartExit()
    {
        gameStartUI.SetActive(false);
        settingUI.SetActive(true);
    }

    private void BackToGameStartUI()
    {
        if (JoinMenuUI.activeSelf)
            JoinMenuUI.SetActive(false);
        if (lobbyRoomUI.activeSelf)
            lobbyRoomUI.SetActive(false);
        createRoomButton.gameObject.SetActive(true);
        joinRoomButton.gameObject.SetActive(true);
        backToGameStart.gameObject.SetActive(false);
    }
    #endregion

    #region Enhancement Methods
    public void JewelUpGradeATK()
    {
        if (UserData.Instance.Character.Gems > UserData.Instance.Character.AttackEnhancement)
        {
            UserData.Instance.Character.AttackPower++;
            UserData.Instance.Character.Gems -= UserData.Instance.Character.AttackEnhancement;
            UserData.Instance.Character.AttackEnhancement++;
            UserData.Instance.SavePlayerData();
            Debug.Log("Jewel upgraded ATK");
        }
        else
        {
            Debug.LogWarning("Not enough gems to upgrade ATK");
        }
    }

    public void JewelDownGradeATK()
    {
        if (UserData.Instance.Character.AttackPower > 0 && UserData.Instance.Character.AttackEnhancement > 0)
        {
            UserData.Instance.Character.AttackPower--;
            UserData.Instance.Character.Gems += (UserData.Instance.Character.AttackEnhancement - 1);
            UserData.Instance.Character.AttackEnhancement--;
            UserData.Instance.SavePlayerData();
            Debug.Log("Jewel downgraded ATK");
        }
        else
        {
            Debug.LogWarning("Cannot downgrade ATK");
        }
    }

    public void JewelUpGradeHP()
    {
        if (UserData.Instance.Character.Gems > UserData.Instance.Character.HealthEnhancement * 5)
        {
            UserData.Instance.Character.MaxHealth += 5;
            UserData.Instance.Character.Gems -= UserData.Instance.Character.HealthEnhancement * 5;
            UserData.Instance.Character.HealthEnhancement++;
            UserData.Instance.SavePlayerData();
            Debug.Log("Jewel upgraded HP");
        }
        else
        {
            Debug.LogWarning("Not enough gems to upgrade HP");
        }
    }

    public void JewelDownGradeHP()
    {
        if (UserData.Instance.Character.MaxHealth > 0 && UserData.Instance.Character.HealthEnhancement > 0)
        {
            UserData.Instance.Character.MaxHealth -= 5;
            UserData.Instance.Character.Gems += (UserData.Instance.Character.HealthEnhancement - 1) * 5;
            UserData.Instance.Character.HealthEnhancement--;
            UserData.Instance.SavePlayerData();
            Debug.Log("Jewel downgraded HP");
        }
        else
        {
            Debug.LogWarning("Cannot downgrade HP");
        }
    }

    public void CoinUpGradeATK()
    {
        if (UserData.Instance.Character.Coins > UserData.Instance.Character.AttackEnhancement * 5)
        {
            UserData.Instance.Character.AttackPower++;
            UserData.Instance.Character.Coins -= UserData.Instance.Character.AttackEnhancement * 5;
            UserData.Instance.Character.AttackEnhancement++;
            UserData.Instance.SavePlayerData();
            Debug.Log("Coin upgraded ATK");
        }
        else
        {
            Debug.LogWarning("Not enough coins to upgrade ATK");
        }
    }

    public void CoinDownGradeATK()
    {
        if (UserData.Instance.Character.AttackPower > 0 && UserData.Instance.Character.AttackEnhancement > 0)
        {
            UserData.Instance.Character.AttackPower--;
            UserData.Instance.Character.Coins += (UserData.Instance.Character.AttackEnhancement - 1) * 5;
            UserData.Instance.Character.AttackEnhancement--;
            UserData.Instance.SavePlayerData();
            Debug.Log("Coin downgraded ATK");
        }
        else
        {
            Debug.LogWarning("Cannot downgrade ATK");
        }
    }

    public void CoinUpGradeHP()
    {
        if (UserData.Instance.Character.Coins > UserData.Instance.Character.HealthEnhancement * 5)
        {
            UserData.Instance.Character.MaxHealth += 5;
            UserData.Instance.Character.Coins -= UserData.Instance.Character.HealthEnhancement * 5;
            UserData.Instance.Character.HealthEnhancement++;
            UserData.Instance.SavePlayerData();
            Debug.Log("Coin upgraded HP");
        }
        else
        {
            Debug.LogWarning("Not enough coins to upgrade HP");
        }
    }

    public void CoinDownGradeHP()
    {
        if (UserData.Instance.Character.MaxHealth > 0 && UserData.Instance.Character.HealthEnhancement > 0)
        {
            UserData.Instance.Character.MaxHealth -= 5;
            UserData.Instance.Character.Coins += (UserData.Instance.Character.HealthEnhancement - 1) * 5;
            UserData.Instance.Character.HealthEnhancement--;
            UserData.Instance.SavePlayerData();
            Debug.Log("Coin downgraded HP");
        }
        else
        {
            Debug.LogWarning("Cannot downgrade HP");
        }
    }
    #endregion
    private async void Start()
    {
        await RefreshLobbyList();
        await RefreshPlayerList();
    }
    #region Player Info
    private void ChangePlayerNameToPlayerID()
    {
        isName = !isName;

        if (UserData.Instance != null && UserData.Instance.Character != null)
        {
            nameText.text = isName ? UserData.Instance.Character.PlayerName : UserData.Instance.Character.PlayerId;
        }
        else
        {
            nameText.text = "Unknown";
            Debug.LogError("Character data is not loaded in UserData.");
        }
    }
    #endregion

    #region Lobby Management
    private void OnClickedCreateRoomUIButton()
    {
        createRoomButton.gameObject.SetActive(false);
        joinRoomButton.gameObject.SetActive(false);
        Button_JoinRoom.gameObject.SetActive(false);
        JoinMenuUI.SetActive(true);
        backToGameStart.gameObject.SetActive(true);
        startSceneButton.gameObject.SetActive(true);
    }

    private async void OnClickedJoinRoomUIButton()
    {
        createRoomButton.gameObject.SetActive(false);
        joinRoomButton.gameObject.SetActive(false);
        lobbyRoomUI.SetActive(true);
        backToGameStart.gameObject.SetActive(true);
        startSceneButton.gameObject.SetActive(false);
        Button_JoinRoom.gameObject.SetActive(true);
        await RefreshLobbyList();
        await RefreshPlayerList();
    }

    private async void OnClickedLeaveRoomButton()
    {
        backToGameStart.gameObject.SetActive(true);
        closeGameStartUIButton.gameObject.SetActive(true);

        Debug.Log("Leaving room...");

        if (LobbyManager.Instance.lobby != null)
        {
            try
            {
                bool success = await LobbyManager.Instance.LeaveLobby();

                if (success)
                {

                    ClearPlayerListUI();
                    UpdateUIAfterLeavingRoom();
                    await RefreshLobbyList();
                    await RefreshPlayerList();
                    Debug.Log("Room left successfully.");
                }
                else
                {
                    Debug.LogError("Failed to leave the lobby.");
                }
            }
            catch (Exception ex)
            {
                Debug.LogError($"Failed to leave room: {ex.Message}");
            }
        }
        else
        {
            Debug.LogWarning("Not currently in any room.");
        }
    }


    private void ClearPlayerListUI()
    {
        if (LobbyRoomPlayerListContent != null)
        {
            foreach (Transform child in LobbyRoomPlayerListContent)
            {
                if (child != null)
                {
                    Destroy(child.gameObject);
                }
            }
        }
        else
        {
            Debug.LogWarning("LobbyRoomPlayerListContent is null. Cannot clear player list UI.");
        }
    }

    private void UpdateUIAfterLeavingRoom()
    {
        JoinMenuUI.SetActive(false);
        lobbyRoomUI.SetActive(false);
        leaveRoomButton.gameObject.SetActive(false);
        Button_JoinRoom.gameObject.SetActive(false);
        backToGameStart.gameObject.SetActive(true);
        closeGameStartUIButton.gameObject.SetActive(true);
        createRoomButton.gameObject.SetActive(true);
        joinRoomButton.gameObject.SetActive(true);
    }

    private async void RoomCodeSubmit()
    {
        Debug.Log("Creating room...");

        if (LobbyManager.Instance == null)
        {
            Debug.LogError("LobbyManager.Instance is null");
            return;
        }

        string sceneName = string.IsNullOrEmpty(lobbyRoomCodeInputField.text) ? "파티사냥 하실분" : lobbyRoomCodeInputField.text;

        Dictionary<string, DataObject> lobbyData = new Dictionary<string, DataObject>()
    {
        { "MapIndex", new DataObject(DataObject.VisibilityOptions.Public, $"{currentMapIndex}") },
        { "SceneName", new DataObject(DataObject.VisibilityOptions.Public, sceneName) },
        { "GameStart", new DataObject(DataObject.VisibilityOptions.Member, "false") }
    };

        try
        {
            var lobby = await LobbyManager.Instance.CreateLobby(sceneName, 3, lobbyData);
            if (lobby != null)
            {
                Debug.Log("Room created successfully.");
                Debug.Log($"Lobby ID: {lobby.Id}");
                Debug.Log($"Lobby Name: {lobby.Name}");

                if (JoinMenuUI == null)
                {
                    Debug.LogError("JoinMenuUI is null");
                }
                if (lobbyRoomUI == null)
                {
                    Debug.LogError("lobbyRoomUI is null");
                }
                if (backToGameStart == null)
                {
                    Debug.LogError("backToGameStart is null");
                }
                if (closeGameStartUIButton == null)
                {
                    Debug.LogError("closeGameStartUIButton is null");
                }
                if (leaveRoomButton == null)
                {
                    Debug.LogError("leaveRoomButton is null");
                }


                Debug.Log("Calling UpdateUIAfterRoomCreation");
                UpdateUIAfterRoomCreation();

                Debug.Log("Calling RefreshLobbyList");
                await RefreshLobbyList();

                Debug.Log("Calling RefreshPlayerList");
                await RefreshPlayerList();
            }
            else
            {
                Debug.LogError("Failed to create lobby.");
            }
        }
        catch (Exception ex)
        {
            Debug.LogError($"Failed to create lobby: {ex.Message}");
            Debug.LogError($"Stack Trace: {ex.StackTrace}");
        }
    }

    private void Update()
    {
        if (LobbyManager.Instance.lobby != null && LobbyManager.Instance.lobby.Data.ContainsKey("GameStart"))
        {
            if (LobbyManager.Instance.lobby.Data["GameStart"].Value == "true")
            {
                string sceneName = LobbyManager.Instance.lobby.Data["SceneName"].Value;
                LoadGameScene(sceneName);
            }
        }
    }




    private async Task JoinLobby(string lobbyId)
    {
        Debug.Log($"Joining lobby with ID: {lobbyId}");

        Dictionary<string, PlayerDataObject> playerData = new Dictionary<string, PlayerDataObject>();
        if (UserData.Instance != null && UserData.Instance.Character != null)
        {
            playerData["PlayerName"] = new PlayerDataObject(PlayerDataObject.VisibilityOptions.Member, UserData.Instance.Character.PlayerName);
            Debug.Log($"Setting player data for join: {UserData.Instance.Character.PlayerName}");
        }

        bool success = await LobbyManager.Instance.JoinLobby(lobbyId, playerData);
        if (success)
        {
            Debug.Log("Room joined successfully.");
            await RefreshPlayerList();
        }
        else
        {
            Debug.LogError("Failed to join room.");
        }
    }

    private void UpdateUIAfterRoomCreation()
    {
        try
        {
            if (JoinMenuUI == null)
                throw new NullReferenceException("JoinMenuUI is not assigned.");
            if (lobbyRoomUI == null)
                throw new NullReferenceException("lobbyRoomUI is not assigned.");
            if (backToGameStart == null)
                throw new NullReferenceException("backToGameStart is not assigned.");
            if (closeGameStartUIButton == null)
                throw new NullReferenceException("closeGameStartUIButton is not assigned.");
            if (leaveRoomButton == null)
                throw new NullReferenceException("leaveRoomButton is not assigned.");

            JoinMenuUI.SetActive(false);
            lobbyRoomUI.SetActive(true);
            backToGameStart.gameObject.SetActive(false);
            closeGameStartUIButton.gameObject.SetActive(false);
            leaveRoomButton.gameObject.SetActive(true);

            Debug.Log("UI updated after room creation.");
        }
        catch (Exception ex)
        {
            Debug.LogError($"UpdateUIAfterRoomCreation failed: {ex.Message}");
            Debug.LogError($"Stack Trace: {ex.StackTrace}");
        }
    }

    private void SelectLobby(string lobbyId)
    {
        selectedLobbyId = lobbyId;
        DisplayPlayerList(lobbyId);
    }

    // 방 참가
    private async void JoinRoomButtonClicked()
    {
        if (string.IsNullOrEmpty(selectedLobbyId))
        {
            Debug.LogWarning("No lobby selected to join.");
            return;
        }

        // 플레이어 데이터를 설정합니다.
        Dictionary<string, PlayerDataObject> playerData = new Dictionary<string, PlayerDataObject>();
        if (UserData.Instance != null && UserData.Instance.Character != null)
        {
            playerData["PlayerName"] = new PlayerDataObject(PlayerDataObject.VisibilityOptions.Member, UserData.Instance.Character.PlayerName);
            Debug.Log($"Setting player data for join: {UserData.Instance.Character.PlayerName}");
        }

        bool success = await LobbyManager.Instance.JoinLobby(selectedLobbyId, playerData);
        if (success)
        {
            Debug.Log("Room joined successfully.");
            await RefreshPlayerList();
        }
        else
        {
            Debug.LogError("Failed to join room.");
        }
    }

    private async void DisplayPlayerList(string lobbyId)
    {
        try
        {
            Debug.Log($"Attempting to retrieve lobby with ID: {lobbyId}");
            Lobby selectedLobby = await LobbyService.Instance.GetLobbyAsync(lobbyId);
            Debug.Log("Lobby retrieved successfully");

            if (LobbyRoomPlayerListContent != null)
            {
                foreach (Transform child in LobbyRoomPlayerListContent)
                {
                    Destroy(child.gameObject);
                }

                foreach (var player in selectedLobby.Players)
                {
                    Debug.Log($"Fetching player name for player ID: {player.Id}");
                    string playerName = await GetPlayerName(player.Id);
                    Debug.Log($"Player name for player ID: {player.Id} is {playerName}");
                    GameObject playerItem = Instantiate(LobbyPlayerNamePrefab, LobbyRoomPlayerListContent);
                    var playerNameUI = playerItem.GetComponent<LobbyPlayerListUI>();
                    playerNameUI.Initialize(playerName);
                }
            }
            else
            {
                Debug.LogWarning("LobbyRoomPlayerListContent is null. Cannot display player list.");
            }
        }
        catch (RequestFailedException ex)
        {
            Debug.LogError($"Failed to display player list: {ex.Message} (Error Code: {ex.ErrorCode})");
        }
        catch (Exception ex)
        {
            Debug.LogError($"Failed to display player list: {ex.Message}");
        }
    }

    private async Task<string> GetPlayerName(string playerId)
    {
        string playerName = LobbyManager.Instance.GetCachedPlayerName(playerId);

        if (playerName == "Unknown")
        {
            try
            {
                await UserData.Instance.LoadPlayerDataFromServer(playerId);

                if (UserData.Instance.Character != null)
                {
                    playerName = UserData.Instance.Character.PlayerName;
                    LobbyManager.Instance.CachePlayerName(playerId, playerName);
                }
                else
                {
                    Debug.LogWarning($"Character data is null for player ID: {playerId}");
                }
            }
            catch (Exception ex)
            {
                Debug.LogError($"Exception occurred while fetching player name for player ID: {playerId}. Exception: {ex.Message}");
            }
        }

        return playerName;
    }
    #endregion

    #region Map Management
    private void OnLeftButtonClicked()
    {
        if (currentMapIndex - 1 > 0)
        {
            currentMapIndex--;
        }
        else
        {
            currentMapIndex = 0;
        }

        UpdateMap();

        Debug.Log($"Selected map index: {currentMapIndex}, scene name: {mapSelectionData.Maps[currentMapIndex].SceneName}");

    }

    private void OnRightButtonClicked()
    {
        if (currentMapIndex + 1 < mapSelectionData.Maps.Count)
        {
            currentMapIndex++;
        }
        else
        {
            currentMapIndex = 0; // 처음으로 돌아갑니다.
        }

        UpdateMap();

        // 추가적인 작업이 필요하면 여기서 처리합니다.
        Debug.Log($"Selected map index: {currentMapIndex}, scene name: {mapSelectionData.Maps[currentMapIndex].SceneName}");
    }

    private void UpdateMap()
    {
        var mapInfo = MapSelectionData.Instance.Maps[currentMapIndex];
        mapImage.sprite = mapInfo.MapImage;
        mapName.text = mapInfo.MapName;
    }
    #endregion

    #region Game Start
    private async void OnLobbyUpdated(Lobby updatedLobby)
    {
        await UpdatePlayerListUI(updatedLobby);
    }


    private async Task UpdatePlayerListUI(Lobby lobby)
    {
        if (!ValidateLobbyData(lobby))
        {
            return;
        }

        if (LobbyRoomPlayerListContent != null)
        {
            foreach (Transform child in LobbyRoomPlayerListContent)
            {
                if (child != null)
                {
                    Destroy(child.gameObject);
                }
            }

            foreach (var player in lobby.Players)
            {
                if (LobbyRoomPlayerListContent != null)
                {
                    GameObject playerItem = Instantiate(LobbyPlayerNamePrefab, LobbyRoomPlayerListContent);
                    var playerUI = playerItem.GetComponent<LobbyPlayerListUI>();

                    if (playerUI == null)
                    {
                        Debug.LogError("LobbyPlayerListUI component is missing on LobbyPlayerNamePrefab.");
                        continue;
                    }

                    string playerName = await GetPlayerName(player.Id);
                    playerUI.Initialize(playerName);
                }
            }
        }
        else
        {
            Debug.LogWarning("LobbyRoomPlayerListContent is null. Cannot update player list UI.");
        }
    }

    // 동기화 되며 넘어가는 씬
    private async void OnStartGame()
    {

        //Debug.Log("Starting game...");

        //if (LobbyManager.Instance.lobby.HostId == AuthenticationService.Instance.PlayerId)
        //{
        //    Debug.Log("Creating relay as host...");

        //    string joinCode = await RelayManager.Instance.CreateRelay(3);
        //    if (!string.IsNullOrEmpty(joinCode))
        //    {
        //        Debug.Log($"Relay server created. Join code: {joinCode}");

        //        var (allocationId, key, connectionData, dtlsAddress, dtlsPort) = RelayManager.Instance.GetHostConnectionInfo();

        //        UnityTransport transport = NetworkManager.Singleton.GetComponent<UnityTransport>();
        //        transport.SetHostRelayData(dtlsAddress, (ushort)dtlsPort, allocationId, key, connectionData, true);

        //        NetworkManager.Singleton.StartHost();

        //        var lobbyData = new Dictionary<string, DataObject>
        //    {
        //        { "JoinCode", new DataObject(DataObject.VisibilityOptions.Member, joinCode) }
        //    };

        //        bool updated = await LobbyManager.Instance.UpdateLobbyData(lobbyData);
        //        if (updated)
        //        {
        //            Debug.Log("Host started successfully.");

        //            // Scene 전환 플래그 설정
        //            string sceneName = mapSelectionData.Maps[currentMapIndex].SceneName;
        //            bool flagSet = await LobbyManager.Instance.SetGameStartFlag(sceneName);
        //            if (flagSet)
        //            {
        //                Debug.Log("Game start flag set successfully.");
        //            }
        //            else
        //            {
        //                Debug.LogError("Failed to set game start flag.");
        //            }
        //        }
        //        else
        //        {
        //            Debug.LogError("Failed to update lobby data.");
        //        }
        //    }
        //    else
        //    {
        //        Debug.LogError("Failed to create relay as host.");
        //    }
        //}
        //else
        //{
        //    Debug.Log("Joining relay as client...");

        //    string joinCode = LobbyManager.Instance.lobby.Data["JoinCode"].Value;
        //    bool success = await RelayManager.Instance.JoinRelay(joinCode);
        //    if (success)
        //    {
        //        var (allocationId, key, connectionData, hostConnectionData, dtlsAddress, dtlsPort) = RelayManager.Instance.GetClientConnectionInfo();

        //        UnityTransport transport = NetworkManager.Singleton.GetComponent<UnityTransport>();
        //        transport.SetClientRelayData(dtlsAddress, (ushort)dtlsPort, allocationId, key, connectionData, hostConnectionData, true);

        //        NetworkManager.Singleton.StartClient();

        //        Debug.Log("Client started successfully.");
        //    }
        //    else
        //    {
        //        Debug.LogError("Failed to join relay as client.");
        //    }
        //}

        Debug.Log("Starting game...");

        // 싱글 플레이어 모드에서는 네트워크 관련 코드를 모두 제거합니다.

        // 씬 이름을 mapSelectionData에서 가져옵니다.
        string sceneName = mapSelectionData.Maps[currentMapIndex].SceneName;

        // 게임 씬을 로드합니다.
        LoadGameScene(sceneName);
    }

    private void LoadGameScene(string sceneName)
    {
        //sceneName = mapSelectionData.Maps[currentMapIndex].SceneName;
        SceneLoader.Instance.LoadSceneAsync(sceneName);
    }

    private async Task<bool> StartGame(string sceneName)
    {
        try
        {
            var updatedData = new Dictionary<string, DataObject>();

            foreach (var data in LobbyManager.Instance.lobby.Data)
            {
                updatedData[data.Key] = new DataObject(data.Value.Visibility, data.Value.Value);
            }

            if (updatedData.ContainsKey("GameStart"))
            {
                updatedData["GameStart"] = new DataObject(updatedData["GameStart"].Visibility, "true");
            }
            else
            {
                updatedData["GameStart"] = new DataObject(DataObject.VisibilityOptions.Public, "true");
            }

            if (updatedData.ContainsKey("SceneName"))
            {
                updatedData["SceneName"] = new DataObject(updatedData["SceneName"].Visibility, sceneName);
            }
            else
            {
                updatedData["SceneName"] = new DataObject(DataObject.VisibilityOptions.Public, sceneName);
            }

            await LobbyService.Instance.UpdateLobbyAsync(LobbyManager.Instance.lobby.Id, new UpdateLobbyOptions { Data = updatedData });
            return true;
        }
        catch (Exception ex)
        {
            Debug.LogError($"Failed to start game: {ex.Message}");
            return false;
        }
    }


    //private async Task StartHost()
    //{
    //    string joinCode = await RelayManager.Instance.CreateRelay(3);
    //    if (!string.IsNullOrEmpty(joinCode))
    //    {
    //        Debug.Log($"Relay server created. Join code: {joinCode}");
    //        var (allocationId, key, connectionData, dtlsAddress, dtlsPort) = RelayManager.Instance.GetHostConnectionInfo();

    //        UnityTransport transport = NetworkManager.Singleton.GetComponent<UnityTransport>();
    //        transport.SetHostRelayData(dtlsAddress, (ushort)dtlsPort, allocationId, key, connectionData, true);

    //        NetworkManager.Singleton.StartHost();
    //    }
    //    else
    //    {
    //        Debug.LogError("Failed to create relay.");
    //    }
    //}

    //private async Task JoinHost(string joinCode)
    //{
    //    bool success = await RelayManager.Instance.JoinRelay(joinCode);
    //    if (success)
    //    {
    //        var (allocationId, key, connectionData, hostConnectionData, dtlsAddress, dtlsPort) = RelayManager.Instance.GetClientConnectionInfo();

    //        UnityTransport transport = NetworkManager.Singleton.GetComponent<UnityTransport>();
    //        transport.SetClientRelayData(dtlsAddress, (ushort)dtlsPort, allocationId, key, connectionData, hostConnectionData, true);

    //        NetworkManager.Singleton.StartClient();
    //    }
    //    else
    //    {
    //        Debug.LogError("Failed to join relay.");
    //    }
    //}
    #endregion

    private async void LobbyRoomConnect()
    {
        Debug.Log($"Joining room with ID: {selectedLobbyId}");

        var playerData = new Dictionary<string, PlayerDataObject>
        {
            { "PlayerName", new PlayerDataObject(PlayerDataObject.VisibilityOptions.Member, "PlayerName") }
        };

        bool success = await LobbyManager.Instance.JoinLobby(selectedLobbyId, playerData);
        if (success)
        {
            Debug.Log("Room joined successfully.");
            await UpdatePlayerListUI(LobbyManager.Instance.lobby);
        }
        else
        {
            Debug.LogError("Failed to join room.");
        }
    }
    private async Task RefreshLobbyList()
    {
        try
        {
            var lobbies = await LobbyManager.Instance.GetLobbies();

            // 현재 UI 요소가 파괴되지 않았는지 확인합니다.
            if (LobbyRoomListContent != null)
            {
                foreach (Transform child in LobbyRoomListContent)
                {
                    if (child != null)
                    {
                        Destroy(child.gameObject);
                    }
                }

                foreach (var lobby in lobbies)
                {
                    if (LobbyRoomListContent != null)
                    {
                        GameObject lobbyItem = Instantiate(LobbyRoomListPrefab, LobbyRoomListContent);
                        var lobbyRoomListUI = lobbyItem.GetComponent<LobbyRoomListUI>();
                        await lobbyRoomListUI.Initialize(lobby, SelectLobby, lobbyRoomCodeInputField.GetComponent<TMP_InputFieldManager>().GetHiddenTitleText());

                        // 각 로비의 플레이어 이름을 가져와서 설정합니다.
                        foreach (var player in lobby.Players)
                        {
                            string playerName = await GetPlayerName(player.Id);
                            // 여기에 로비 UI 요소에 플레이어 이름을 설정하는 코드를 추가합니다.
                        }
                    }
                }
            }
            else
            {
                Debug.LogWarning("LobbyRoomListContent is null. Cannot refresh lobby list.");
            }
        }
        catch (Exception ex)
        {
            Debug.LogError($"Failed to refresh lobby list: {ex.Message}");
        }
    }
    private async Task RefreshPlayerList()
    {
        if (LobbyRoomPlayerListContent == null)
        {
            Debug.LogWarning("LobbyRoomPlayerListContent is null. Cannot refresh player list.");
            return;
        }

        try
        {
            var lobby = LobbyManager.Instance.lobby;
            if (lobby == null)
            {
                Debug.LogWarning("LobbyManager.Instance.lobby is null. Cannot refresh player list.");
                return;
            }

            var players = lobby.Players;
            if (players == null)
            {
                Debug.LogWarning("Lobby players list is null. Cannot refresh player list.");
                return;
            }

            foreach (Transform child in LobbyRoomPlayerListContent)
            {
                if (child != null)
                {
                    Destroy(child.gameObject);
                }
            }

            foreach (var player in players)
            {
                if (LobbyRoomPlayerListContent != null)
                {
                    GameObject playerItem = Instantiate(LobbyPlayerNamePrefab, LobbyRoomPlayerListContent);
                    var playerUI = playerItem.GetComponent<LobbyPlayerListUI>();

                    if (playerUI == null)
                    {
                        Debug.LogError("LobbyPlayerListUI component is missing in the instantiated player item.");
                        continue;
                    }

                    string playerName = LobbyManager.Instance.GetCachedPlayerName(player.Id);
                    playerUI.Initialize(playerName);
                }
                else
                {
                    Debug.LogWarning("LobbyRoomPlayerListContent became null while refreshing player list.");
                }
            }
        }
        catch (Exception ex)
        {
            Debug.LogError($"Failed to refresh player list: {ex.Message}");
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
}



