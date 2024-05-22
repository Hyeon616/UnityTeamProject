using System;
using Unity.Services.Authentication;
using Unity.Services.Core;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Init : MonoBehaviour
{

    async void Start()
    {
        try
        {
            await UnityServices.InitializeAsync();

            if (UnityServices.State == ServicesInitializationState.Initialized)
            {
                AuthenticationService.Instance.SignedIn += OnSignedIn;
                await AuthenticationService.Instance.SignInAnonymouslyAsync();

                if (AuthenticationService.Instance.IsSignedIn)
                {
                    // ����� �̸� �ʱ�ȭ
                    InitializeUserName();
                }
            }
        }
        catch (Exception ex)
        {
            Debug.Log($"�÷��̾� ���� �ʱ�ȭ ���� : {ex.Message}");
        }

    }
    private void OnDisable()
    {
        if (AuthenticationService.Instance != null)
        {
            AuthenticationService.Instance.SignedIn -= OnSignedIn;
        }
    }

    private void InitializeUserName()
    {
        string userName = PlayerPrefs.GetString("Username");
        if (string.IsNullOrEmpty(userName))
        {
            userName = "Player";
            PlayerPrefs.SetString("Username", userName);
        }
    }

    private void OnSignedIn()
    {
        Debug.Log($"Player Id : {AuthenticationService.Instance.PlayerId}");
        Debug.Log($"Token : {AuthenticationService.Instance.AccessToken}");
    }

}
