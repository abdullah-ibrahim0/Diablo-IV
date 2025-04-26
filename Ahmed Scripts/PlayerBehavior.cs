using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class PlayerBehavior : MonoBehaviour
{
    private DemonBehavior[] demons;
    private DemonBehavior nearestChasingDemon;
    public int doorsUsed;
    private void Start()
    {
        demons = FindObjectsOfType<DemonBehavior>();
        nearestChasingDemon = null;

    }
    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.H))
        {
            attackDemon();
        }
    }
    public int health = 100;

    public void TakeDamage(int damage)
    {
        health -= damage;
        Debug.Log("Player Health: " + health);

        if (health <= 0)
        {
            Die();
        }
    }

    public void attackDemon()
    {
        foreach (DemonBehavior demon in demons)
        {
            if (demon.IsChasing && !demon.isDead)
            {
                demon.TakeDamage(5);
                Debug.Log("DEMON'S HEALTH:" + demon.health);

            }
        }
    }

   
        void Die()
        {
            Debug.Log("Player has died");
        }

    
}

