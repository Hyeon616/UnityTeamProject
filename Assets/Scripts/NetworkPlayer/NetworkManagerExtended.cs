using Mirror;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NetworkManagerExtended : NetworkManager
{
    
    public void StartGame(string sceneName)
    {
        if (NetworkServer.active)
        {
            Debug.Log($"Loading scene {sceneName}");
            ServerChangeScene(sceneName);
        }
        else
        {
            Debug.LogError("Not the server. Only the server can start the game.");
        }
    }


    public override void OnServerChangeScene(string newSceneName)
    {
        base.OnServerChangeScene(newSceneName);

        // 서버에서 씬 변경 후 추가 작업
        Debug.Log($"Server changed scene to {newSceneName}");
    }

    public override void OnClientChangeScene(string newSceneName, SceneOperation sceneOperation, bool customHandling)
    {
        base.OnClientChangeScene(newSceneName, sceneOperation, customHandling);
        // 클라이언트에서 씬 변경 후 추가 작업 (필요시)
        Debug.Log($"Client changing scene to {newSceneName}");
    }

    public override void OnClientSceneChanged()
    {
        base.OnClientSceneChanged();
        // 클라이언트의 씬 변경 후 추가 작업 (필요시)
        Debug.Log("Client scene changed");

        // 클라이언트가 씬 변경 후 서버에 플레이어 추가 요청을 보냄
        if (NetworkClient.connection.identity == null)
        {
            NetworkClient.AddPlayer();
        }

    }
    public override void OnServerAddPlayer(NetworkConnectionToClient conn)
    {
        // 플레이어 프리팹을 스폰하고 연결된 클라이언트에 추가
        GameObject player = Instantiate(playerPrefab);
        NetworkServer.AddPlayerForConnection(conn, player);
        Debug.Log($"Player added for connection: {conn.connectionId}");
    }


}



