using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[CreateAssetMenu(fileName = "NewHUDSpriteLibrary", menuName = "TanukiSunset/NewHUDSpriteLibrary", order = 1)]
public class HUDSpriteLibrary : ScriptableObject
{
    [Reorderable] public List<HUDSprite> hudSprites;

    public Sprite GetSpriteFromId(string spriteId)
    {
        Sprite resultSprite;

        switch (GameRoot.Instance.inputCheck.GetInputType())
        {
            case (InputCheck.InputType.MouseKeyboard):
                resultSprite = hudSprites.FirstOrDefault(x => x.spriteId == spriteId).mouseKeyboardSprite;
            break;
            case (InputCheck.InputType.Controller):
                //TODO: Change 0 value to match different controller types.
                resultSprite = GetControllerSprite(0, spriteId);
            break;
            default:
                resultSprite = hudSprites.FirstOrDefault(x => x.spriteId == spriteId).mouseKeyboardSprite;
            break;
        }
        return resultSprite;
    }

    private Sprite GetControllerSprite(int controllerType, string spriteId)
    {
        Sprite result;

        if (controllerType == 0)
            result = hudSprites.FirstOrDefault(x => x.spriteId == spriteId).xboxSprite;
        else if (controllerType == 1)
            result = hudSprites.FirstOrDefault(x => x.spriteId == spriteId).playstationSprite;
        else if (controllerType == 2)
            result = hudSprites.FirstOrDefault(x => x.spriteId == spriteId).switchSprite;
        else // If invalid value, make default xbox style
            result = hudSprites.FirstOrDefault(x => x.spriteId == spriteId).xboxSprite;

        return result;
    }
}

[System.Serializable]
public class HUDSprite
{
    public string spriteId;
    public Sprite xboxSprite;
    public Sprite playstationSprite;
    public Sprite switchSprite;
    public Sprite mouseKeyboardSprite;
}
