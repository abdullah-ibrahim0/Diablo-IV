using UnityEngine;

public class DamageDealer : MonoBehaviour
{
    public int damageAmount = 10;
    
    public BossController boss;
    public MinionController minion;

    void Start()
    {
        
    }
    void Update()
    {
        //if (Input.GetKeyDown(KeyCode.Space)) // Simulate damage when pressing Space
        //{
            //boss.TakeDamage(damageAmount);
        //}

        //if (Input.GetKeyDown(KeyCode.S))
        //{
        //    boss.HandleShieldDamage(15);  // Deal 5 damage when M key is pressed
        //}
    }
}
