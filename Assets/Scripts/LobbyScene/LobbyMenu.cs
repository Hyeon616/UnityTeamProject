using Mirror;
using System;
using System.Collections.Generic;
using TMPro;
using Unity.Services.Lobbies.Models;
using UnityEngine;
using UnityEngine.UI;

public class LobbyMenu : MonoBehaviour
{
    [Header("Game Start")]
    [SerializeField] private Button createRoomButton;
    [SerializeField] private Button ParticipateRoomButton;

    [Header("Lobby")]
    [SerializeField] private Button backToGameStart;
    [SerializeField] private GameObject JoinMenuUI;
    [SerializeField] private GameObject lobbyRoomUI;
    [SerializeField] private Button startSceneButton;
    [SerializeField] private Button Button_JoinRoom;
    [SerializeField] private Button BackRoomButton;


    private void OnEnable()
    {
        createRoomButton.onClick.AddListener(OnClickedCreateRoomUIButton);
        ParticipateRoomButton.onClick.AddListener(OnClickedJoinRoomUIButton);

        backToGameStart.onClick.AddListener(BackToGameStartUI);

        startSceneButton.onClick.AddListener(OnStartGame);
        Button_JoinRoom.onClick.AddListener(JoinRoomButtonClicked);
        BackRoomButton.onClick.AddListener(OnClickBackRoomButton);
    }

    
    private void OnDisable()
    {
        createRoomButton.onClick.RemoveListener(OnClickedCreateRoomUIButton);
        ParticipateRoomButton.onClick.RemoveListener(OnClickedJoinRoomUIButton);

        backToGameStart.onClick.RemoveListener(BackToGameStartUI);

        startSceneButton.onClick.RemoveListener(OnStartGame);
        Button_JoinRoom.onClick.RemoveListener(JoinRoomButtonClicked);
        BackRoomButton.onClick.RemoveListener(OnClickBackRoomButton);
    }


    private void OnClickedCreateRoomUIButton()
    {
        createRoomButton.gameObject.SetActive(false);
        ParticipateRoomButton.gameObject.SetActive(false);
        Button_JoinRoom.gameObject.SetActive(false);
        JoinMenuUI.SetActive(true);
        backToGameStart.gameObject.SetActive(true);
        startSceneButton.gameObject.SetActive(true);
        BackRoomButton.gameObject.SetActive(true);
    }

    private void BackToGameStartUI()
    {
        if (JoinMenuUI.activeSelf)
            JoinMenuUI.SetActive(false);
        if (lobbyRoomUI.activeSelf)
            lobbyRoomUI.SetActive(false);
        createRoomButton.gameObject.SetActive(true);
        ParticipateRoomButton.gameObject.SetActive(true);
        backToGameStart.gameObject.SetActive(false);
    }

    private async void OnClickedJoinRoomUIButton()
    {
        createRoomButton.gameObject.SetActive(false);
        ParticipateRoomButton.gameObject.SetActive(false);
        lobbyRoomUI.SetActive(true);
        backToGameStart.gameObject.SetActive(true);
        startSceneButton.gameObject.SetActive(false);
        Button_JoinRoom.gameObject.SetActive(true);
        BackRoomButton.gameObject.SetActive(true);
    }

    private void OnClickBackRoomButton()
    {
        BackRoomButton.gameObject.SetActive(false);
        JoinMenuUI.SetActive(false);
        lobbyRoomUI.SetActive(false);
        createRoomButton.gameObject.SetActive(true);
        ParticipateRoomButton.gameObject.SetActive(true);
    }


    private void OnStartGame()
    {
        // TODO
        Debug.Log("게임시작");

    }

    private async void JoinRoomButtonClicked()
    {
        //TODO
        Debug.Log("방참가");
    }


}
