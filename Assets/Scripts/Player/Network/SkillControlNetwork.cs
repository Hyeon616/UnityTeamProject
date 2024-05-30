using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SkillControlNetwork : MonoBehaviour
{
    //비활성화 연출에 사용될 이미지를 타나내거나 숨겨주기 위해 게임오브젝트 만듬
    public GameObject[] hideSkillButtons;

    //TextPro가 처음에는 비활성화 되어 있어서 바로 컴포넌트를 가져올 수 없어서 게임오브젝트를 만들고 textpros작성
    public GameObject[] textPros;
    public TextMeshProUGUI[] hideSkillTimeTexts;
    public Image[] hideSkillImages;
    NetworkPlayerController playerSkill;

    //스킬 사용중인지 
    public bool[] isHideSkills = { false, false, false, false };

    public float[] skillTimes = { 2, 4, 4, 0 };
    public float[] getSkillTimes = { 0, 0, 0, 0, };

    

    void Update()
    {
        HideSkillChk();
    }

    public void HideSkillSetting(int skillNum)
    {
        //if (!isHideSkills[skillNum])
        //{
        //    if (IsServer)
        //    {
        //        HandleSkillActivation(skillNum);
        //    }
        //    else
        //    {
        //        HandleSkillActivationServerRpc(skillNum);
        //    }
        //}
        
    }
    

    private void HandleSkillActivation(int skillNum)
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

        HideSkillServerSide(skillNum);
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
    private void HideSkillServerSide(int skillNum)
    {
        hideSkillButtons[skillNum].SetActive(true);
        getSkillTimes[skillNum] = skillTimes[skillNum];
        isHideSkills[skillNum] = true;
    }
    
}
