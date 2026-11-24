using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

[RequireComponent (typeof(Animator))]

public class BGMNotification : MonoBehaviour
{
    public Text songNameText;

    private Animator _animator;

    void Awake()
    {
        _animator = GetComponent<Animator>();
    }

    public void UpdateSongName(string songName)
    {
        if (
            SceneManager.GetActiveScene().name.Contains("Store") || 
            SceneManager.GetActiveScene().name.Contains("Gameplay") || 
            SceneManager.GetActiveScene().name.Contains("Endless")
        )
        {
            songNameText.text = songName;
            _animator.SetTrigger("Active"); 
        } 
    }
}
