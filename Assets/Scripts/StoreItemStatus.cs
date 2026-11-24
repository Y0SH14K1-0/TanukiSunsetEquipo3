using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]

public class StoreItemStatus : MonoBehaviour
{
    public GameObject availableStatusGameObject;
    public GameObject ownedStatusGameObject;
    public GameObject equippedStatusGameObject;

    public StoreItemStatusState currentStatus = StoreItemStatusState.AVAILABLE;

    private StoreItemStatusState lastStatus = StoreItemStatusState.AVAILABLE;

    public void UpdateStatusObject(StoreItemStatusState toState)
    {
        currentStatus = toState;

        availableStatusGameObject.SetActive(false);
        ownedStatusGameObject.SetActive(false);
        equippedStatusGameObject.SetActive(false);

        if (currentStatus == StoreItemStatusState.AVAILABLE) availableStatusGameObject.SetActive(true);
        if (currentStatus == StoreItemStatusState.OWNED) ownedStatusGameObject.SetActive(true);
        if (currentStatus == StoreItemStatusState.EQUIPPED) equippedStatusGameObject.SetActive(true);
    }

    private void Update()
    {
        if (lastStatus == currentStatus) return;

        UpdateStatusObject(currentStatus);
        lastStatus = currentStatus;
    }
}

public enum StoreItemStatusState
{
    AVAILABLE,
    OWNED,
    EQUIPPED,
}
