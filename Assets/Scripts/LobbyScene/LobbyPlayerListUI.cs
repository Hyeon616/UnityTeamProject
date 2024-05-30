using TMPro;
using UnityEngine;

public class LobbyPlayerListUI : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI playerNameText;

    public void Initialize(string playerName)
    {
        if (playerNameText == null)
        {
            Debug.LogError("playerNameText is not assigned.");
            return;
        }

        playerNameText.text = playerName;
    }
}