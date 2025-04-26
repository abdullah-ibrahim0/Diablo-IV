using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyController1 : MonoBehaviour
{

    public int hp;

    public string type;

    // Start is called before the first frame update
    void Start()
    {
        type = "Minion";
        if (type == "Minion")
        {
            hp = 20;
        } 
        if (type == "Demon")
        {
            hp = 40;
        }
        if (type == "Lilith")
        {
            hp = 50;
        }

    }

    // Update is called once per frame
    void Update()
    {
        if (hp <= 0)
        {
            PlayerController player = GameObject.FindWithTag("Wanderer").GetComponent<PlayerController>();
            if (player != null)
            {
                player.xp += 10;
            }
            Destroy(gameObject);
        }
    }

    public void Damage(int damage)
    {
        hp-= damage;
        Debug.Log("HP for min" + hp);
    }

    public void Stunned()
    {
        // Implement in enemy
        Debug.Log($"{gameObject.name} is stunned!");
    }

    public void SlowDown()
    {
        // Implement in enemy
        Debug.Log($"{gameObject.name} is slowed down!");
    }
}
