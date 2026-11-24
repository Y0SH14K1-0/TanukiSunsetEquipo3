using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MoveObject : MonoBehaviour
{
    Vector3 startPoint;
    public Transform endPoint;
    public float speed = 1f;
    // Start is called before the first frame update
    void Start()
    {
        startPoint = transform.position;
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        if (Vector3.Distance(TanukiTestRoot.Instance.longBoardControls.transform.position, transform.position) < 35f)
        {
            if (Vector3.Distance(transform.position, endPoint.position) > 4f)
                transform.Translate((endPoint.position - startPoint).normalized * speed * Time.deltaTime, Space.World);
        }
    }
}
