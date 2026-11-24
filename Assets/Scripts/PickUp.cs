using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PickUp : MonoBehaviour
{
    public bool changeSong = false;
    public int pointAmount;
    public string description;
    public bool collectable = false;

    public GameObject PickUpEffect;

    private void OnTriggerEnter(Collider other)
    {
        if(other.gameObject.tag == "Player")
        {
            if (changeSong) AudioService.Instance.PlayNextSong();
            Destroy(gameObject);
            /*
            if (PickUpEffect != null)
                Instantiate(PickUpEffect, transform.position, transform.rotation);

            if(pointAmount > 0)
            {
                GameManager.Instance.scoreSystem.InstantTrick(pointAmount, description);
                if (description.ToLower() == "tape")
                {
                    GameManager.Instance.missionManagerHUD.UpdateMissionProgress(MissionType.TapeCount, 1);
                }
            }
            if (collectable)
            {
                GameManager.Instance.collectablesHUD.CollectPoints();
            }
            */
        }
    }
}
