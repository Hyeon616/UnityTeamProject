using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using TMPro;
using Unity.Services.Core;
using Unity.Services.Lobbies;
using Unity.Services.Lobbies.Models;
using UnityEngine;
using UnityEngine.UI;
using Mirror;


public class LobbyController : MonoBehaviour
{
    [Header("Main")]
    [SerializeField] private GameObject mainLobbyUI;
    [Header("Status")]
    [SerializeField] private GameObject statusEnhancementUI;
    [Header("Equip")]
    [SerializeField] private GameObject equipEnhancementUI;
    [Header("Stat")]
    [SerializeField] private GameObject statUI;
    [Header("PlayerName")]
    [SerializeField] private Button playerNameButton;
    [SerializeField] private TextMeshProUGUI nameText;
    private bool isName = true;

    [Header("Main Button")]
    [SerializeField] private Button backToMainButton;
    [SerializeField] private Button equipButton;
    [SerializeField] private Button statButton;

    [Header("Equip Enhancement")]
    [SerializeField] private Button weaponEnhancement;
    [SerializeField] private Button weaponDiminishment;
    [SerializeField] private Button armorEnhancement;
    [SerializeField] private Button armorDiminishment;

    [Header("Stat Enhancement")]
    [SerializeField] private Button attackEnhancement;
    [SerializeField] private Button attackDiminishment;
    [SerializeField] private Button healthEnhancement;
    [SerializeField] private Button healthDiminishment;

    //private string selectedLobbyId;
    private void OnEnable()
    {

        RegisterListeners();
    }

    private void OnDisable()
    {
        UnregisterListeners();
    }

    private void RegisterListeners()
    {
        // 강화 UI 
        AddListener(equipButton, EquipEnhancementUI);
        AddListener(statButton, StatusEnhancementUI);

        // 뒤로가기
        AddListener(backToMainButton, BackToMainUI);

        // Name 토글
        AddListener(playerNameButton, ChangePlayerNameToPlayerID);

        // 무기 강화
        AddListener(weaponEnhancement, JewelUpGradeATK);
        AddListener(weaponDiminishment, JewelDownGradeATK);
        AddListener(armorEnhancement, JewelUpGradeHP);
        AddListener(armorDiminishment, JewelDownGradeHP);

        // 스탯 강화
        AddListener(attackEnhancement, CoinUpGradeATK);
        AddListener(attackDiminishment, CoinDownGradeATK);
        AddListener(healthEnhancement, CoinUpGradeHP);
        AddListener(healthDiminishment, CoinDownGradeHP);

    }

    private void UnregisterListeners()
    {
        
        RemoveListener(equipButton, EquipEnhancementUI);
        RemoveListener(statButton, StatusEnhancementUI);

        RemoveListener(backToMainButton, BackToMainUI);

        RemoveListener(playerNameButton, ChangePlayerNameToPlayerID);

        RemoveListener(weaponEnhancement, JewelUpGradeATK);
        RemoveListener(weaponDiminishment, JewelDownGradeATK);
        RemoveListener(armorEnhancement, JewelUpGradeHP);
        RemoveListener(armorDiminishment, JewelDownGradeHP);

        RemoveListener(attackEnhancement, CoinUpGradeATK);
        RemoveListener(attackDiminishment, CoinDownGradeATK);
        RemoveListener(healthEnhancement, CoinUpGradeHP);
        RemoveListener(armorDiminishment, CoinDownGradeHP);

    }

    #region Listener 함수
    private void AddListener(Button button, UnityEngine.Events.UnityAction action)
    {
        if (button != null)
        {
            button.onClick.AddListener(action);
        }
        else
        {
            Debug.LogWarning("버튼이 할당 되지 않았습니다.");
        }
    }

    private void RemoveListener(Button button, UnityEngine.Events.UnityAction action)
    {
        if (button != null)
        {
            button.onClick.RemoveListener(action);
        }
    }
    #endregion

    #region MainUI Methods
    private void EquipEnhancementUI()
    {
        mainLobbyUI.SetActive(false);
        equipEnhancementUI.SetActive(true);
        statUI.SetActive(true);
    }

    private void StatusEnhancementUI()
    {
        mainLobbyUI.SetActive(false);
        statusEnhancementUI.SetActive(true);
        statUI.SetActive(true);
    }

    private void BackToMainUI()
    {
        equipEnhancementUI.SetActive(false);
        statusEnhancementUI.SetActive(false);
        statUI.SetActive(false);
        mainLobbyUI.SetActive(true);
    }

    #endregion

    #region Enhancement Methods
    public void JewelUpGradeATK()
    {
        if (UserData.Instance.Character.Gems > UserData.Instance.Character.AttackEnhancement)
        {
            UserData.Instance.Character.AttackPower++;
            UserData.Instance.Character.Gems -= UserData.Instance.Character.AttackEnhancement;
            UserData.Instance.Character.AttackEnhancement++;
            UserData.Instance.SavePlayerData();
            Debug.Log("Jewel upgraded ATK");
        }
        else
        {
            Debug.LogWarning("Not enough gems to upgrade ATK");
        }
    }

    public void JewelDownGradeATK()
    {
        if (UserData.Instance.Character.AttackPower > 0 && UserData.Instance.Character.AttackEnhancement > 0)
        {
            UserData.Instance.Character.AttackPower--;
            UserData.Instance.Character.Gems += (UserData.Instance.Character.AttackEnhancement - 1);
            UserData.Instance.Character.AttackEnhancement--;
            UserData.Instance.SavePlayerData();
            Debug.Log("Jewel downgraded ATK");
        }
        else
        {
            Debug.LogWarning("Cannot downgrade ATK");
        }
    }

    public void JewelUpGradeHP()
    {
        if (UserData.Instance.Character.Gems > UserData.Instance.Character.HealthEnhancement * 5)
        {
            UserData.Instance.Character.MaxHealth += 5;
            UserData.Instance.Character.Gems -= UserData.Instance.Character.HealthEnhancement * 5;
            UserData.Instance.Character.HealthEnhancement++;
            UserData.Instance.SavePlayerData();
            Debug.Log("Jewel upgraded HP");
        }
        else
        {
            Debug.LogWarning("Not enough gems to upgrade HP");
        }
    }

    public void JewelDownGradeHP()
    {
        if (UserData.Instance.Character.MaxHealth > 0 && UserData.Instance.Character.HealthEnhancement > 0)
        {
            UserData.Instance.Character.MaxHealth -= 5;
            UserData.Instance.Character.Gems += (UserData.Instance.Character.HealthEnhancement - 1) * 5;
            UserData.Instance.Character.HealthEnhancement--;
            UserData.Instance.SavePlayerData();
            Debug.Log("Jewel downgraded HP");
        }
        else
        {
            Debug.LogWarning("Cannot downgrade HP");
        }
    }

    public void CoinUpGradeATK()
    {
        if (UserData.Instance.Character.Coins > UserData.Instance.Character.AttackEnhancement * 5)
        {
            UserData.Instance.Character.AttackPower++;
            UserData.Instance.Character.Coins -= UserData.Instance.Character.AttackEnhancement * 5;
            UserData.Instance.Character.AttackEnhancement++;
            UserData.Instance.SavePlayerData();
            Debug.Log("Coin upgraded ATK");
        }
        else
        {
            Debug.LogWarning("Not enough coins to upgrade ATK");
        }
    }

    public void CoinDownGradeATK()
    {
        if (UserData.Instance.Character.AttackPower > 0 && UserData.Instance.Character.AttackEnhancement > 0)
        {
            UserData.Instance.Character.AttackPower--;
            UserData.Instance.Character.Coins += (UserData.Instance.Character.AttackEnhancement - 1) * 5;
            UserData.Instance.Character.AttackEnhancement--;
            UserData.Instance.SavePlayerData();
            Debug.Log("Coin downgraded ATK");
        }
        else
        {
            Debug.LogWarning("Cannot downgrade ATK");
        }
    }

    public void CoinUpGradeHP()
    {
        if (UserData.Instance.Character.Coins > UserData.Instance.Character.HealthEnhancement * 5)
        {
            UserData.Instance.Character.MaxHealth += 5;
            UserData.Instance.Character.Coins -= UserData.Instance.Character.HealthEnhancement * 5;
            UserData.Instance.Character.HealthEnhancement++;
            UserData.Instance.SavePlayerData();
            Debug.Log("Coin upgraded HP");
        }
        else
        {
            Debug.LogWarning("Not enough coins to upgrade HP");
        }
    }

    public void CoinDownGradeHP()
    {
        if (UserData.Instance.Character.MaxHealth > 0 && UserData.Instance.Character.HealthEnhancement > 0)
        {
            UserData.Instance.Character.MaxHealth -= 5;
            UserData.Instance.Character.Coins += (UserData.Instance.Character.HealthEnhancement - 1) * 5;
            UserData.Instance.Character.HealthEnhancement--;
            UserData.Instance.SavePlayerData();
            Debug.Log("Coin downgraded HP");
        }
        else
        {
            Debug.LogWarning("Cannot downgrade HP");
        }
    }
    #endregion

    #region Player Info
    private void ChangePlayerNameToPlayerID()
    {
        isName = !isName;

        if (UserData.Instance != null && UserData.Instance.Character != null)
        {
            nameText.text = isName ? UserData.Instance.Character.PlayerName : UserData.Instance.Character.PlayerId;
        }
        else
        {
            nameText.text = "Unknown";
            //Debug.LogError("Character data is not loaded in UserData.");
        }
    }
    #endregion


}



