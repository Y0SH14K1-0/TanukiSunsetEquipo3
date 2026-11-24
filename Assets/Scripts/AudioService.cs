using System.Collections;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.SceneManagement;

public class AudioService : MonoBehaviour
{
    private static AudioService _instance;
    public static AudioService Instance => _instance;

    private void Awake ()
    {
        if (_instance != null && _instance != this)
        {
            Destroy (this.gameObject);
        }
        else
        {
            _instance = this;
            SceneManager.sceneLoaded += OnSceneLoaded;
        }
    }

    //public MusicLibrary musicLibrary;
    public AudioClip tapeChangeClip;
    public AudioClip tapeChangeClipIntro;
    public AudioSource deathSfxAudioSource;
    public AudioMixer audioMixer;
    public AudioSourceLoudnessUtility audioSourceLoudnessUtility;

    public delegate void OnSongChangeDelegate (Song newSong);
    public OnSongChangeDelegate onSongChange;

    private AudioSource audioSource
    {
        // ReSharper disable once Unity.PerformanceCriticalCodeInvocation
        get => (_audioSource) ? _audioSource : _audioSource = GetComponent<AudioSource> ();
        set => _audioSource = value;
    }

    private AudioSource _audioSource;
    //private Song _currentSong;
    private Coroutine _nextSongCoroutine;
    private Coroutine _firstSongCoroutine;
    private bool _isPausedOrDead;
    private bool _hasExited = true;
    private bool _loadedNextEndlessScene;
    private Scene _lastSceneLoaded;
    private bool _isMute;
    private bool _isDipped;

    public void MuteMusic(bool value)
    {
        audioSource.mute = value;
        _isMute = value;
    }

    private void Update ()
    {
        // ReSharper disable once Unity.PerformanceCriticalCodeInvocation
        if (_nextSongCoroutine != null || _firstSongCoroutine != null || _isMute || _isDipped) return;
        
        // if (!audioSource.isPlaying && !_isPausedOrDead)
        // {
        //     PlayNextSong ();
        //     return;
        // }
        //
        // if (audioSource.time > audioSource.clip.length - 0.3f)
        // {
        //     PlayNextSong ();
        // }
    }

    public void LoadingNextEndlessScene()
    {
        _loadedNextEndlessScene = true;
    }

    private void OnSceneLoaded (Scene scene, LoadSceneMode mode)
    {
        if (_hasExited == false || _loadedNextEndlessScene)
        {
            _loadedNextEndlessScene = false;
            //SetGroovinParameters (true, _currentSong.bpm);
            return;
        }

        _lastSceneLoaded = scene;

        if (_lastSceneLoaded.name == "Start")
        {
            audioSource.Stop();
            return;
        }

        audioSource.Stop ();
        StartCoroutine (C_LowPassLerp (22000f, 0.1f));

        Song sceneSong;
        
        //sceneSong = musicLibrary.GetNextRandomSong ();

        // if (TanukiTestRoot.Instance.GameSession.sceneSongsHeardList.Contains (_lastSceneLoaded.name))
        // {
        //     sceneSong = musicLibrary.GetNextRandomSong ();
        // }
        // else
        // {
        //     sceneSong = musicLibrary.GetSongFromScene (_lastSceneLoaded.name);
        //     if (sceneSong is { forcePlay: false }) TanukiTestRoot.Instance.GameSession.sceneSongsHeardList.Add (_lastSceneLoaded.name);
        // }

        //if (sceneSong == null) return;
        
        //_currentSong = sceneSong;
        audioSource.Stop ();
        if (_lastSceneLoaded.name == "Store")
        {
            audioSource = GameObject.Find ("AS_RadioAudioSource").GetComponent<AudioSource> ();
        }
        //audioSource.clip = _currentSong.audioClip;
        audioSource.volume = 1f;
        
        if (!audioSource.isActiveAndEnabled) return;
        //onSongChange?.Invoke (_currentSong);
        audioSource.Play ();
        //SetGroovinParameters (true, _currentSong.bpm);
    }

    /*public void PlaySceneSong ()
    {
        if (firstSongCoroutine != null || nextSongCoroutine != null) return;
        firstSongCoroutine = StartCoroutine (C_PlaySceneSong ());
    }*/

    public void PlayNextSong ()
    {
        if (_firstSongCoroutine != null || _nextSongCoroutine != null) return;
        _nextSongCoroutine = StartCoroutine (C_PlayNextSong ());
    }

    public void PlayNextSongFast ()
    {
        if (_firstSongCoroutine != null || _nextSongCoroutine != null) return;
        _nextSongCoroutine = StartCoroutine (C_PlayNextSongFast ());
    }

    public void PlaySongWithId (string songId, bool tapeEffect = true)
    {
        if (_firstSongCoroutine != null || _nextSongCoroutine != null) return;
        _nextSongCoroutine = StartCoroutine (C_PlaySongWithId (songId, tapeEffect));
    }

    public void PlayerDied ()
    {
        _isPausedOrDead = true;
        deathSfxAudioSource.Play ();
        StartCoroutine (C_LowPassLerp (300f));
        Time.timeScale = 0.5f;
        Time.fixedDeltaTime = Time.timeScale / 50f;
    }

    public void RetryButtonPressed (bool resetSong = false)
    {
        if (!resetSong) _hasExited = false;
        _isPausedOrDead = false;
        StartCoroutine (C_LowPassLerp (22000f));
    }

    public void Pause (bool pause)
    {
        _isPausedOrDead = pause;

        if (pause)
            audioSource.Pause ();
        else
            audioSource.UnPause ();
    }

    public void FadeAndStop(float duration)
    {
        StartCoroutine(C_FadeAndStop(duration));
    }

    private IEnumerator C_LowPassLerp (float target, float duration = 0.5f)
    {
        audioMixer.GetFloat ("LowPassCutoff", out var currentValue);

        if (currentValue <= 0f) yield break;

        for (var f = 0f; f <= 1f; f += Time.unscaledDeltaTime / duration)
        {
            audioMixer.SetFloat ("LowPassCutoff", Mathf.Lerp (currentValue, target, Mathf.SmoothStep (0f, 1f, f)));
            yield return null;
        }

        audioMixer.SetFloat ("LowPassCutoff", target);
    }

    private IEnumerator C_PlayNextSong ()
    {
        audioSource.Stop ();
        //_currentSong = musicLibrary.GetNextRandomSong ();
        audioSource.clip = tapeChangeClip;
        audioSource.volume = 1f;
        audioSource.Play ();
        SetGroovinParameters (false);

        yield return new WaitForSecondsRealtime (tapeChangeClip.length);

        //onSongChange?.Invoke (_currentSong);
        //audioSource.clip = _currentSong.audioClip;
        audioSource.Play ();
        //SetGroovinParameters (true, _currentSong.bpm);

        _nextSongCoroutine = null;
    }

    private IEnumerator C_PlayNextSongFast ()
    {
        audioSource.Stop ();
        //_currentSong = musicLibrary.GetNextRandomSong ();
        audioSource.clip = tapeChangeClipIntro;
        audioSource.volume = 1f;
        audioSource.Play ();
        SetGroovinParameters (false);

        yield return new WaitForSecondsRealtime (tapeChangeClipIntro.length);

        //onSongChange?.Invoke (_currentSong);
        //audioSource.clip = _currentSong.audioClip;
        audioSource.Play ();
        //SetGroovinParameters (true, _currentSong.bpm);

        _nextSongCoroutine = null;
    }

    private IEnumerator C_FadeAndStop(float duration)
    {
        float initialVolume = audioSource.volume;
        float endVolume = 0f;

        for (float f = 0f; f < 1f; f += (Time.unscaledDeltaTime/duration))
        {
            audioSource.volume = Mathf.Lerp(initialVolume, endVolume, f);
            yield return null;
        }
        audioSource.Stop();
        audioSource.volume = initialVolume;
    }

    private IEnumerator C_PlaySongWithId (string songId, bool tapeEffect = true)
    {
        audioSource.Stop ();

        //_currentSong = musicLibrary.GetSongWithId (songId);

        if (tapeEffect)
        {
            audioSource.clip = tapeChangeClip;
            audioSource.volume = 1f;
            audioSource.Play ();
            SetGroovinParameters (false);

            yield return new WaitForSecondsRealtime (tapeChangeClip.length);
        }

        //onSongChange?.Invoke (_currentSong);
        //audioSource.clip = _currentSong.audioClip;
        audioSource.Play ();
        //SetGroovinParameters (true, _currentSong.bpm);

        _nextSongCoroutine = null;
    }

    public void OnGameplaySceneExit ()
    {
        _hasExited = true;
        audioSource.Stop ();
        StartCoroutine (C_LowPassLerp (22000f));
    }

    // public Song GetCurrentSong ()
    // {
    //     return _currentSong;
    // }

    private static void SetGroovinParameters (bool isGrooving, int bpm = 60)
    {
        if (isGrooving)
        {
            //Debug.Log ("Starting to groove at: " + bpm + "BPM.");
            if (TanukiTestRoot.Instance.sunAnimatorBPM != null) TanukiTestRoot.Instance.sunAnimatorBPM.StartGroovin (bpm);
            TanukiTestRoot.Instance.RaccoonAnimations.StartGroovin (bpm);
        }
        else
        {
            //Debug.Log ("Stopping to groove.");
            if (TanukiTestRoot.Instance.sunAnimatorBPM != null) TanukiTestRoot.Instance.sunAnimatorBPM.StopGroovin ();
            TanukiTestRoot.Instance.RaccoonAnimations.StopGroovin ();
        }
    }

    public void DipMusicVolume (float targetVolume, float duration, bool isDipped)
    {
        _isDipped = isDipped;
        StartCoroutine (C_DipMusic (targetVolume, duration, 0.5f, 0.5f));
    }

    public void DipMusicVolume (float targetVolume, bool isDipped)
    {
        _isDipped = isDipped;
        StartCoroutine (C_DipMusic (targetVolume, 0.5f));
    }

    private IEnumerator C_DipMusic (float targetVolume, float dipDuration, float startFadeDuration, float endFadeDuration)
    {
        for (var f = 0f; f < 1f; f += (Time.unscaledDeltaTime / startFadeDuration))
        {
            audioSource.volume = Mathf.Lerp (1f, targetVolume, f);
            yield return null;
        }
        audioSource.volume = targetVolume;

        yield return new WaitForSecondsRealtime (dipDuration);

        for (var f = 0f; f < 1f; f += (Time.unscaledDeltaTime / endFadeDuration))
        {
            audioSource.volume = Mathf.Lerp (targetVolume, 1f, f);
            yield return null;
        }
        audioSource.volume = 1f;
    }

    private IEnumerator C_DipMusic (float targetVolume, float startFadeDuration = 0.4f)
    {
        for (var f = 0f; f < 1f; f += (Time.unscaledDeltaTime / startFadeDuration))
        {
            audioSource.volume = Mathf.Lerp (1f, targetVolume, f);
            yield return null;
        }
        audioSource.volume = targetVolume;
    }
}