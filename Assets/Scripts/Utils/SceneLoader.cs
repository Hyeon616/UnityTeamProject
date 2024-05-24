using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using DG.Tweening;
using TMPro;

public class SceneLoader : Singleton<SceneLoader>
{
    [SerializeField] private GameObject loadingScreen;
    [SerializeField] private CanvasGroup loadingScreenCanvasGroup;
    [SerializeField] private TextMeshProUGUI loadingText;

    public void LoadSceneAsync(string sceneName)
    {
        StartCoroutine(LoadSceneCoroutine(sceneName));
    }

    private IEnumerator LoadSceneCoroutine(string sceneName)
    {
        // �ε� ȭ�� ���̵���
        loadingScreen.SetActive(true);
        loadingScreenCanvasGroup.alpha = 0;
        loadingScreenCanvasGroup.DOFade(1, 1f);

        // �ؽ�Ʈ ������Ʈ �ڷ�ƾ ����
        Coroutine loadingTextCoroutine = StartCoroutine(UpdateLoadingText());

        AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(sceneName);
        asyncLoad.allowSceneActivation = false;

        while (!asyncLoad.isDone)
        {
            if (asyncLoad.progress >= 0.9f)
            {
                asyncLoad.allowSceneActivation = true;
            }
            yield return null;
        }

        // �ؽ�Ʈ ������Ʈ �ڷ�ƾ ����
        StopCoroutine(loadingTextCoroutine);

        // �ε� �ؽ�Ʈ ���̵�ƿ�
        loadingText.DOFade(0, 1f);

        // �ε� ȭ�� ���̵�ƿ�
        yield return loadingScreenCanvasGroup.DOFade(0, 1f).WaitForCompletion();
        loadingScreen.SetActive(false);
    }

    private IEnumerator UpdateLoadingText()
    {
        while (true)
        {
            loadingText.text = "Loading.";
            yield return new WaitForSeconds(0.3f);
            loadingText.text = "Loading..";
            yield return new WaitForSeconds(0.3f);
            loadingText.text = "Loading...";
            yield return new WaitForSeconds(0.3f);
        }
    }

}
