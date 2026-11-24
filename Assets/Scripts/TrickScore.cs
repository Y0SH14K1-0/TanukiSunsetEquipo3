using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TrickScore : MonoBehaviour
{
    public Animator animator;
    public Text text;
    public Text description;

    void Start()
    {
        Destroy(gameObject, 2f);
    }
}
