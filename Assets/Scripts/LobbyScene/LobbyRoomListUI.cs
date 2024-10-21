using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class LobbyRoomListUI : MonoBehaviour
{
     public TextMeshProUGUI mapNameText;
     public TextMeshProUGUI titleText;
     public TextMeshProUGUI playerCountText;
     public Button Button_LobbyRoomListPrefab;

    private string roomName;

    public void Initialize(string roomName, string mapName, int currentPlayers, int maxPlayers)
    {
        this.roomName = roomName;
        mapNameText.text = mapName;
        titleText.text = roomName;
        playerCountText.text = $"{currentPlayers}/{maxPlayers}";

    }

    public void UpdatePlayerCount(int currentPlayers, int maxPlayers)
    {
        playerCountText.text = $"{currentPlayers}/{maxPlayers}";
    }

}