using JetBrains.Annotations;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


public class LobbyController : MonoBehaviour
{
    public GameObject lobbyMainUI;
    public GameObject lobbyStatusUI;
    public GameObject lobbyEquipUI;
    public GameObject settingUI;
    public GameObject gameSelect;
    public GameObject statUI;
    public GameObject stageSelect;
    public GameObject tutorialSelect;


    //Ȩ��ư
    public void OnClickHome()
    {
        settingUI.SetActive(false);
        lobbyMainUI.SetActive(true);
        lobbyEquipUI.SetActive(false);
        lobbyStatusUI.SetActive(false);
        statUI.SetActive(false);
    }
    //������ư
    public void OnClickSet()
    {
        settingUI.SetActive(true);
    }
    //���� ����
    public void OnClickSetout()
    {
        settingUI.SetActive(false);
    }
    //�ڷΰ����ư
    public void OnClickBackSpace()
    {

        lobbyEquipUI.SetActive(false);
        lobbyStatusUI.SetActive(false);
        statUI.SetActive(false);
        lobbyMainUI.SetActive(true);
    }
    //Ÿ��Ʋ��ư
    public void OnClickTitle()
    {
        FadeInFadeOutSceneManager.Instance.ChangeScene("StartScene");
    }
    //�����ư
    public void OnClickQuit()
    {

    }
    //����ư
    public void OnClickEquip()
    {
        lobbyMainUI.SetActive(false);
        lobbyEquipUI.SetActive(true);
        statUI.SetActive(true);
    }
    //���ȹ�ư
    public void OnClickStatus()
    {
        lobbyMainUI.SetActive(false);
        lobbyStatusUI.SetActive(true);
        statUI.SetActive(true);
    }
    //���۹�ư
    public void OnClickGameStartEnter()
    {
        gameSelect.SetActive(true);
        //tutorialSelect.SetActive(true);
        //stageSelect.SetActive(false);
    }
    
    //������������Ʈ
    public void OnClickStageSelect()
    {
        tutorialSelect.SetActive(false);
        stageSelect.SetActive(true);
    }
    //���������ڷΰ���
    public void OnClickStageBackSpace()
    {
        tutorialSelect.SetActive(true);
        stageSelect.SetActive(false);
    }
    //���۹�ư ����
    public void OnClickGameStartExit()
    {
        gameSelect.SetActive(false);
    }
    
}
