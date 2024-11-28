using TMPro;
using UnityEngine;

public class InfoView : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI cointext;
    [SerializeField] private TextMeshProUGUI jeweltext;
    [SerializeField] private TextMeshProUGUI currentatk;
    [SerializeField] private TextMeshProUGUI currenthp;
    [SerializeField] private TextMeshProUGUI afteratk;
    [SerializeField] private TextMeshProUGUI afterhp;
    [SerializeField] private TextMeshProUGUI jewelAtkUpgradetext;
    [SerializeField] private TextMeshProUGUI jewelHPUpgradetext;
    [SerializeField] private TextMeshProUGUI coinAtkUpgradetext;
    [SerializeField] private TextMeshProUGUI coinHPUpgradetext;
    [SerializeField] private TextMeshProUGUI PlayerNameText;

    //private void OnEnable()
    //{
    //    UserData.Instance.OnCharacterDataChanged += UpdateUI;
    //}

    //private void OnDisable()
    //{
    //    if (UserData.Instance != null)
    //        UserData.Instance.OnCharacterDataChanged -= UpdateUI;
    //}

    private void Start()
    {
        UpdateUI();
        UserData.Instance.OnCharacterDataChanged += UpdateUI;
    }
    private void OnDestroy()
    {
        if (UserData.Instance != null)
            UserData.Instance.OnCharacterDataChanged -= UpdateUI;
    }

    private void UpdateUI()
    {
        if (UserData.Instance != null && UserData.Instance.Character != null)
        {
            var character = UserData.Instance.Character;

            PlayerNameText.text = $"{character.PlayerName}";
            cointext.text = $" {character.Coins}";
            jeweltext.text = $" {character.Gems}";
            jewelAtkUpgradetext.text = $"-{character.WeaponEnhancement}";
            jewelHPUpgradetext.text = $"-{character.ArmorEnhancement}";
            coinAtkUpgradetext.text = $"-{character.AttackEnhancement}";
            coinHPUpgradetext.text = $"-{character.HealthEnhancement}";
            currentatk.text = $"{character.AttackPower}";
            currenthp.text = $"{character.MaxHealth}";
            afteratk.text = $"{character.AttackPower + 1}";
            afterhp.text = $"{character.MaxHealth + 5}";
        }
        else
        {
            Debug.LogWarning("UserData.Instance or UserData.Instance.Character is null.");
        }
    }

}
