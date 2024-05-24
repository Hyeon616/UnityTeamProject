using DG.Tweening;
using System;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.Networking;

public class RegisterLoginManager : MonoBehaviour
{
    [SerializeField] private TMP_InputField usernameField;
    [SerializeField] private TMP_InputField passwordField;
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
        //Debug.Log("Register URL: " + registerUrl);
        //Debug.Log("Login URL: " + loginUrl);
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
        if (string.IsNullOrEmpty(usernameField.text))
        {
            ShowFeedback("���̵� �Է����ּ���");
            yield break;
        }

        if (string.IsNullOrEmpty(passwordField.text))
        {
            ShowFeedback("�н����带 �Է����ּ���");
            yield break;
        }

        var formData = new RegisterData
        {
            Username = usernameField.text,
            Password = passwordField.text
        };

        string jsonData = JsonUtility.ToJson(formData);

        UnityWebRequest request = new UnityWebRequest(registerUrl, "POST");
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
            if (request.responseCode == 400)
            {
                ShowFeedback("���̵� �̹� �ֽ��ϴ�.");
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

    IEnumerator LoginUser()
    {
        if (string.IsNullOrEmpty(usernameField.text))
        {
            ShowFeedback("���̵� �Է����ּ���");
            yield break;
        }

        if (string.IsNullOrEmpty(passwordField.text))
        {
            ShowFeedback("�н����带 �Է����ּ���");
            yield break;
        }

        var formData = new LoginData
        {
            Username = usernameField.text,
            Password = passwordField.text
        };

        string jsonData = JsonUtility.ToJson(formData);

        UnityWebRequest request = new UnityWebRequest(loginUrl, "POST");
        byte[] bodyRaw = System.Text.Encoding.UTF8.GetBytes(jsonData);
        request.uploadHandler = new UploadHandlerRaw(bodyRaw);
        request.downloadHandler = new DownloadHandlerBuffer();
        request.SetRequestHeader("Content-Type", "application/json");

        yield return request.SendWebRequest();

        if (request.result == UnityWebRequest.Result.Success)
        {
            // todo
            ShowFeedback("�α��� ����");
            authPanel.SetActive(false);
            yield return new WaitForSeconds(2);
            isLogin = true;
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

    void ShowFeedback(string message)
    {
        feedbackText.text = message;
        feedbackText.DOFade(1, 1).OnComplete(() => feedbackText.DOFade(0, 2));
    }

    [System.Serializable]
    public class RegisterData
    {
        public string Username;
        public string Password;
    }

    [System.Serializable]
    public class LoginData
    {
        public string Username;
        public string Password;
    }
}
