using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MainMenuController : MonoBehaviour
{

    [SerializeField] private GameObject mainScreen;
    [SerializeField] private GameObject joinScreen;

    [SerializeField] Button hostButton;
    [SerializeField] Button joinButton;

    [SerializeField] Button submitCodeButton;
    [SerializeField] TextMeshProUGUI codeText;

    void OnEnable()
    {
        hostButton.onClick.AddListener(OnHostClicked);
        joinButton.onClick.AddListener(OnJoinClicked);
        submitCodeButton.onClick.AddListener(SubmitCodeClicked);
    }

    private void OnDisable()
    {
        hostButton.onClick.RemoveListener(OnHostClicked);
        joinButton.onClick.RemoveListener(OnJoinClicked);
        submitCodeButton.onClick.RemoveListener(SubmitCodeClicked);
    }

    private async void OnHostClicked()
    {
        bool succeeded = await GameLobbyManager.Instance.CreateLobby();
        if (succeeded)
        {
            SceneManager.LoadSceneAsync("RoomScene_WJH");
        }

    }

    private void OnJoinClicked()
    {
        mainScreen.SetActive(false);
        joinScreen.SetActive(true);
    }

    private async void SubmitCodeClicked()
    {
        string code = codeText.text;
        code = code.Substring(0, code.Length - 1);

        bool succeeded = await GameLobbyManager.Instance.JoinLobby(code);

        if (succeeded)
        {
            SceneManager.LoadSceneAsync("RoomScene_WJH");
        }
    }
}
