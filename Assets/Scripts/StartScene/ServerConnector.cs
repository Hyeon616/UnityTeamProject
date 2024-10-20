using System;
using System.Collections;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

public class ServerConnector : MonoBehaviour
{
    private TcpClient _tcpClient;
    private NetworkStream _stream;
    private readonly string SERVER_IP = "127.0.0.1";
    private readonly int SERVER_PORT = 7777;

    async void Start()
    {
        await ConnectToServer();
    }

    private void OnDestroy()
    {
        _stream?.Close();
        _tcpClient?.Close();
    }

    private async Task ConnectToServer()
    {
        try
        {
            _tcpClient = new TcpClient();
            await _tcpClient.ConnectAsync(SERVER_IP, SERVER_PORT);
            _stream = _tcpClient.GetStream();
            Debug.Log("서버에 연결되었습니다.");
        }
        catch (Exception ex)
        {
            Debug.Log($"서버 연결 실패 : {ex.Message}");
        }
    }

    public async Task<string> SendMessage(string message)
    {
        if(_stream == null)
        {
            Debug.Log("서버에 연결되어 있지 않습니다.");
            return null;
        }

        try
        {
            byte[] data = Encoding.UTF8.GetBytes(message);
            await _stream.WriteAsync(data, 0, data.Length);

            byte[] response = new byte[1024];
            int readData = await _stream.ReadAsync(response, 0, response.Length);
            string encodingResponse = Encoding.UTF8.GetString(response, 0 ,readData);

            return encodingResponse;

        }
        catch (Exception ex)
        { 
            Debug.Log($"서버 전송 실패 : {ex.Message}");
            return null;
        }
    }

}
