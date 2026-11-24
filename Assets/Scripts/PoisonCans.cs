using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PoisonCans : MonoBehaviour
{
    // Start is called before the first frame update
    public GameObject splashEffect;

    private void OnTriggerEnter(Collider other)
    {
        if (other.tag == "Player")
        {
            if (splashEffect != null)
                Instantiate(splashEffect, transform.position, Quaternion.identity);

            GameManager.Instance.statusEffectSystem.AddTemporaryStatusEffect(StatusEffectSystem.StatusEffect.EffectType.POISON, 6f);
            this.enabled = false;
        }
    }
}
