using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ArrowController : MonoBehaviour
{
    public float speed = 20f;
    public int damage = 20;

    private Rigidbody rb;

    public GameObject rogue;


    void Start()
    {
        rb = GetComponent<Rigidbody>();
        rb.velocity = transform.forward * speed; // Propel the arrow forward
        rogue = GameObject.FindWithTag("Wanderer"); // Example: Find by tag

    }

    private void OnTriggerEnter(Collider other)
    {
        // Log the tag of the object the arrow collided with
        Debug.Log($"Collided with: {other.tag}");

        // Check if the arrow collided with an enemy
        if (other.CompareTag("enemy"))
        {
            Debug.Log("Collided with enemy!");

            // Try to damage the boss
            BossController boss = other.GetComponent<BossController>();
            if (boss != null)
            {
                boss.TakeDamage(damage, rogue); // Apply damage to the boss
                Destroy(gameObject); // Destroy the arrow after hitting the boss
                return;
            }

            // If it's not a boss, try to damage a minion
            MinionMovement minion = other.GetComponent<MinionMovement>();
            DemonBehavior demon = other.GetComponent<DemonBehavior>();
            if (minion != null)
            {
                minion.TakeDamage(damage); // Apply damage to the minion
                Destroy(gameObject); // Destroy the arrow after hitting the minion
                return;
            }
            if (demon != null)
            {
                demon.TakeDamage(damage); // Apply damage to the minion
                Destroy(gameObject); // Destroy the arrow after hitting the minion
                return;
            }
        }

        // Prevent damaging the Wanderer
        if (other.CompareTag("Wanderer"))
        {
            Debug.Log("Collided with Wanderer!");
            return; // Do nothing if the Wanderer collides
        }
        else
        {
            Debug.Log("Collided with something else, destroying the arrow.");
            Destroy(gameObject); // Destroy the arrow for other collisions
        }
       

    }
}
