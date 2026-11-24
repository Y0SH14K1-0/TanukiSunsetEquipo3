using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;

public class WooshDetector : MonoBehaviour
{
    public AudioMixerGroup mixerGroup;
    public Rigidbody boardRb;
    public GameObject reflectionNoisePrefab;
    public GameObject reflectionEnginePrefab;

    private bool onCooldown = false;

    void OnCollisionEnter(Collision collision)
    {
        GameObject prefabToInstantiate = reflectionNoisePrefab;

        string colliderName = collision.collider.name.ToLower();
        if (onCooldown || colliderName == "" ||
            colliderName.Contains("startsegment") ||
            colliderName.Contains("variant") ||
            colliderName.Contains("ramp") ||
            colliderName.Contains("road"))
        {
            return;
        }

        if (colliderName.Contains("car") ||
            colliderName.Contains("bus"))
        {
            prefabToInstantiate = reflectionEnginePrefab;
        }

        float pitchVel = boardRb.linearVelocity.sqrMagnitude / 900f;
        GameObject newAudioObject = Instantiate(prefabToInstantiate);
        newAudioObject.GetComponent<AudioSource>().pitch = Mathf.Clamp(pitchVel, 0.8f, 1.2f) + Random.Range(-0.1f, 0.1f);
        newAudioObject.transform.position = collision.GetContact(0).point;

        onCooldown = true;
        Invoke("CooldownOver", 0.2f);
    }

    void CooldownOver()
    {
        onCooldown = false;
    }
}
