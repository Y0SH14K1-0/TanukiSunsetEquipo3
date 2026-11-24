using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ScoreIncrease : MonoBehaviour
{
    public Text text;
    public float points;
    
    public void BumpScore()
    {
        GameManager.Instance.scoreSystem.BumpScore(points);
    }

    void Start()
    {
        Destroy(gameObject, 2f);
    }
}
