using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TrialClearRecord : MonoBehaviour
{
    public Text labelText;
    public Text timerText;

    public Image labelImage;
    public Image timerImage;

    public Color normalImageColor;
    public Color playerImageColor;

    public Color normalTextColor;
    public Color playerTextColor;

    public void SetState(bool isPlayerRecord)
    {
        labelImage.color = isPlayerRecord ? playerImageColor : normalImageColor;
        timerImage.color = isPlayerRecord ? playerImageColor : normalImageColor;

        labelText.color = isPlayerRecord ? playerTextColor : normalTextColor;
        timerText.color = isPlayerRecord ? playerTextColor : normalTextColor;
    }
}
