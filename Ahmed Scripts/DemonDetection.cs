using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class DemonDetection : MonoBehaviour
{
    private DemonBehavior[] demonsDetected;
    private DemonBehavior currentChasingDemon;
    private DemonBehavior nextDemon;

    private void Start()
    {
    }

    private void Update()
    {
        if (currentChasingDemon && currentChasingDemon.isDead && nextDemon )
        {
            nextDemon.StartChasing();
        }
    }
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("enemy"))
        {
            Debug.Log("DEMON AAAAAA");
            DemonBehavior demon = other.GetComponent<DemonBehavior>();
            
            if (currentChasingDemon == null && demon != null && !demon.IsChasing)
            {
                currentChasingDemon = demon;
                demon.StartChasing();
            }
            else
            {
                Debug.Log("DEMON 2");
                nextDemon = demon;
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("enemy"))
        {
            DemonBehavior demon = other.GetComponent<DemonBehavior>();
            if (currentChasingDemon == demon)
            {
                demon.StopChasing();
                currentChasingDemon = null;
            }
        }
    }
}
