using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class BossBar : MonoBehaviour
{
    [SerializeField] public Boss boss;
    [SerializeField] public Image Image1;
    [SerializeField] public Image Image2;
    [SerializeField] public Text HpCount; // Text ������Ʈ ���� ���
    private bool setcolor = false;

    TextMeshProUGUI textMeshProUGUI;

    float bossFixHp = 5000f;
    float bossCurrentHp = 5000f;
    // Start is called before the first frame update
    void Start()
    {
        Boss bossinfo = boss.GetComponent<Boss>();
        textMeshProUGUI = GetComponent<TextMeshProUGUI>();
        textMeshProUGUI.text = $"{bossCurrentHp.ToString("F0")} / {bossFixHp.ToString("F0")}";
        // bossCurrentHp = bossinfo.currentHealth;
        //bossFixHp = bossinfo.fixHealth;
        // RefreshBossHp(bossinfo);

    }

    // Update is called once per frame
    void Update()
    {
        if (textMeshProUGUI == null)
            Debug.Log("No");
        

    }
    public void RefreshBossHp(Boss boss,float currenthp =0)
    {
        hpProgress(currenthp);
        bossFixHp = boss.fixHealth;
        bossCurrentHp = boss.currentHealth;
        textMeshProUGUI.text = $"{bossCurrentHp.ToString("F0")} / {bossFixHp.ToString("F0")}";
    }

    public void hpProgress(float currenthp)
    {
        int displayNumber = Mathf.FloorToInt(currenthp / 100);
               float currentUnitHealth = currenthp % 100;

        // ü�� ���� ���̸� ���� ���� ü�¿� ���� ������Ʈ
        if (Image2 != null)
        {
            Debug.Log(currentUnitHealth);
            Image2.fillAmount = currentUnitHealth / 100f;

            if (currentUnitHealth == 90 )
            {
                // Image2�� ������ Image1�� �������� ����
                Image2.color = (!setcolor)?Image2.color : Image1.color;
                // Image1�� ������ ������ �������� ����
                Image1.color = GetRandomColorExcluding(Image2.color);
                setcolor = true;
            }
        }
        HpCount.text = $"x {displayNumber}";
        //healthText.text = "x" + displayNumber.ToString();

        // ���� ü���� 10 ���� ���� ������ ��ȯ
        //float currentUnitHealth = currentHealth % 100;

        // ü�� ���� ���̸� ���� ���� ü�¿� ���� ������Ʈ
        //healthBar.fillAmount = currentUnitHealth / 100f;
    }
    private Color GetRandomColorExcluding(Color excludeColor)
    {
        Color newColor;
        do
        {
            newColor = new Color(Random.value, Random.value, Random.value);
        } while (newColor == excludeColor);
        return newColor;
    }
}
