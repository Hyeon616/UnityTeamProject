using UnityEngine;
using Unity.Netcode;
using UnityEngine.UI;
using TMPro;

public class NetworkStatusHandler : MonoBehaviour
{
    [SerializeField]
    private TextMeshProUGUI statusText; // ���� ���¸� ǥ���� Text UI

    private void Start()
    {
        // ��Ʈ��ũ �̺�Ʈ �ݹ� ���
        NetworkManager.Singleton.OnClientConnectedCallback += OnClientConnected;
        NetworkManager.Singleton.OnClientDisconnectCallback += OnClientDisconnected;
    }

    private void OnDestroy()
    {
        // ��Ʈ��ũ �̺�Ʈ �ݹ� ����
        if (NetworkManager.Singleton != null)
        {
            NetworkManager.Singleton.OnClientConnectedCallback -= OnClientConnected;
            NetworkManager.Singleton.OnClientDisconnectCallback -= OnClientDisconnected;
        }
    }

    private void OnClientConnected(ulong clientId)
    {
        if (clientId == NetworkManager.Singleton.LocalClientId)
        {
            // Ŭ���̾�Ʈ�� ������ �����
            Debug.Log("Connected to server");
            UpdateStatusText("Connected to server");
        }
    }

    private void OnClientDisconnected(ulong clientId)
    {
        if (clientId == NetworkManager.Singleton.LocalClientId)
        {
            // Ŭ���̾�Ʈ�� �������� ���� ������
            Debug.Log("Disconnected from server");
            UpdateStatusText("Disconnected from server");
        }
    }

    private void UpdateStatusText(string message)
    {
        if (statusText != null)
        {
            statusText.text = message;
        }
    }
}