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

    private void UpdateCharacter()
    {
        Character = Character; 
    }

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
        }
        else
        {
            Debug.LogError("DB 업데이트 실패");
        }

        
    }

    #region Stat

    public void JewelUpGradeATK()
    {
        if (Character.Gems > Character.AttackEnhancement)
        {
            Character.AttackPower++;
            Character.Gems -= Character.AttackEnhancement;
            Character.AttackEnhancement++;
            UpdateCharacter();

            SavePlayerData();
        }
    }

    public void JewelDownGradeATK()
    {
        if (Character.AttackPower > 0 && Character.AttackEnhancement > 0)
        {
            Character.AttackPower--;
            Character.Gems += (Character.AttackEnhancement - 1);
            Character.AttackEnhancement--;
            UpdateCharacter();

            SavePlayerData();
        }
    }

    public void JewelUpGradeHP()
    {
        if (Character.Gems > Character.HealthEnhancement * 5)
        {
            Character.MaxHealth += 5;
            Character.Gems -= Character.HealthEnhancement * 5;
            Character.HealthEnhancement++;
            UpdateCharacter();

            SavePlayerData();
        }
    }

    public void JewelDownGradeHP()
    {
        if (Character.MaxHealth > 0 && Character.HealthEnhancement > 0)
        {
            Character.MaxHealth -= 5;
            Character.Gems += (Character.HealthEnhancement - 1) * 5;
            Character.HealthEnhancement--;
            UpdateCharacter();

            SavePlayerData();
        }
    }

    public void CoinUpGradeATK()
    {
        if (Character.Coins > Character.AttackEnhancement * 5)
        {
            Character.AttackPower++;
            Character.Coins -= Character.AttackEnhancement * 5;
            Character.AttackEnhancement++;
            UpdateCharacter();

            SavePlayerData();
        }
    }

    public void CoinDownGradeATK()
    {
        if (Character.AttackPower > 0 && Character.AttackEnhancement > 0)
        {
            Character.AttackPower--;
            Character.Coins += (Character.AttackEnhancement - 1) * 5;
            Character.AttackEnhancement--;
            UpdateCharacter();

            SavePlayerData();
        }
    }

    public void CoinUpGradeHP()
    {
        if (Character.Coins > Character.HealthEnhancement * 5)
        {
            Character.MaxHealth += 5;
            Character.Coins -= Character.HealthEnhancement * 5;
            Character.HealthEnhancement++;
            UpdateCharacter();

            SavePlayerData();
        }
    }

    public void CoinDownGradeHP()
    {
        if (Character.MaxHealth > 0 && Character.HealthEnhancement > 0)
        {
            Character.MaxHealth -= 5;
            Character.Coins += (Character.HealthEnhancement - 1) * 5;
            Character.HealthEnhancement--;
            UpdateCharacter();

            SavePlayerData();
        }
    }

    public void PlusCoins()
    {
        Character.Coins++;
        UpdateCharacter();

        SavePlayerData();
    }

    public void PlusJewels()
    {
        Character.Gems++;
        UpdateCharacter();

        SavePlayerData();
    }

    #endregion
}
