using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class GameStartMenu : MonoBehaviour
{

    [Header("Setting")]
    [SerializeField] private GameObject settingUI;

    [Header("GameMenu")]
    [SerializeField] private GameObject gameStartUI;
    [SerializeField] private Button gameStartButton;

    [Header("Game Start")]
    [SerializeField] private Button closeGameStartUIButton;


    private void OnEnable()
    {
        gameStartButton.onClick.AddListener(GameStartEnter);
        closeGameStartUIButton.onClick.AddListener(OnClickGameStartExit);

    }

    private void OnDisable()
    {
        gameStartButton.onClick.RemoveListener(GameStartEnter);
        closeGameStartUIButton.onClick.RemoveListener(OnClickGameStartExit);
    }


    private void GameStartEnter()
    {
        gameStartUI.SetActive(true);
        settingUI.SetActive(false);
    }

    private void OnClickGameStartExit()
    {
        gameStartUI.SetActive(false);
        settingUI.SetActive(true);
    }


}
