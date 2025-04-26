using UnityEngine;
using UnityEngine.UI;  // For working with UI elements

public class WandererController : MonoBehaviour
{
    //public int maxHealth = 50;      // Wanderer's maximum health
    //public int currentHealth;       // Wanderer's current health
    //public Slider healthBar;        // Health bar UI element
    public Animator animator;       // Reference to the Animator for death animation

    public BossController boss;

    public RogueController rogue;

    public PlayerController barbarian;

    public SorcererHealth sorcerer;

    [Header("Sound Effects")]
    [SerializeField] AudioSource audioSource; // Reference to the AudioSource component
    [SerializeField] AudioClip damagedSound;

    void Start()
    {
        //currentHealth = maxHealth;  // Initialize health to max
        //healthBar.maxValue = maxHealth;  // Set the max health on the slider
        //healthBar.value = currentHealth; // Set the initial health on the slider
        
        if (audioSource == null)
        {
            audioSource = GetComponent<AudioSource>();
        }
    }

    void Update()
    {
        //if (Input.GetKeyDown(KeyCode.S))
        //{
        //   boss.HandleShieldDamage(15,gameObject);  // Deal 15 damage when S key is pressed
        //}

    }

        // Method to handle damage taken by the Wanderer
        public void TakeDamage(int damage)
    {
        //Debug.Log("I am at Wand Take Damage");
        //currentHealth -= damage;  // Reduce current health
        //currentHealth = Mathf.Clamp(currentHealth, 0, maxHealth); // Ensure health doesn't go below 0

        //healthBar.value = currentHealth;  // Update the health bar slider

        //// Check if the Wanderer's health is 0 (death condition)
        //if (currentHealth == 0)
        //{
        //    Die();
        //}
        Debug.Log("Checking for Rogue and Barbarian...");

        if (rogue != null && rogue.gameObject.activeInHierarchy)
        {
            Debug.Log("Rogue is active and enabled in the scene.");
            rogue.Damage(damage);
        }

        if (barbarian != null && barbarian.gameObject.activeInHierarchy)
        {
            Debug.Log("Barbarian is active and enabled in the scene.");
            barbarian.Damage(damage);
        }

        //Debug.Log("Hi" + sorcerer.gameObject.activeInHierarchy);

        if (sorcerer != null && sorcerer.gameObject.activeInHierarchy)
        {
            Debug.Log("Sorcerer is active and enabled in the scene.");
            sorcerer.TakeDamage(damage);
        }

        if (audioSource != null)
        {
            audioSource.Play(); // Play the specific clip
        }
    }

    // Method to handle the death of the Wanderer
    void Die()
    {
        if (animator != null)
        {
            animator.SetTrigger("Die");  // Play the death animation (trigger should be defined in Animator)
        }

        // Optionally, you can notify the BossController if needed (e.g., for the Boss to track defeated Wanderers)
        BossController boss = FindObjectOfType<BossController>();
        if (boss != null)
        {
            boss.MinionDefeated(gameObject); // Notify the BossController when the Wanderer dies
        }

        Destroy(gameObject);  // Destroy the Wanderer object after a short delay (animation time)
    }
}