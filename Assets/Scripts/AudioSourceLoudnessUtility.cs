using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(AudioSource))]

public class AudioSourceLoudnessUtility : MonoBehaviour
{
    public float updateStep = 0.2f;
    public int sampleDataLength = 1024;
    public delegate void OnUpdateSongIntensityDelegate(float intensity);
    public OnUpdateSongIntensityDelegate OnUpdateSongIntensity;

    private AudioSource _audioSource;
    private float _currentTimer = 0f;
    private float _clipLoudness;
    private float[] _clipSampleData;

    void Start()
    {
        _audioSource = GetComponent<AudioSource>();
        _clipSampleData = new float[sampleDataLength];
    }

    void FixedUpdate()
    {
        if (_audioSource == null || !_audioSource.isPlaying) return;

        _currentTimer += Time.fixedDeltaTime;
        if (_currentTimer >= updateStep)
        {
            _currentTimer = 0f;
            try
            {
                if (!_audioSource.clip.GetData(_clipSampleData, _audioSource.timeSamples)) return;
                _clipLoudness = 0f;
                foreach (var sample in _clipSampleData)
                {
                    _clipLoudness += Mathf.Abs(sample);
                }
                _clipLoudness /= sampleDataLength;
                if (OnUpdateSongIntensity != null) OnUpdateSongIntensity(_clipLoudness);
            }
            catch (System.Exception e) { Debug.LogWarning(e.Message); }
        }
    }
}
