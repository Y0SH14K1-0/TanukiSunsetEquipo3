using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class RewindSceneManager : MonoBehaviour
{
    private static RewindSceneManager _instance;
    public static RewindSceneManager Instance { get { return _instance; } }

    private void Awake()
    {
        if (_instance != null && _instance != this)
        {
            Destroy(this.gameObject);
        }
        else
        {
            _instance = this;
            SceneManager.sceneLoaded += OnSceneLoaded;
        }

        //DontDestroyOnLoad(this.gameObject);
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {

    }

    public static void LoadScene(string sceneName)
    {
        SceneManager.LoadScene(sceneName);
    }

    public static void LoadSceneAsync(string sceneName)
    {
        SceneManager.LoadSceneAsync(sceneName);
    }
}
