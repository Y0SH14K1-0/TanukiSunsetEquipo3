using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent (typeof(Image))]

public class HUDSpriteImage : MonoBehaviour
{
    public string spriteId = "";
    private Image image;
    
    void Start()
    {
        if (string.IsNullOrEmpty(spriteId)) return;

        GameRoot.Instance.inputCheck.onInputHasChanged += UpdateBarOnInputChange;

        image = GetComponent<Image>();
        image.sprite = GameRoot.Instance.hudSpriteLibrary.GetSpriteFromId(spriteId);
    }

    public void UpdateBarOnInputChange(InputCheck.InputType newInputType)
    {
        if (image == null)
        {
            GameRoot.Instance.inputCheck.onInputHasChanged -= UpdateBarOnInputChange;
            return;
        }
        image.sprite = GameRoot.Instance.hudSpriteLibrary.GetSpriteFromId(spriteId);
    }
}
