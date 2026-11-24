using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class SelectableHelper : MonoBehaviour
{
    public GameObject targetGameObject;

    public void SelectTargetGameObject()
    {
        EventSystem.current.SetSelectedGameObject(null);
        EventSystem.current.SetSelectedGameObject(targetGameObject);
    }
}
