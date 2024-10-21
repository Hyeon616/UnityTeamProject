using Newtonsoft.Json;
using System.Collections.Generic;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;
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

    private string selectedRoomName;
    private Dictionary<string, GameObject> roomListItems = new Dictionary<string, GameObject>();
    private bool isInRoom = false;

    private void OnEnable()
    {
        leaveRoomButton.onClick.AddListener(OnClickedLeaveRoomButton);
        joinRoomButton.onClick.AddListener(OnClickedJoinRoomButton);
        lobbyRoomCodeSubmit.onClick.AddListener(RoomCodeSubmit);

    }

    private void OnDisable()
    {
        leaveRoomButton.onClick.RemoveListener(OnClickedLeaveRoomButton);
        joinRoomButton.onClick.RemoveListener(OnClickedJoinRoomButton);
        lobbyRoomCodeSubmit.onClick.RemoveListener(RoomCodeSubmit);
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
        var leaveRoomRequest = new
        {
            action = "leave_room",
            playerId = UserData.Instance.UserId
        };
        string jsonRequest = JsonConvert.SerializeObject(leaveRoomRequest);
        string response = await ServerConnector.Instance.SendMessage(jsonRequest);
        var responseData = JsonConvert.DeserializeObject<Dictionary<string, object>>(response);
        if (responseData["status"].ToString() == "success")
        {
            JoinMenuUI.SetActive(true);
            lobbyRoomUI.SetActive(false);
            BackRoomButton.gameObject.SetActive(true);
            isInRoom = false;
            // 방 목록 업데이트
            await GetRoomList();
        }
        else
        {
            Debug.LogError($"Failed to leave room: {responseData["message"]}");
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

        joinRoomButton.gameObject.SetActive(true);
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
            joinRoomButton.gameObject.SetActive(false);
            isInRoom = true;
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
        var getRoomListRequest = new { action = "get_room_list" };
        string jsonRequest = JsonConvert.SerializeObject(getRoomListRequest);
        string response = await ServerConnector.Instance.SendMessage(jsonRequest);
        var responseData = JsonConvert.DeserializeObject<Dictionary<string, object>>(response);

        if (responseData["status"].ToString() == "success")
        {
            var rooms = JsonConvert.DeserializeObject<List<Dictionary<string, object>>>(responseData["rooms"].ToString());
            UpdateRoomList(rooms);
        }
        else
        {
            Debug.LogError($"Failed to get room list: {responseData["message"]}");
        }
    }
    private void UpdateRoomList(List<Dictionary<string, object>> rooms)
    {
        foreach (Transform child in LobbyRoomListContent)
        {
            Destroy(child.gameObject);
        }
        roomListItems.Clear();

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

            // 방 버튼에 클릭 이벤트 추가
            roomListUI.Button_LobbyRoomListPrefab.onClick.AddListener(() => ShowRoomPlayers(roomName, players));
        }

        joinRoomButton.gameObject.SetActive(false);
    }
}
