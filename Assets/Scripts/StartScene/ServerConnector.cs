using System;
using System.Collections;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

public class ServerConnector : Singleton<ServerConnector>
{
    private TcpClient _tcpClient;
    public NetworkStream _stream;
    private readonly string SERVER_IP = "127.0.0.1";
    private readonly int SERVER_PORT = 7777;
    private object _streamLock = new object();


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
            _tcpClient.ReceiveBufferSize = 8192;
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

        lock (_streamLock)
        {
            try
            {
                byte[] data = Encoding.UTF8.GetBytes(message);
                _stream.Write(data, 0, data.Length);
                byte[] response = new byte[8192];
                int readData = _stream.Read(response, 0, response.Length);
                string encodingResponse = Encoding.UTF8.GetString(response, 0, readData);
                return encodingResponse;
            }
            catch (Exception ex)
            {
                Debug.Log($"서버 전송 실패 : {ex.Message}");
                return null;
            }
        }
    }

    public async Task<string> ReadMessage()
    {
        if (_stream == null) return null;

        try
        {
            byte[] buffer = new byte[8192];
            int bytesRead = await _stream.ReadAsync(buffer, 0, buffer.Length);
            if (bytesRead > 0)
            {
                return Encoding.UTF8.GetString(buffer, 0, bytesRead);
            }
        }
        catch (Exception ex)
        {
            Debug.LogError($"메시지 읽기 실패: {ex.Message}");
        }
        return null;
    }

}
