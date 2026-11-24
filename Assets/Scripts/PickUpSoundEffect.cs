using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PickUpSoundEffect : MonoBehaviour
{
    public List<AudioClip> audioClips;

    private AudioSource audioSource;

    void Start()
    {
        audioSource = GetComponent<AudioSource>();
        audioSource.clip = audioClips[Random.Range(0, audioClips.Count)];
        audioSource.pitch = Random.Range(0.7f, 1.3f);
        audioSource.Play();
        Invoke("Destroy", 1f);
    }

    void Destroy()
    {
        Destroy(gameObject);
    }
}
