using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent (typeof(AudioSource))]

public class RandomAudioSource : MonoBehaviour
{
    public List<AudioClip> audioClips;
    public float minPitch = 1f;
    public float maxPitch = 1f;
    public float minVolume = 1f;
    public float maxVolume = 1f;

    private AudioSource _audioSource;

    void Awake()
    {
        _audioSource = GetComponent<AudioSource>();
    }

    public void PlayRandom()
    {
        if (audioClips == null || audioClips.Count <= 0) return;

        AudioClip audioClipToPlay = audioClips[Random.Range(0, audioClips.Count)];
        float pitch = Random.Range(minPitch, maxPitch);
        float volume = Random.Range(minVolume, maxVolume);

        _audioSource.pitch = pitch;
        _audioSource.volume = volume;
        _audioSource.clip = audioClipToPlay;

        _audioSource.Play();
    }
}
