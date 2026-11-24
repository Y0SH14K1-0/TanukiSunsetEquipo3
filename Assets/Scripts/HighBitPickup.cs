using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HighBitPickup : MonoBehaviour
{
    public GameObject PickUpEffect;
    public Vector3 EffectPosOffset;
    public bool collectable = false;
    public bool attachToTanuki = true;
    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.tag == "Player")
        {

            if (collectable)
            {
                if (GameManager.Instance.longBoardCTRL.tanukiIsFlipping || !GameManager.Instance.longBoardCTRL.grounded)
                {
                    GameManager.Instance.collectablesHUD.CollectPoints(2);
                    GameManager.Instance.longBoardCTRL.GentleSpeedup(0.6f);
                    //GameManager.Instance.raccoonAnimations.PlayGainBitsEffect();
                    Destroy(gameObject);
                    if (PickUpEffect != null)
                    {
                        if (attachToTanuki)
                        {
                            GameObject g = Instantiate(PickUpEffect, GameManager.Instance.longBoardCTRL.transform.position + EffectPosOffset, transform.rotation);
                            g.transform.parent = GameManager.Instance.longBoardCTRL.transform;
                        }
                        else
                            Instantiate(PickUpEffect, transform.position + EffectPosOffset, transform.rotation);
                    }
                }
            }
        }
    }
}
