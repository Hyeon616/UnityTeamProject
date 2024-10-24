using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
public class SkillControl : MonoBehaviour
{
    //비활성화 연출에 사용될 이미지를 타나내거나 숨겨주기 위해 게임오브젝트 만듬
    public GameObject[] hideSkillButtons;

    //TextPro가 처음에는 비활성화 되어 있어서 바로 컴포넌트를 가져올 수 없어서 게임오브젝트를 만들고 textpros작성
    public GameObject[] textPros;
    public TextMeshProUGUI[] hideSkillTimeTexts;
    public Image[] hideSkillImages;
    NetworkPlayerAnimator playerSkill;

    //스킬 사용중인지 
    public bool[] isHideSkills = { false, false, false, false };

    private float[] skillTimes = { 2, 4, 4, 0 };
    public float[] getSkillTimes = { 0, 0, 0, 0, };

    private bool isInitialized = false;

    void Start()
    {
        // UI 초기화
        for (int i = 0; i < textPros.Length; i++)
        {
            hideSkillTimeTexts[i] = textPros[i].GetComponent<TextMeshProUGUI>();
            hideSkillButtons[i].SetActive(false);
        }

        // 플레이어 찾기 시작
        StartCoroutine(WaitForPlayerSpawn());
    }

    private IEnumerator WaitForPlayerSpawn()
    {
        // GameManager의 LocalPlayer가 설정될 때까지 대기
        while (GameManager.Instance == null || GameManager.Instance.LocalPlayer == null)
        {
            yield return new WaitForSeconds(0.1f);
        }

        // LocalPlayer 참조
        playerSkill = GameManager.Instance.LocalPlayer;
        isInitialized = true;
    }

    void Update()
    {
        // 초기화가 완료된 경우에만 스킬 체크 실행
        if (isInitialized)
        {
            HideSkillChk();
        }
    }

    public void HideSkillSetting(int skillNum)
    {
        if (!isHideSkills[skillNum])
        {
            switch (skillNum)
            {
              case 0:
                    playerSkill.OnDash();
                    break;  
                case 1:
                    playerSkill.OnSkillA();
                    break; 
                case 2:
                    playerSkill.OnSkillB();
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
