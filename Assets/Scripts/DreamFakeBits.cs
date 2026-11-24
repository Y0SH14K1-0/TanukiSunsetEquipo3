using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DreamFakeBits : MonoBehaviour
{
    public Rigidbody RB;
    public RotateOverTime ROT;
    public GameObject DeathPS;
    public AudioSource AC;
    void Start()
    {
        
    }

    // Update is called once per frame

    private void OnCollisionEnter(Collision collision)
    {
        AC.pitch = Random.Range(0.9f, 1.2f);
        AC.Play();
        RB.isKinematic = false;
        ROT.enabled = false;
        Invoke("DestroyBit", 0.8f);
    }

    void DestroyBit()
    {
        Instantiate(DeathPS, transform.position, transform.rotation);
        Destroy(gameObject);
    }
    
}
