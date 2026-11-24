using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Woosh : MonoBehaviour
{
    public AudioSource audioSource;

    private void Start()
    {
        StartCoroutine(C_Destroy());
    }

    IEnumerator C_Destroy()
    {
        yield return new WaitForSecondsRealtime(1f);
        for (float f = 1f; f >= 0f; f -= Time.unscaledDeltaTime)
        {
            audioSource.volume = f;
            yield return null;
        }
        audioSource.volume = 0f;
        Destroy(gameObject);
    }
}
