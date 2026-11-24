using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class SnapToGround : MonoBehaviour
{
    [MenuItem("Custom/Snap To Ground %g")]
    public static void Ground()
    {
        foreach (var transform in Selection.transforms)
        {
            var hits = Physics.RaycastAll(transform.position + (Vector3.up*10f), Vector3.down, 30f);
            foreach (var hit in hits)
            {
                if (hit.collider.gameObject == transform.gameObject)
                    continue;

                transform.position = hit.point;
                break;
            }
        }
    }

    [MenuItem("Custom/Snap To Ground Normal #%g")]
    public static void GroundNormal()
    {
        foreach (var transform in Selection.transforms)
        {
            var hits = Physics.RaycastAll(transform.position + (Vector3.up * 10f), Vector3.down, 30f);
            foreach (var hit in hits)
            {
                if (hit.collider.gameObject == transform.gameObject)
                    continue;

                transform.position = hit.point;
                transform.up = hit.normal;
                break;
            }
        }
    }

}