using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TanukiBitsHUD : MonoBehaviour
{
    public Animator tanukiBitsHUDAnimator;
    public Text tanukiBitsNumberText;
    public Text purchaseNumberText;

    private int _tanukiBitsCollected;

    public delegate void OnTanukiBitsChangedDelegate(int bitsCollected);
    public OnTanukiBitsChangedDelegate OnTanukiBitsChanged;

    public void Awake()
    {
        _tanukiBitsCollected = SaveSystem.CurrentSaveData().tanukiBitsCollected;
        tanukiBitsNumberText.text = string.Format("{0:n0}", _tanukiBitsCollected);
    }

    public void CollectBit(int bits = 1)
    {
        if (tanukiBitsHUDAnimator.isActiveAndEnabled) tanukiBitsHUDAnimator.SetTrigger("Bump");
        _tanukiBitsCollected += bits;
        tanukiBitsNumberText.text = string.Format("{0:n0}", _tanukiBitsCollected);

        if (OnTanukiBitsChanged != null) OnTanukiBitsChanged(_tanukiBitsCollected);
    }

    public void SaveTanukiBits()
    {
        SaveSystem.CurrentSaveData().tanukiBitsCollected = _tanukiBitsCollected;
    }

    public void LoseBits(int amount)
    {
        _tanukiBitsCollected -= amount;
        if (_tanukiBitsCollected < 0) _tanukiBitsCollected = 0;
        SaveTanukiBits();
        purchaseNumberText.text = "- " + string.Format("{0:n0}", amount);
        tanukiBitsNumberText.text = string.Format("{0:n0}", _tanukiBitsCollected);
        if (tanukiBitsHUDAnimator.isActiveAndEnabled) tanukiBitsHUDAnimator.SetTrigger("Purchase");

        if (OnTanukiBitsChanged != null) OnTanukiBitsChanged(_tanukiBitsCollected);
    }

    public bool PurchaseWithBits(int amount)
    {
        if (_tanukiBitsCollected - amount < 0) return false;
        _tanukiBitsCollected -= amount;
        SaveTanukiBits();
        purchaseNumberText.text = "- "+string.Format("{0:n0}", amount);
        tanukiBitsNumberText.text = string.Format("{0:n0}", _tanukiBitsCollected);
        if (tanukiBitsHUDAnimator.isActiveAndEnabled) tanukiBitsHUDAnimator.SetTrigger("Purchase");

        if (OnTanukiBitsChanged != null) OnTanukiBitsChanged(_tanukiBitsCollected);
        return true;
    }
}
