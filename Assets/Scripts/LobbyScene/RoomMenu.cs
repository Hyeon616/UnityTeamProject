using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
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

            // 응답이 null이거나 비어있는지 확인
            if (string.IsNullOrEmpty(response))
            {
                return;
            }

            try
            {
                var responseData = JsonConvert.DeserializeObject<Dictionary<string, object>>(response);
                if (responseData != null && responseData["status"].ToString() == "success")
                {
                    BackRoomButton.gameObject.SetActive(true);
                    isInRoom = false;
                    isRoomHost = false;
                    UpdateRoomButton();
                    await GetRoomList();
                }
                else
                {
                    string errorMessage = responseData != null && responseData.ContainsKey("message") ? responseData["message"].ToString() : "서버 응답 실패";
                }
            }
            catch (JsonReaderException ex)
            {
                Debug.Log($"서버 응답 실패 : {ex.Message}\nResponse: {response}");
            }
        }
        catch (Exception ex)
        {
            Debug.Log($"서버 응답 실패(방에서 나가기) : {ex.Message}");
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
        if (!isRoomHost)
        {
            Debug.Log("방장만 게임을 시작할 수 있습니다.");
            return;
        }

        var startGameRequest = new
        {
            action = "start_game",
            roomName = selectedRoomName,
            hostId = UserData.Instance.UserId,
            sceneName = MapMenu.SelectedMap.SceneName
        };

        string jsonRequest = JsonConvert.SerializeObject(startGameRequest);
        string response = await ServerConnector.Instance.SendMessage(jsonRequest);

        var responseData = JsonConvert.DeserializeObject<Dictionary<string, object>>(response);

        if (responseData["status"].ToString() == "success")
        {
            string sceneName = responseData["sceneName"].ToString();
            await LoadGameScene(sceneName);
        }
        else
        {
            Debug.LogError($"게임 시작 실패: {responseData["message"]}");
        }
    }

    private async Task LoadGameScene(string sceneName)
    {
        AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(sceneName);

        while (!asyncLoad.isDone)
        {
            await Task.Yield();
        }
    }

}
