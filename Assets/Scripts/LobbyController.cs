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

    
    void Start()
    {
        
    }

    
    void Update()
    {
        
    }

    //Ȩ��ư
    void OnClickHome()
    {
        settingUI.SetActive(false);
        lobbyMainUI.SetActive(true);
        lobbyEquipUI.SetActive(false);
        lobbyStatusUI.SetActive(false);
    }
    //������ư
    void OnClickEnterSetting()
    {
        settingUI.SetActive(true);
    }
    void OnClickExitSetting()
    {
        settingUI.SetActive(false);
    }
    //�ڷΰ����ư
    void OnClickBackSpace()
    {
        lobbyMainUI.SetActive(true);
        lobbyEquipUI.SetActive(false);
        lobbyStatusUI.SetActive(false);
    }
    //Ÿ��Ʋ��ư
    void OnClickTitle()
    {
        FadeInFadeOutSceneManager.Instance.ChangeScene("StartScene");
    }
    //�����ư
    void OnClickQuit()
    {

    }
    //����ư
    void OnClickEquip()
    {
        lobbyMainUI.SetActive(false);
        lobbyEquipUI.SetActive(true);
    }
    //���ȹ�ư
    void OnClickStatus()
    {
        lobbyMainUI.SetActive(false);
        lobbyStatusUI.SetActive(true);
    }
    //���ӽ���
    void OnClickGameStart()
    {

    }
}
