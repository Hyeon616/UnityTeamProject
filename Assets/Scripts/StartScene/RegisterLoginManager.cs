using DG.Tweening;
using System;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.Networking;

public class RegisterLoginManager : MonoBehaviour
{
    [SerializeField] private TMP_InputField loginUsernameField;
    [SerializeField] private TMP_InputField loginPasswordField;
    [SerializeField] private TMP_InputField RegisterUsernameField;
    [SerializeField] private TMP_InputField RegisterPasswordField;
    [SerializeField] private TMP_InputField RegisterPlayerNameField;
    [SerializeField] private TextMeshProUGUI feedbackText;
    [SerializeField] private GameObject authPanel;
    [SerializeField] private TextMeshProUGUI loginText;
    
    private string registerUrl;
    private string loginUrl;

    public static bool isLogin = false;
    public static event Action OnLoginSuccess;

    IEnumerator Start()
    {
        // Remote Config ���� �ε�� ������ ���
        while (string.IsNullOrEmpty(RemoteConfigManager.ServerUrl))
        {
            yield return null;
        }

        registerUrl = $"{RemoteConfigManager.ServerUrl}/api/register";
        loginUrl = $"{RemoteConfigManager.ServerUrl}/api/login";
        Debug.Log("Register URL: " + registerUrl);
        Debug.Log("Login URL: " + loginUrl);
    }

    public void OnRegisterButtonClicked()
    {
        StartCoroutine(RegisterUser());
    }

    public void OnLoginButtonClicked()
    {
        StartCoroutine(LoginUser());
    }

    public void OnCancelButtonClicked()
    {
        authPanel.SetActive(false);
        loginText.gameObject.SetActive(true);
    }

    IEnumerator RegisterUser()
    {
        if (string.IsNullOrEmpty(RegisterUsernameField.text))
        {
            ShowFeedback("���̵� �Է����ּ���");
            yield break;
        }

        if (string.IsNullOrEmpty(RegisterPasswordField.text))
        {
            ShowFeedback("�н����带 �Է����ּ���");
            yield break;
        }

        var formData = new RegisterData
        {
            Username = RegisterUsernameField.text,
            Password = RegisterPasswordField.text,
            PlayerName = RegisterPlayerNameField.text
        };

        string jsonData = JsonUtility.ToJson(formData);

        using (UnityWebRequest request = new UnityWebRequest(registerUrl, "POST"))
        {
            byte[] bodyRaw = System.Text.Encoding.UTF8.GetBytes(jsonData);
            request.uploadHandler = new UploadHandlerRaw(bodyRaw);
            request.downloadHandler = new DownloadHandlerBuffer();
            request.SetRequestHeader("Content-Type", "application/json");

            yield return request.SendWebRequest();

            if (request.result == UnityWebRequest.Result.Success)
            {
                ShowFeedback("ȸ������ ����");
            }
            else
            {
                if (request.responseCode == 409)
                {
                    var response = JsonUtility.FromJson<ErrorResponse>(request.downloadHandler.text);
                    if (response.errorCode == "USERNAME_EXISTS")
                    {
                        ShowFeedback("���̵� �̹� �ֽ��ϴ�.");
                    }
                    else if (response.errorCode == "PLAYERNAME_EXISTS")
                    {
                        ShowFeedback("�÷��̾� �̸��� �̹� �ֽ��ϴ�.");
                    }
                }
                else if (request.responseCode == 500)
                {
                    ShowFeedback("���� ����");
                }
                else
                {
                    ShowFeedback("Error: " + request.error);
                }
            }
        }

    }

    IEnumerator LoginUser()
    {
        if (string.IsNullOrEmpty(loginUsernameField.text))
        {
            ShowFeedback("���̵� �Է����ּ���");
            yield break;
        }

        if (string.IsNullOrEmpty(loginPasswordField.text))
        {
            ShowFeedback("�н����带 �Է����ּ���");
            yield break;
        }

        var formData = new LoginData
        {
            Username = loginUsernameField.text,
            Password = loginPasswordField.text
        };

        string jsonData = JsonUtility.ToJson(formData);

        using (UnityWebRequest request = new UnityWebRequest(loginUrl, "POST"))
        {
            byte[] bodyRaw = System.Text.Encoding.UTF8.GetBytes(jsonData);
            request.uploadHandler = new UploadHandlerRaw(bodyRaw);
            request.downloadHandler = new DownloadHandlerBuffer();
            request.SetRequestHeader("Content-Type", "application/json");

            yield return request.SendWebRequest();

            if (request.result == UnityWebRequest.Result.Success)
            {
                ShowFeedback("�α��� ����");
                isLogin = true;
                yield return new WaitForSeconds(1);
                loginText.gameObject.SetActive(false);
                authPanel.SetActive(false);
                OnLoginSuccess?.Invoke();
            }
            else
            {
                if (request.responseCode == 500)
                {
                    ShowFeedback("���� ����");
                }
                else if (request.responseCode == 404)
                {
                    ShowFeedback("���̵� ���ų� ��й�ȣ�� Ʋ�Ƚ��ϴ�");
                }
                else
                {
                    ShowFeedback("Error: " + request.error);
                }
            }
        }
    }

    void ShowFeedback(string message)
    {
        feedbackText.text = message;
        feedbackText.DOFade(1, 1).OnComplete(() => feedbackText.DOFade(0, 2));
    }

}
