using System;
using UnityEngine;

public class PlayerMovement : MonoBehaviour
{

    public float moveSpeed = 5f; // Speed of the player's movement
    public int health = 50;
    public Transform enemy;
    public float stopDis = 1f;
    private int counter = 0;

    void Update()
    {
        // Get input from the horizontal and vertical axes (arrow keys or WASD)
        float moveX = Input.GetAxis("Horizontal");
        float moveZ = Input.GetAxis("Vertical");

        // Create a movement vector
        Vector3 movement = new Vector3(moveX, 0f, moveZ);

        // Move the player
        transform.Translate(movement * moveSpeed * Time.deltaTime, Space.World);
        //Debug.Log("health =" + health);

        float distance = Vector3.Distance(transform.position, enemy.position);

        if (distance <= stopDis)
        {
            if (counter % 280 == 0)
            {
                counter++;
                //health -= 5;
            }
            else
            {
                counter++;
            }
        }
        else
        {
            counter = 0;
        }

    }
    void OnCollisionEnter(Collision collision)
    {
        // Check if the collision object is tagged as "Minion"
        if (collision.gameObject.CompareTag("minion"))
        {
            TakeDamage(5); // Lose 10 HP for each hit (you can adjust the value)
        }
    }
    void TakeDamage(int damage)
    {
        health -= damage;


        // Check if health falls below 0

    }
}
