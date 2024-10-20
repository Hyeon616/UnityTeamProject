using DG.Tweening;
using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

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

    private ServerConnector serverConnector;

    public static bool isLogin = false;
    public static event Action OnLoginSuccess;

    private void Start()
    {
        serverConnector = FindObjectOfType<ServerConnector>();
        if (serverConnector == null)
        {
            Debug.Log("serverConnector가 없습니다.");
        }
    }

    public void OnRegisterButtonClicked()
    {
        StartCoroutine(SendRequest("register"));
    }

    public void OnLoginButtonClicked()
    {
        StartCoroutine(SendRequest("login"));
    }

    public void OnCancelButtonClicked()
    {
        authPanel.SetActive(false);
        loginText.gameObject.SetActive(true);
    }

    private IEnumerator SendRequest(string action)
    {
        string id = null;
        string password = null;
        string playername = null;

        if (action == "register")
        {
            id = RegisterUsernameField.text;
            password = RegisterPasswordField.text;
            playername = RegisterPlayerNameField.text;
            if (string.IsNullOrEmpty(id))
            {
                ShowFeedback("아이디를 입력해주세요.");
                yield break;
            }

            if (string.IsNullOrEmpty(password))
            {
                ShowFeedback("패스워드를 입력해주세요.");
                yield break;
            }

            if (string.IsNullOrEmpty(playername))
            {
                ShowFeedback("닉네임을 입력해주세요.");
                yield break;
            }

        }
        else if (action == "login")
        {
            id = loginUsernameField.text;
            password = loginPasswordField.text;
            if (string.IsNullOrEmpty(id))
            {
                ShowFeedback("아이디를 입력해주세요.");
                yield break;
            }

            if (string.IsNullOrEmpty(password))
            {
                ShowFeedback("패스워드를 입력해주세요.");
                yield break;
            }
        }

        var request = new { action, id, password, playername };
        string jsonRequest = JsonConvert.SerializeObject(request);

        yield return StartCoroutine(SendMessage(jsonRequest));

    }


    IEnumerator SendMessage(string message)
    {
        var task = serverConnector.SendMessage(message);
        yield return new WaitUntil(() => task.IsCompleted);

        if (task.Exception != null)
        {
            Debug.Log($"서버 전송 실패 : {task.Exception.Message}");
            ShowFeedback("서버 연결 오류");
            yield break;
        }

        string jsonResponse = task.Result;

        if (string.IsNullOrEmpty(jsonResponse))
        {
            ShowFeedback("서버 응답 없음");
            yield break;
        }

        var response = JsonConvert.DeserializeObject<Dictionary<string, object>>(jsonResponse);
        if (response.TryGetValue("status", out object status) && status.ToString() == "success")
        {
            response.TryGetValue("action", out object action);
            string actionType = action.ToString();

            Debug.Log(actionType);

            if (actionType.ToString() == "register")
            {
                ShowFeedback("회원가입 성공");
            }
            else if (actionType.ToString() == "login")
            {
                ShowFeedback("로그인 성공");
                isLogin = true;
                yield return new WaitForSeconds(1);
                loginText.gameObject.SetActive(false);
                authPanel.SetActive(false);
                OnLoginSuccess?.Invoke();
            }

        }
        else if (response.TryGetValue("message", out object responseMessage))
        {
            ShowFeedback($"에러 : {responseMessage}");
        }
        else
        {
            ShowFeedback("알 수 없는 오류 발생");
        }
    }

    void ShowFeedback(string message)
    {
        feedbackText.text = message;
        feedbackText.DOFade(1, 1).OnComplete(() => feedbackText.DOFade(0, 2));
    }



}
