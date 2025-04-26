using UnityEngine;

public class EnemyController : MonoBehaviour
{
    //[SerializeField] int maxHealth = 10;
    //int currentHealth;

    public BossController boss;

    public MinionMovement minion;
    //public bool IsDefeated => currentHealth <= 0; // Property to check if the enemy is defeated

    void Start()
    {
        //currentHealth = maxHealth; // Initialize health
    }

    public void TakeDamage(int damage)
    {
        //currentHealth -= damage;
        //Debug.Log($"{name} took {damage} damage. Remaining health: {currentHealth}");

        //if (IsDefeated)
        //{
        //    HandleDefeat();
        //}

        boss.TakeDamage(damage, new GameObject());
        Debug.Log("Take damage in Enemey Controller" + damage);

        minion.TakeDamage(damage);
    }

    void HandleDefeat()
    {
        Debug.Log($"{name} is defeated!");
        // Add logic to disable or destroy the enemy
        Destroy(gameObject); // For example, destroy the enemy
    }
}
