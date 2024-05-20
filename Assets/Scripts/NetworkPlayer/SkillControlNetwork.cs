using System.Collections;
using TMPro;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

public class SkillControlNetwork : NetworkBehaviour
{
    //��Ȱ��ȭ ���⿡ ���� �̹����� Ÿ�����ų� �����ֱ� ���� ���ӿ�����Ʈ ����
    public GameObject[] hideSkillButtons;

    //TextPro�� ó������ ��Ȱ��ȭ �Ǿ� �־ �ٷ� ������Ʈ�� ������ �� ��� ���ӿ�����Ʈ�� ����� textpros�ۼ�
    public GameObject[] textPros;
    public TextMeshProUGUI[] hideSkillTimeTexts;
    public Image[] hideSkillImages;
    NetworkPlayerController playerSkill;

    //��ų ��������� 
    public bool[] isHideSkills = { false, false, false, false };

    private float[] skillTimes = { 2, 4, 4, 0 };
    public float[] getSkillTimes = { 0, 0, 0, 0, };

    public override void OnNetworkSpawn()
    {
        if (IsLocalPlayer)
        {
            playerSkill = transform.root.GetComponent<NetworkPlayerController>();
            //playerSkill = GameObject.FindWithTag("Player").GetComponent<NetworkPlayerController>();
            for (int i = 0; i < textPros.Length; i++)
            {
                hideSkillTimeTexts[i] = textPros[i].GetComponent<TextMeshProUGUI>();
                hideSkillButtons[i].SetActive(false);
            }
        }

    }

    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        HideSkillChk();
    }
    public void HideSkillSetting(int skillNum)
    {
        if (!isHideSkills[skillNum])
        {
            switch (skillNum)
            {
                case 0:
                    playerSkill.Dash();
                    break;
                case 1:
                    playerSkill.SkillA();
                    break;
                case 2:
                    playerSkill.SkillB();
                    break;
                case 3:
                    playerSkill.SkillClick();
                    break;
            }
        }
        hideSkillButtons[skillNum].SetActive(true);
        getSkillTimes[skillNum] = skillTimes[skillNum];
        isHideSkills[skillNum] = true;
    }

    private void HideSkillChk()
    {
        if (isHideSkills[0])
        {
            StartCoroutine(SkillTimeChk(0));
        }
        if (isHideSkills[1])
        {
            StartCoroutine(SkillTimeChk(1));
        }
        if (isHideSkills[2])
        {
            StartCoroutine(SkillTimeChk(2));
        }
        if (isHideSkills[3])
        {
            StartCoroutine(SkillTimeChk(3));
        }
    }

    IEnumerator SkillTimeChk(int skillNum)
    {
        yield return null;


        if (getSkillTimes[skillNum] > 0 || skillNum == 3)
        {
            getSkillTimes[skillNum] -= Time.deltaTime;
            if (getSkillTimes[skillNum] < 0)
            {
                /*Debug.Log("test1");*/
                getSkillTimes[skillNum] = 0;
                isHideSkills[skillNum] = false;
                hideSkillButtons[skillNum].SetActive(false);
            }

            hideSkillTimeTexts[skillNum].text = getSkillTimes[skillNum].ToString("00");
            float time = getSkillTimes[skillNum] / skillTimes[skillNum];
            hideSkillImages[skillNum].fillAmount = time;
        }
    }
}
