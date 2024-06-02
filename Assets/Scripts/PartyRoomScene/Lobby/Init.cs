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

            if (AuthenticationService.Instance.IsSignedIn)
            {
                Debug.Log("Already signed in.");
            }
            else
            {
                await AuthenticationService.Instance.SignInAnonymouslyAsync();
            }

            IsAuthenticated = AuthenticationService.Instance.IsSignedIn;
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
