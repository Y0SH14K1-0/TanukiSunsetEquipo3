using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class HUDBarButton : MonoBehaviour
{
    public Text buttonLabelText;
    public Image buttonImage;

    private HUDBarConfiguration.HUDBarButtonObject currentHUDBarButtonObject;

    public void UpdateButtonWithObject(HUDBarConfiguration.HUDBarButtonObject HUDBarButtonObject)
    {
        currentHUDBarButtonObject = HUDBarButtonObject;
        RefreshButton();
    }

    public void RefreshButton()
    {
        buttonLabelText.text = currentHUDBarButtonObject.buttonLabel;
        buttonImage.sprite = GameRoot.Instance.hudSpriteLibrary.GetSpriteFromId(currentHUDBarButtonObject.spriteId);
    }
}
