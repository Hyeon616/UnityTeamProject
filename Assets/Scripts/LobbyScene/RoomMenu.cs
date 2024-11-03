using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class RoomMenu : MonoBehaviour
{
    [Header("Room")]
    [SerializeField] private Transform LobbyRoomListContent;
    [SerializeField] private Transform LobbyRoomPlayerListContent;
    [SerializeField] private GameObject LobbyRoomListPrefab;
    [SerializeField] private GameObject LobbyPlayerNamePrefab;
    [SerializeField] private Button leaveRoomButton;
    [SerializeField] private Button joinRoomButton;
    [SerializeField] private Button startSceneButton;
    [SerializeField] private GameObject lobbyRoomUI;
    [SerializeField] private GameObject JoinMenuUI;
    [SerializeField] private Button lobbyRoomCodeSubmit;
    [SerializeField] private TMP_InputField lobbyRoomCodeInputField;
    [SerializeField] private Button BackRoomButton;
    [SerializeField] private Button ParticipateLobbyButton;


    private string selectedRoomName;
    private Dictionary<string, GameObject> roomListItems = new Dictionary<string, GameObject>();
    private bool isInRoom = false;
    private bool isRoomHost = false;
    private bool isSceneLoading = false;

    private void OnEnable()
    {
        leaveRoomButton.onClick.AddListener(OnClickedLeaveRoomButton);
        joinRoomButton.onClick.AddListener(OnClickedJoinRoomButton);
        lobbyRoomCodeSubmit.onClick.AddListener(RoomCodeSubmit);
        ParticipateLobbyButton.onClick.AddListener(OnClickedParticipateRoomButton);
        startSceneButton.onClick.AddListener(OnClickedStartScene);

        joinRoomButton.gameObject.SetActive(false);
        startSceneButton.gameObject.SetActive(false);
    }


    private void OnDisable()
    {
        leaveRoomButton.onClick.RemoveListener(OnClickedLeaveRoomButton);
        joinRoomButton.onClick.RemoveListener(OnClickedJoinRoomButton);
        lobbyRoomCodeSubmit.onClick.RemoveListener(RoomCodeSubmit);
        ParticipateLobbyButton.onClick.RemoveListener(OnClickedParticipateRoomButton);
        startSceneButton.onClick.RemoveListener(OnClickedStartScene);
    }


    // 방 생성
    private async void RoomCodeSubmit()
    {
        if (isInRoom)
        {
            Debug.Log("이미 방에 있습니다.");
            return;
        }

        // 방 생성
        string roomName = string.IsNullOrEmpty(lobbyRoomCodeInputField.text) ? "파티사냥 하실분" : lobbyRoomCodeInputField.text;
        var createRoomRequest = new
        {
            action = "create_room",
            roomName = roomName,
            hostId = UserData.Instance.UserId,
            mapName = MapMenu.SelectedMap.MapName
        };

        string jsonRequest = JsonConvert.SerializeObject(createRoomRequest);
        string response = await ServerConnector.Instance.SendMessage(jsonRequest);

        var responseData = JsonConvert.DeserializeObject<Dictionary<string, object>>(response);
        // 방 생성 성공
        if (responseData["status"].ToString() == "success")
        {
            Debug.Log($"Room created: {roomName}");
            JoinMenuUI.SetActive(false);
            lobbyRoomUI.SetActive(true);
            BackRoomButton.gameObject.SetActive(false);
            isInRoom = true;
            isRoomHost = true;
            selectedRoomName = roomName;

            UpdateRoomButton();
            await GetRoomList();

            _ = ListenRoomState();
        }
        else
        {
            Debug.Log($"{responseData["message"]}");
        }
    }

    // 방에서 나가기
    private async void OnClickedLeaveRoomButton()
    {

        try
        {
            var leaveRoomRequest = new
            {
                action = "leave_room",
                playerId = UserData.Instance.UserId
            };
            string jsonRequest = JsonConvert.SerializeObject(leaveRoomRequest);
            string response = await ServerConnector.Instance.SendMessage(jsonRequest);

            if (!string.IsNullOrEmpty(response))
            {
                var responseData = JsonConvert.DeserializeObject<Dictionary<string, object>>(response);
                if (responseData != null && responseData["status"].ToString() == "success")
                {
                    // UI 상태와 플래그 초기화
                    BackRoomButton.gameObject.SetActive(true);
                    isInRoom = false; // ListenGameStart가 이 플래그를 보고 멈춤
                    isRoomHost = false;
                    selectedRoomName = null;

                    // UI 초기화
                    foreach (Transform child in LobbyRoomPlayerListContent)
                    {
                        Destroy(child.gameObject);
                    }
                    foreach (Transform child in LobbyRoomListContent)
                    {
                        Destroy(child.gameObject);
                    }
                    roomListItems.Clear();

                    UpdateRoomButton();
                    await GetRoomList();
                }
                else
                {
                    Debug.LogError($"방 나가기 실패: {responseData?["message"]}");
                }
            }
        }
        catch (Exception ex)
        {
            Debug.LogError($"방 나가기 처리 중 오류 발생: {ex.Message}");
        }
    }

    private void ShowRoomPlayers(string roomName, List<string> players)
    {
        selectedRoomName = roomName; // 선택된 방 이름 저장

        foreach (Transform child in LobbyRoomPlayerListContent)
        {
            Destroy(child.gameObject);
        }

        foreach (string player in players)
        {
            GameObject playerItem = Instantiate(LobbyPlayerNamePrefab, LobbyRoomPlayerListContent);
            playerItem.GetComponentInChildren<TextMeshProUGUI>().text = player;
        }

        UpdateRoomButton();
    }

    public async void JoinRoom(string roomName)
    {
        if (isInRoom)
        {
            Debug.Log("이미 방에 있습니다.");
            return;
        }

        var joinRoomRequest = new
        {
            action = "join_room",
            roomName = roomName,
            playerId = UserData.Instance.UserId
        };

        string jsonRequest = JsonConvert.SerializeObject(joinRoomRequest);
        string response = await ServerConnector.Instance.SendMessage(jsonRequest);
        var responseData = JsonConvert.DeserializeObject<Dictionary<string, object>>(response);

        if (responseData["status"].ToString() == "success")
        {
            Debug.Log($"Joined room: {roomName}");
            JoinMenuUI.SetActive(false);
            lobbyRoomUI.SetActive(true);
            BackRoomButton.gameObject.SetActive(false);
            isInRoom = true;
            isRoomHost = false;

            UpdateRoomButton();
            await GetRoomList();


            _ = ListenRoomState();
        }
        else
        {
            Debug.LogError($"Failed to join room: {responseData["message"]}");
        }
    }

    private void OnClickedJoinRoomButton()
    {
        if (!string.IsNullOrEmpty(selectedRoomName))
        {
            JoinRoom(selectedRoomName);
        }
        else
        {
            Debug.LogError("No room selected");
        }
    }

    public async Task GetRoomList()
    {
        try
        {
            var getRoomListRequest = new { action = "get_room_list" };
            string jsonRequest = JsonConvert.SerializeObject(getRoomListRequest);
            string response = await ServerConnector.Instance.SendMessage(jsonRequest);

            if (string.IsNullOrEmpty(response))
            {
                return;
            }

            try
            {
                var responseData = JsonConvert.DeserializeObject<Dictionary<string, object>>(response);
                if (responseData != null && responseData["status"].ToString() == "success")
                {
                    var rooms = JsonConvert.DeserializeObject<List<Dictionary<string, object>>>(responseData["rooms"].ToString());
                    UpdateRoomList(rooms);
                }
                else
                {
                    string errorMessage = responseData != null && responseData.ContainsKey("message") ? responseData["message"].ToString() : "서버 응답 실패";
                    Debug.Log($"서버 응답 실패 (방 목록) : {errorMessage}");
                }
            }
            catch (JsonReaderException ex)
            {
                Debug.Log($"서버 응답 실패 (방 목록) : {ex.Message}\n응답: {response}");
            }
        }
        catch (Exception ex)
        {
            Debug.Log($"서버 응답 실패 (방 목록) : {ex.Message}");
        }
    }

    private void UpdateRoomList(List<Dictionary<string, object>> rooms)
    {
        foreach (Transform child in LobbyRoomListContent)
        {
            Destroy(child.gameObject);
        }
        roomListItems.Clear();

        // 현재 선택된 방의 플레이어 목록도 업데이트
        Dictionary<string, object> selectedRoom = null;
        if (!string.IsNullOrEmpty(selectedRoomName))
        {
            selectedRoom = rooms.FirstOrDefault(r => r["Name"].ToString() == selectedRoomName);
            if (selectedRoom != null)
            {
                List<string> players = JsonConvert.DeserializeObject<List<string>>(selectedRoom["Players"].ToString());
                ShowRoomPlayers(selectedRoomName, players);
            }
        }

        foreach (var room in rooms)
        {
            string roomName = room["Name"].ToString();
            string mapName = room["MapName"].ToString();
            List<string> players = JsonConvert.DeserializeObject<List<string>>(room["Players"].ToString());
            int maxPlayers = int.Parse(room["MaxPlayers"].ToString());

            GameObject roomListItem = Instantiate(LobbyRoomListPrefab, LobbyRoomListContent);
            LobbyRoomListUI roomListUI = roomListItem.GetComponent<LobbyRoomListUI>();

            roomListUI.Initialize(roomName, mapName, players.Count, maxPlayers);
            roomListItems[roomName] = roomListItem;

            roomListUI.Button_LobbyRoomListPrefab.onClick.AddListener(() => ShowRoomPlayers(roomName, players));
        }

        // 선택한 방이 삭제된 경우
        if (selectedRoom == null && !string.IsNullOrEmpty(selectedRoomName))
        {
            selectedRoomName = null;
            foreach (Transform child in LobbyRoomPlayerListContent)
            {
                Destroy(child.gameObject);
            }
        }

        UpdateRoomButton();
    }

    private void UpdateRoomButton()
    {
        // 방에 참가하지 않은 상태
        if (!isInRoom)
        {
            joinRoomButton.gameObject.SetActive(true);
            startSceneButton.gameObject.SetActive(false);
            return;
        }

        // 방에 참가한 상태
        joinRoomButton.gameObject.SetActive(false);

        // 방장인 경우에만 startSceneButton 표시
        startSceneButton.gameObject.SetActive(isRoomHost);
    }

    private async void OnClickedParticipateRoomButton()
    {
        await GetRoomList();
    }

    private async void OnClickedStartScene()
    {
        if (!isRoomHost || isSceneLoading) return;

        try
        {
            isSceneLoading = true;

            var startGameRequest = new
            {
                action = "start_game",
                roomName = selectedRoomName,
                hostId = UserData.Instance.UserId,
                sceneName = MapMenu.SelectedMap.SceneName
            };

            string jsonRequest = JsonConvert.SerializeObject(startGameRequest);
            Debug.Log($"[StartGame] 요청 전송: {jsonRequest}");

            string response = await ServerConnector.Instance.SendMessage(jsonRequest);
            Debug.Log($"[StartGame] 응답 수신: {response}");

            var responseData = JsonConvert.DeserializeObject<Dictionary<string, object>>(response);

            if (responseData["status"].ToString() != "success")
            {
                Debug.LogError($"[StartGame] 게임 시작 실패: {responseData["message"]}");
                isSceneLoading = false;
            }
        }
        catch (Exception ex)
        {
            Debug.LogError($"[StartGame] 에러 발생: {ex.Message}");
            isSceneLoading = false;
        }
    }

    private async Task ListenRoomState()
    {
        Debug.Log($"[ListenRoomState] 시작 - Player ID: {UserData.Instance.UserId}");

        try
        {
            while (isInRoom && !isSceneLoading)
            {
                string message = await ServerConnector.Instance.ReadMessage();
                if (string.IsNullOrEmpty(message)) continue;

                Debug.Log($"[ListenRoomState] 받은 메시지: {message}");

                // 여러 JSON 메시지 분리
                var messages = message.Split(new[] { "}{" }, StringSplitOptions.None)
                    .Select(m => m.EndsWith("}") ? m : m + "}")
                    .Select(m => m.StartsWith("{") ? m : "{" + m);

                foreach (var msg in messages)
                {
                    try
                    {
                        var data = JsonConvert.DeserializeObject<Dictionary<string, object>>(msg);
                        if (data == null || !data.ContainsKey("action")) continue;

                        string action = data["action"].ToString();
                        string status = data["status"]?.ToString();

                        Debug.Log($"[ListenRoomState] 처리 중: Action={action}, Status={status}");

                        if (status != "success") continue;

                        switch (action)
                        {
                            case "start_game":
                                Debug.Log($"[ListenRoomState] 게임 시작 메시지 수신 - Player: {UserData.Instance.UserId}");
                                string sceneName = data["sceneName"].ToString();
                                isInRoom = false;
                                isSceneLoading = true;
                                await LoadGameScene(sceneName);
                                return;

                            case "get_room_list":
                                var rooms = JsonConvert.DeserializeObject<List<Dictionary<string, object>>>(data["rooms"].ToString());
                                UpdateRoomList(rooms);
                                break;
                        }
                    }
                    catch (Exception ex)
                    {
                        Debug.LogWarning($"[ListenRoomState] 메시지 처리 중 오류: {ex.Message}\n메시지: {msg}");
                    }
                }

                await Task.Delay(10);
            }
        }
        catch (Exception ex)
        {
            Debug.LogError($"[ListenRoomState] 에러 발생: {ex.Message}\n{ex.StackTrace}");
            isSceneLoading = false;
        }
    }



    private async Task LoadGameScene(string sceneName)
    {
        try
        {
            Debug.Log($"[LoadGameScene] 씬 로드 시작: {sceneName}");

            AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(sceneName);
            asyncLoad.allowSceneActivation = true;

            while (!asyncLoad.isDone)
            {
                await Task.Yield();
            }

            Debug.Log($"[LoadGameScene] 씬 로드 완료: {sceneName}");
        }
        catch (Exception ex)
        {
            Debug.LogError($"[LoadGameScene] 에러 발생: {ex.Message}");
            isSceneLoading = false;
        }
    }
}
