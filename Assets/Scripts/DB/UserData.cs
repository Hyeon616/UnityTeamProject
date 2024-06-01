using System;
using System.Collections;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Networking;

public class UserData : Singleton<UserData>
{
    public string UserId { get; set; }
    public CharacterData Character { get; set; }

    private const string ApiEndpoint = "/api/players";

    private string SaveDataUrl => $"{RemoteConfigManager.ServerUrl}{ApiEndpoint}";
    private string LoadDataUrl => $"{RemoteConfigManager.ServerUrl}{ApiEndpoint}";

    IEnumerator Start()
    {
        yield return new WaitUntil(() => !string.IsNullOrEmpty(RemoteConfigManager.ServerUrl));

        Debug.Log($"Server URL: {RemoteConfigManager.ServerUrl}");
        Debug.Log($"Save Data URL: {SaveDataUrl}");
        Debug.Log($"Load Data URL: {LoadDataUrl}");
    }

    
    public void LoadPlayerData(string userId, CharacterData character)
    {
        UserId = userId;
        Character = character;
    }

    public void SavePlayerData()
    {
        StartCoroutine(SavePlayerDataToDatabase(Character));
    }

    private IEnumerator SavePlayerDataToDatabase(CharacterData characterData)
    {
        string url = $"{SaveDataUrl}/{UserId}";
        string jsonData = JsonUtility.ToJson(characterData);

        Debug.Log($"Saving player data to URL: {url}");
        Debug.Log($"JSON data: {jsonData}");

        using (UnityWebRequest request = new UnityWebRequest(url, "PUT"))
        {
            byte[] bodyRaw = System.Text.Encoding.UTF8.GetBytes(jsonData);
            request.uploadHandler = new UploadHandlerRaw(bodyRaw);
            request.downloadHandler = new DownloadHandlerBuffer();
            request.SetRequestHeader("Content-Type", "application/json");

            yield return request.SendWebRequest();

            if (request.result == UnityWebRequest.Result.Success)
            {
                Debug.Log("Player data saved successfully.");
            }
            else
            {
                LogError(request, "Error saving player data");
            }
        }
    }

    public async Task<string> LoadPlayerNameFromServer(string playerId)
    {
        CharacterData characterData = await LoadPlayerDataFromDatabase(playerId);
        return characterData?.PlayerName ?? "NoName Player";
    }



    public async Task LoadPlayerDataFromServer(string playerId)
    {
        Character = await LoadPlayerDataFromDatabase(playerId);
        if (Character != null)
        {
            Debug.Log("Player data loaded successfully.");
        }
        else
        {
            Debug.LogWarning("Failed to load player data. Character data is null.");
        }
    }



    private async Task<CharacterData> LoadPlayerDataFromDatabase(string playerId)
    {
        string url = $"{LoadDataUrl}/ugs/{playerId}"; // URL 경로 변경

        using (UnityWebRequest request = UnityWebRequest.Get(url))
        {
            Debug.Log($"Sending request to URL: {url}");
            await request.SendWebRequestAsync();

            if (request.result == UnityWebRequest.Result.Success)
            {
                string jsonResult = request.downloadHandler.text;
                Debug.Log($"Received response: {jsonResult}");
                return JsonUtility.FromJson<CharacterData>(jsonResult);
            }
            else
            {
                Debug.LogError($"Error loading player data: {request.error}");
                Debug.LogError($"Response Code: {request.responseCode}");
                Debug.LogError($"Response Text: {request.downloadHandler.text}");
                return null;
            }
        }
    }

    private void LogError(UnityWebRequest request, string message)
    {
        Debug.LogError($"{message}: {request.error}");
        Debug.LogError($"Response Code: {request.responseCode}");
        Debug.LogError($"Response Text: {request.downloadHandler.text}");
    }
}

public static class UnityWebRequestExtensions
{
    public static Task<UnityWebRequest> SendWebRequestAsync(this UnityWebRequest request)
    {
        var tcs = new TaskCompletionSource<UnityWebRequest>();
        request.SendWebRequest().completed += operation =>
        {
            if (request.result == UnityWebRequest.Result.ConnectionError || request.result == UnityWebRequest.Result.ProtocolError)
            {
                tcs.SetException(new Exception(request.error));
            }
            else
            {
                tcs.SetResult(request);
            }
        };
        return tcs.Task;
    }
}

public class UnityWebRequestException : System.Exception
{
    public UnityWebRequest Request { get; }

    public UnityWebRequestException(UnityWebRequest request) : base(request.error)
    {
        Request = request;
    }
}