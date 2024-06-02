using TMPro;
using UnityEngine;

public class LobbyPlayerListUI : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI playerNameText;

    private void Awake()
    {
        if (playerNameText == null)
        {
            enabled = false;
            return;
        }
    }

    public void Initialize(string playerName)
    {
        if (string.IsNullOrEmpty(playerName))
        {
            Debug.LogError("playerName is null or empty.");
            return;
        }

        playerNameText.text = playerName;
    }

    public string GetPlayerNameText()
    {
        return playerNameText.text;
    }
}