using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class StoreVerticalScrollbar : MonoBehaviour
{
    public int columnsPerRow = 3;
    public float scrollSpeed = 4f;

    private Scrollbar scrollbar;
    private bool _isActive = false;
    private float targetValue;

    private void Awake()
    {
        scrollbar = GetComponent<Scrollbar>();
    }

    public void Enable(bool value)
    {
        _isActive = value;
    }

    public void HasSelectedButton(int buttonIndex, int totalButtonCount)
    {
        int totalRows = (totalButtonCount / columnsPerRow)-1;
        int currentRow = (buttonIndex-1) / columnsPerRow;
        targetValue = 1f-((float)currentRow / (float)totalRows);
    }

    private void Update()
    {
        if (!_isActive) return;

        scrollbar.value = Mathf.Lerp(scrollbar.value, targetValue, scrollSpeed * Time.deltaTime);
    }
}
