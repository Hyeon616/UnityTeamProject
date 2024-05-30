using System;
using System.Threading.Tasks;
using Unity.Services.Authentication;
using Unity.Services.Core;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Init : MonoBehaviour
{

    public static bool IsAuthenticated { get; private set; }

    async void Start()
    {
        await InitializeServices();
    }

    private async Task InitializeServices()
    {
        try
        {
            await UnityServices.InitializeAsync();
            Debug.Log("Unity Services Initialized");

            if (AuthenticationService.Instance.IsSignedIn)
            {
                Debug.Log("Already signed in.");
            }
            else
            {
                await AuthenticationService.Instance.SignInAnonymouslyAsync();
                Debug.Log($"Signed in: {AuthenticationService.Instance.PlayerId}");
            }

            IsAuthenticated = AuthenticationService.Instance.IsSignedIn;
            Debug.Log($"Authentication Status: {IsAuthenticated}");
        }
        catch (AuthenticationException ex)
        {
            Debug.LogError($"AuthenticationException: {ex.Message}");
        }
        catch (RequestFailedException ex)
        {
            Debug.LogError($"RequestFailedException: {ex.Message}");
        }
    }

}
