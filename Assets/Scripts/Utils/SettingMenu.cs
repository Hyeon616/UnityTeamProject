using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
public class SettingMenu : MonoBehaviour
{
    public Sprite[] sprites;
    public Button resolutionHD;
    public Button resolutionFHD;
    public Button resolutionFullScreen;


    public Slider masterVolume;
    public Slider bgm;
    public Slider sfx;

    private void OnEnable()
    {
        // ��ư Ŭ�� �̺�Ʈ�� ������ �߰�
        resolutionHD.onClick.AddListener(OnClickResolutionHD);
        resolutionFHD.onClick.AddListener(OnClickResolutionFHD);
        resolutionFullScreen.onClick.AddListener(OnClickResolutionFullScreen);
    }

    private void OnDisable()
    {
        // ��ư Ŭ�� �̺�Ʈ���� ������ ����
        resolutionHD.onClick.RemoveListener(OnClickResolutionHD);
        resolutionFHD.onClick.RemoveListener(OnClickResolutionFHD);
        resolutionFullScreen.onClick.RemoveListener(OnClickResolutionFullScreen);
    }

    public void OnClickResolutionHD()
    {
        resolutionHD.GetComponent<Image>().sprite = sprites[0];
        resolutionFHD.GetComponent<Image>().sprite = sprites[1];
        resolutionFullScreen.GetComponent<Image>().sprite = sprites[1];
        Screen.SetResolution(1280, 720, false);
        
    }
    public void OnClickResolutionFHD()
    {
        resolutionHD.GetComponent<Image>().sprite = sprites[1];
        resolutionFHD.GetComponent<Image>().sprite = sprites[0];
        resolutionFullScreen.GetComponent<Image>().sprite = sprites[1];
        Screen.SetResolution(1920, 1080, false);
    }

    public void OnClickResolutionFullScreen()
    {
        resolutionHD.GetComponent<Image>().sprite = sprites[1];
        resolutionFHD.GetComponent<Image>().sprite = sprites[1];
        resolutionFullScreen.GetComponent<Image>().sprite = sprites[0];
        Screen.SetResolution(1920, 1080, true);
    }

}
