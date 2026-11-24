using Cinemachine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CinemachineCamera : MonoBehaviour
{
    public Camera cinemachineCamera;
    public CinemachineVirtualCamera checkpointCamera;

    public void TriggerCheckpointCamera()
    {
        cinemachineCamera.enabled = true;
        checkpointCamera.gameObject.SetActive(true);
    }

    public void EndCheckpointCamera()
    {
        checkpointCamera.gameObject.SetActive(false);
        StartCoroutine(C_DisableCamera());
    }

    private IEnumerator C_DisableCamera()
    {
        yield return new WaitForSecondsRealtime(1f);
        cinemachineCamera.enabled = false;
    }
}
