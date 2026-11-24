using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LookAtCamera : MonoBehaviour
{
    public float targetDistance = 20f;
    public float lateralDistance = 5f;
    public Vector3 camOffset;

    public void SetNextCamera()
    {
        Vector3 playerPos = TanukiTestRoot.Instance.longBoardControls.transform.position;
        Vector3 normalizedPlayerVel = TanukiTestRoot.Instance.longBoardControls.rb.linearVelocity.normalized;
        Vector3 targetPos = playerPos + (normalizedPlayerVel * targetDistance);
        float randomX = Random.Range(0, 2) == 0 ? -lateralDistance : lateralDistance;
        targetPos += Vector3.Cross(normalizedPlayerVel, Vector3.up) * randomX;
        targetPos += camOffset;
        transform.position = targetPos;
    }

    private void Update()
    {
        if (Vector3.Distance(transform.position, TanukiTestRoot.Instance.longBoardControls.transform.position) > (targetDistance+lateralDistance))
        {
            SetNextCamera();
        }
    }
}
