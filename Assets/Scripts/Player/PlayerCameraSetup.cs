using Cinemachine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerCameraSetup : MonoBehaviour
{
    private CinemachineVirtualCamera virtualCamera;

    private void Start()
    {
        virtualCamera = GetComponent<CinemachineVirtualCamera>();
        StartCoroutine(SetupCamera());
    }

    private IEnumerator SetupCamera()
    {
        while (GameManager.Instance?.LocalPlayer == null)
        {
            yield return new WaitForSeconds(0.1f);
        }

        NetworkPlayerAnimator localPlayer = GameManager.Instance.LocalPlayer.GetComponent<NetworkPlayerAnimator>();
        if (localPlayer != null)
        {
            virtualCamera.Follow = localPlayer.transform;
           // virtualCamera.LookAt = localPlayer.transform;
        }
    }
}
