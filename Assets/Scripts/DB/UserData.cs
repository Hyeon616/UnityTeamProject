using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Networking;

public class UserData : Singleton<UserData>
{
    public string UserId { get; set; }
    private CharacterData _character;
    public CharacterData Character
    {
        get => _character;
        private set
        {
            _character = value;
            OnCharacterDataChanged?.Invoke();
        }
    }

    private void Awake()
    {
        Character = new CharacterData();
    }


    public event Action OnCharacterDataChanged;


    public void LoadPlayerData(string userId, CharacterData characterData)
    {
        UserId = userId;
        Character = characterData;
        OnCharacterDataChanged?.Invoke();
    }

    public async Task SavePlayerData()
    {
        
        var saveRequest = new
        {
            action = "save",
            userId = UserId,
            characterData = Character
        };

        string jsonRequest = JsonConvert.SerializeObject(saveRequest);
        string response = await ServerConnector.Instance.SendMessage(jsonRequest);

        var responseData = JsonConvert.DeserializeObject<Dictionary<string, object>>(response);
        if (responseData.TryGetValue("status", out object status) && status.ToString() == "success")
        {
            OnCharacterDataChanged?.Invoke();
            Debug.Log("값변경");
        }
        else
        {
            Debug.LogError("DB 업데이트 실패");
        }

        
    }

}
