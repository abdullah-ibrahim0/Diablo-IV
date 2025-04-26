using UnityEngine;
using UnityEngine.UI;  // For working with UI elements

public class MinionController : MonoBehaviour
{
    public int maxHealth = 20;      // Minion's maximum health
    public int currentHealth;       // Minion's current health
    public Slider healthBar;        // The health bar UI element
    public Animator animator;       // Reference to the Animator for death animation

    void Start()
    {
        currentHealth = maxHealth;  // Initialize health to max
        healthBar.maxValue = maxHealth;  // Set the max health on the slider
        healthBar.value = currentHealth; // Set the initial health on the slider
    }

    void Update()
    {
        // Check for key press (M) to damage the minion
        //if (Input.GetKeyDown(KeyCode.M))
        //{
            //TakeDamage(5);  // Deal 5 damage when M key is pressed
        //}
    }

    void LateUpdate()
    {
        //if (healthBar != null)
        //{
        //    healthBar.transform.position = transform.position + new Vector3(0, 2, 0); // Offset above the minion
        //}
    }


    // Method to handle damage taken by the minion
    public void TakeDamage(int damage)
    {
        currentHealth -= damage;  // Reduce current health
        currentHealth = Mathf.Clamp(currentHealth, 0, maxHealth); // Ensure health doesn't go below 0

        healthBar.value = currentHealth;  // Update the health bar slider

        // Check if the minion's health is 0 (death condition)
        if (currentHealth == 0)
        {
            Die();
        }
    }

    // Method to handle the death of the minion
    void Die()
    {
        if (animator != null)
        {
            animator.SetTrigger("Die");  // Play the death animation (trigger should be defined in Animator)
        }

        BossController boss = FindObjectOfType<BossController>(); // Reference the boss
        if (boss != null)
        {
            boss.MinionDefeated(gameObject);
        }

        Destroy(gameObject);  // Destroy the minion object after a short delay (animation time)
    }

}