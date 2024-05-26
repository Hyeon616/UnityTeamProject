using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.UI;
public class SettingMenu : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] private GameObject settingUI;
    [SerializeField] private Button settingActive;
    [SerializeField] private Button settingInActive;

    [Header("Resolution Button")]
    [SerializeField] private Sprite[] sprites;
    [SerializeField] private Button resolutionHD;
    [SerializeField] private Button resolutionFHD;
    [SerializeField] private Button resolutionFullScreen;

    [Header("Volume")]
    [SerializeField] private Slider masterVolume;
    [SerializeField] private Slider bgm;
    [SerializeField] private Slider sfx;
    [SerializeField] private AudioMixer audioMixer;

    [Header("Title")]
    [SerializeField] private Button titleButton;
    [Header("Quit")]
    [SerializeField] private Button quitButton;

    private void OnEnable()
    {
        // UI
        settingActive.onClick.AddListener(ShowSettingUI);
        settingInActive.onClick.AddListener(HideSettingUI);

        // ��ư Ŭ�� �̺�Ʈ�� ������ �߰�
        resolutionHD.onClick.AddListener(OnClickResolutionHD);
        resolutionFHD.onClick.AddListener(OnClickResolutionFHD);
        resolutionFullScreen.onClick.AddListener(OnClickResolutionFullScreen);

        // �����̴� �� ���� �̺�Ʈ�� ������ �߰�
        masterVolume.onValueChanged.AddListener(SetMasterVolume);
        bgm.onValueChanged.AddListener(SetBGMVolume);
        sfx.onValueChanged.AddListener(SetSFXVolume);

        // Ÿ��Ʋ
        titleButton.onClick.AddListener(ClickTitle);
        // ����
        quitButton.onClick.AddListener(ClickQuit);
    }

    private void OnDisable()
    {
        // UI
        settingActive.onClick.RemoveListener(ShowSettingUI);
        settingInActive.onClick.RemoveListener(HideSettingUI);

        // ��ư Ŭ�� �̺�Ʈ���� ������ ����
        resolutionHD.onClick.RemoveListener(OnClickResolutionHD);
        resolutionFHD.onClick.RemoveListener(OnClickResolutionFHD);
        resolutionFullScreen.onClick.RemoveListener(OnClickResolutionFullScreen);

        // �����̴� �� ���� �̺�Ʈ���� ������ ����
        masterVolume.onValueChanged.RemoveListener(SetMasterVolume);
        bgm.onValueChanged.RemoveListener(SetBGMVolume);
        sfx.onValueChanged.RemoveListener(SetSFXVolume);

        // Ÿ��Ʋ
        titleButton.onClick.RemoveListener(ClickTitle);
        // ����
        quitButton.onClick.RemoveListener(ClickQuit);

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

    //Ÿ��Ʋ��ư
    private void ClickTitle()
    {
        //Ÿ��Ʋ��
        SceneLoader.Instance.LoadSceneAsync("StartScene");
    }
    //�����ư
    private void ClickQuit()
    {
        // ����� ���¿��� ������ �����մϴ�.
        Application.Quit();

        // ������ ��忡�� ������ �����մϴ�.
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#endif
    }

    private void ShowSettingUI()
    {
        settingUI.SetActive(true); // Setting UI Ȱ��ȭ
    }

    private void HideSettingUI()
    {
        settingUI.SetActive(false); // Setting UI ��Ȱ��ȭ
    }

    private void SetMasterVolume(float value)
    {
        audioMixer.SetFloat("MasterVolume", Mathf.Log10(value) * 20); // �����̴� ���� �α� ��ȯ�Ͽ� ����
    }

    private void SetBGMVolume(float value)
    {
        audioMixer.SetFloat("BGMVolume", Mathf.Log10(value) * 20); // �����̴� ���� �α� ��ȯ�Ͽ� ����
    }

    private void SetSFXVolume(float value)
    {
        audioMixer.SetFloat("SFXVolume", Mathf.Log10(value) * 20); // �����̴� ���� �α� ��ȯ�Ͽ� ����
    }

}
