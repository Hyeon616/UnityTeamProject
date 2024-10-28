using Newtonsoft.Json;
using System;

[Serializable]
public class RegisterData
{
    public string Username;
    public string Password;
    public string PlayerName;

}

[Serializable]
public class LoginData
{
    public string Username;
    public string Password;
}

[Serializable]
public class LoginResponse
{
    public string UserId;
    public string PlayerName;
    public CharacterData Character;
}

[Serializable]
public class ErrorResponse
{
    public string errorCode;
    public string message;
}

[Serializable]
public class CharacterData
{
   
    [JsonProperty("PlayerName")]
    public string PlayerName { get; set; }

    [JsonProperty("PlayerId")]
    public string PlayerId { get; set; }

    [JsonProperty("Gems")]
    public int Gems { get; set; }

    [JsonProperty("Coins")]
    public int Coins { get; set; }

    [JsonProperty("MaxHealth")]
    public int MaxHealth { get; set; }

    [JsonProperty("HealthEnhancement")]
    public int HealthEnhancement { get; set; }

    [JsonProperty("AttackPower")]
    public int AttackPower { get; set; }

    [JsonProperty("AttackEnhancement")]
    public int AttackEnhancement { get; set; }

    [JsonProperty("WeaponEnhancement")]
    public int WeaponEnhancement { get; set; }

    [JsonProperty("ArmorEnhancement")]
    public int ArmorEnhancement { get; set; }
}

[Serializable]
public class SaveUGSPlayerIDRequest
{
    public string UserID;
    public string UGSPlayerID;
}