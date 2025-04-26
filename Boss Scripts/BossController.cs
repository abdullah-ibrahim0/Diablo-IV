using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;
using System.Collections.Generic; // For List<T>

public class BossController : MonoBehaviour
{
    // Summon Minions
    public GameObject minionPrefab; // Reference to the minion prefab
    public int numberOfMinions = 3; // Number of minions to spawn
    public float spawnRadius = 2f;  // Radius around boss to spawn minions

    // Boss Health
    public int maxHealth = 50;
    public int currentHealth;
    public Slider healthSlider;
    public TextMeshProUGUI healthText; // Reference to TextMeshProUGUI component

    // Boss Animator
    public Animator animator; // Reference to the Animator component

    // Dive Bomb Properties
    public GameObject DiveBombEffect;
    public float diveBombRange = 0.5f; // Range to check for Wanderers
    public int diveBombDamage = 20;  // Damage dealt by Dive Bomb

    private List<GameObject> activeMinions = new List<GameObject>(); // Track active minions

    //Shield
    public GameObject shieldEffect; // Reference to the shield particle system
    public int shieldMaxHP = 50; // Maximum shield HP
    private int shieldCurrentHP = -1; // Current shield HP
    public float shieldRegenCooldown = 10f; // Cooldown for shield regeneration
    private bool shieldActive = false; // Is the shield currently active?
    public Slider shieldSlider; // Reference to shield slider UI

    public bool phase2;

    // Aura Properties
    public bool auraActive = false; // Is the aura currently active?
    public int auraDamage = 15;    // Damage dealt by the aura to the attacker
    public GameObject auraEffect;  // Visual effect for the aura
    public bool auraDone = false;

    // Spike Attack Properties
    public GameObject spikeEffect; // Reference to the spike particle system
    public float spikeRange = 5f;  // Range of the spike attack
    public int spikeDamage = 10;   // Damage dealt by the spikes
    public float spikeDelay = 0.5f;

    // Boss Methods
    void Start()
    {
        currentHealth = maxHealth; // Initialize health to max
        healthSlider.maxValue = maxHealth; // Set slider max value
        healthSlider.value = currentHealth; // Set slider to current health
        UpdateHealthText();
    }

    void Update()
    {
        if (shieldCurrentHP == 0 && !shieldActive)
        {
            StartCoroutine(ShieldAndPhase2());
        }

        LookAtNearestWanderer();
    }

    public void TakeDamage(int damage,GameObject wanderer)
    {
        if(activeMinions.Count != 0)
        {
            return;
        }

        if (auraActive)
        {
            // Protect Lilith and shield
            TakeDamageFromWanderer(damage, wanderer);
            return;
        }
        
        if (shieldActive)
        {
            HandleShieldDamage(damage,wanderer);
        }

        else
        {
            Debug.Log("This was boss health" + currentHealth);
            currentHealth -= damage;
            Debug.Log("This is boss health now " + currentHealth);
            currentHealth = Mathf.Clamp(currentHealth, 0, maxHealth); // Ensure health is within bounds
            healthSlider.value = currentHealth; // Update slider

            UpdateHealthText();
            animator.SetBool("Damaged", true);
            Invoke(nameof(ResetHitState), 0.5f);


            // Check if all minions are defeated before allowing summoning
            if (currentHealth < 50 && !animator.GetBool("Summoning") && activeMinions.Count == 0 && !phase2)
            {
                animator.SetBool("Summoning", true); // Trigger Summon animation
                StartCoroutine(SummonMinionsWithDelay()); // Call coroutine to handle the delay
            }
        }
    }

    void ResetHitState()
    {
        animator.SetBool("Damaged", false); // Reset damaged animation state
    }

    void UpdateHealthText()
    {
        healthText.text = $"{currentHealth} / {maxHealth}"; // Update TextMeshPro text

        // If Health Equal 0 then dead
        if (currentHealth == 0)
        {
            animator.SetBool("Dead", true); // Trigger the death animation
            if(!phase2)
            {
                StartCoroutine(ShieldAndPhase2());
            }
            else
            {
                animator.SetBool("AlreadyDead", true);
            }
        }
    }

    // Minion Methods
    IEnumerator SummonMinionsWithDelay()
    {
        // Wait for 2 seconds to let the animation finish
        yield return new WaitForSeconds(1.5f);

        animator.SetBool("Summoning", false);

        // Spawn minions at random positions around the boss
        for (int i = 0; i < numberOfMinions; i++)
        {
            Vector2 randomPosition = Random.insideUnitCircle * spawnRadius;
            Vector3 spawnPosition = new Vector3(
                transform.position.x + randomPosition.x,
                transform.position.y,
                transform.position.z + randomPosition.y
            );

            GameObject minion = Instantiate(minionPrefab, spawnPosition, Quaternion.identity);
            activeMinions.Add(minion);
        }

        // Now trigger Dive Bomb animation after a 10-second delay
        yield return new WaitForSeconds(10f);
        TriggerDiveBomb();
    }

    // Trigger Dive Bomb animation and damage nearby Wanderers
    void TriggerDiveBomb()
    {
        animator.SetBool("DiveBomb", true); // Trigger Dive Bomb animation

        // Find all GameObjects with the "Wanderer" tag within the diveBombRange
        Collider[] hitColliders = Physics.OverlapSphere(transform.position, diveBombRange);
        foreach (var hitCollider in hitColliders)
        {
            if (hitCollider.CompareTag("Wanderer"))
            {
                WandererController wanderer = hitCollider.GetComponent<WandererController>();
                if (wanderer != null)
                {
                    // Start coroutine to apply damage after 2 seconds delay
                    StartCoroutine(ApplyDamageAfterDelay(wanderer, 2f)); // 2-second delay
                }
            }
        }

        // Stop Dive Bomb animation after it finishes
        StartCoroutine(EndDiveBombAnimation());
    }

    // Coroutine to apply damage after a delay
    IEnumerator ApplyDamageAfterDelay(WandererController wanderer, float delay)
    {
        yield return new WaitForSeconds(delay);  // Wait for the specified delay
        Debug.Log("Wanderer damaged by Dive Bomb after " + delay + " seconds.");
        wanderer.TakeDamage(diveBombDamage);  // Apply damage after the delay
    }

    // Coroutine to stop Dive Bomb animation after a delay
    IEnumerator EndDiveBombAnimation()
    {
        yield return new WaitForSeconds(1.7f); // Wait for the duration of the Dive Bomb animation
        DiveBombEffect.SetActive(true);
        animator.SetBool("DiveBomb", false); // Set DiveBomb to false after the animation ends
        yield return new WaitForSeconds(0.5f); // Wait for the duration of the Dive Bomb animation
        DiveBombEffect.SetActive(false);
    }

    // Method to be called when a minion is defeated
    public void MinionDefeated(GameObject minion)
    {
        Debug.Log(minion);
        if (activeMinions.Contains(minion))
        {
            activeMinions.Remove(minion);
        }
    }

    // Shield Methods
    void ActivateShield()
    {
        shieldCurrentHP = shieldMaxHP;
        shieldSlider.maxValue = shieldMaxHP;
        shieldSlider.value = shieldCurrentHP;
        shieldActive = true; // Mark shield as active
        shieldEffect.SetActive(true); // Enable shield particle effect
    }

    public void DeactivateShield()
    {
        shieldActive = false;
        shieldEffect.SetActive(false); // Disable shield particle effect

        //if (!auraDone)
        //{
            //auraDone = true;
            StartCoroutine(ActivateAura());
        //}

        StartCoroutine(TriggerSpikeAttack());
    }

    public void HandleShieldDamage(int damage, GameObject wanderer)
    {

        if (auraActive)
        {
            // Protect Lilith and shield
            TakeDamageFromWanderer(damage, wanderer);
        }

        // Ensure slider is synchronized
        if (shieldSlider.value != shieldCurrentHP)
        {
            shieldSlider.value = shieldCurrentHP;
            Debug.Log("Resynchronized shield slider.");
        }

        shieldCurrentHP -= damage;
        shieldSlider.value = shieldCurrentHP; // Update shield slider

        if (shieldCurrentHP <= 0)
        {
            int remainingDamage = Mathf.Abs(shieldCurrentHP);
            shieldCurrentHP = 0;
            shieldSlider.value = 0; // Set slider to zero
            DeactivateShield();
            TakeDamage(remainingDamage,wanderer); // Apply leftover damage to boss
        }
        else
        {
            Debug.Log("Shield absorbed damage. Remaining shield HP: " + shieldCurrentHP);
        }
    }

    IEnumerator ShieldAndPhase2()
    {
        // Wait for 10 seconds to delay health reset
        yield return new WaitForSeconds(10f); 
        
        if (!phase2)
        {
            StartPhase2();
        }

        // Start the shield activation after Phase 2 starts
        ActivateShield();    
    }

    void StartPhase2()
    {
        animator.SetBool("Dead", false); // Trigger the death animation
        phase2 = true; // Transition to Phase 2
        currentHealth = maxHealth;
        healthSlider.maxValue = maxHealth;
        healthSlider.value = currentHealth;
        UpdateHealthText();
    }

    // Aura Methods
    IEnumerator ActivateAura()
    {
        animator.SetBool("CastAura", true);
     
        //Wait for animation to finish
        yield return new WaitForSeconds(4f);

        animator.SetBool("CastAura", false);

        auraActive = true;
        auraEffect.SetActive(true); // Enable aura visual effect
        Debug.Log("Aura activated!");
    }

    public void DeactivateAura()
    {
        auraActive = false;
        auraEffect.SetActive(false); // Disable aura visual effect
        Debug.Log("Aura deactivated!");
        TriggerSpikeAttack();
    }

    public void TakeDamageFromWanderer(int damage, GameObject wanderer)
    {
        // Reflect damage to the Wanderer
        WandererController wandererController = wanderer.GetComponent<WandererController>();
        if (wandererController != null)
        {
            int totalDamageToWanderer = damage + auraDamage;
            wandererController.TakeDamage(totalDamageToWanderer); // Reflect damage
            Debug.Log($"Aura reflected {totalDamageToWanderer} damage to {wanderer.name}");
            
            // Deactivate the aura
            DeactivateAura();
        }

    }

    // Trigger the Spike Attack
    IEnumerator TriggerSpikeAttack()
    {
        yield return new WaitForSeconds(10f);

        animator.SetBool("SpikeAttack",true); // Set the spike attack animation to true
        StartCoroutine(SpikeAttackCoroutine());
    }

    // Coroutine for Spike Attack
    IEnumerator SpikeAttackCoroutine()
    {
        // Wait for the delay to sync with the animation
        yield return new WaitForSeconds(spikeDelay);

        animator.SetBool("SpikeAttack", false); // Set the spike attack animation to true

        // Activate the spike particle effect
        spikeEffect.SetActive(true);

        // Deal damage to Wanderers in range
        Collider[] hitColliders = Physics.OverlapSphere(transform.position, spikeRange);
        foreach (var collider in hitColliders)
        {
            if (collider.CompareTag("Wanderer"))
            {
                WandererController wanderer = collider.GetComponent<WandererController>();
                if (wanderer != null)
                {
                    wanderer.TakeDamage(spikeDamage);
                    Debug.Log($"Spike Attack hit {collider.name} for {spikeDamage} damage!");
                }
            }
        }

        // Deactivate the particle effect after it finishes
        yield return new WaitForSeconds(2f); // Match this duration to the particle system lifetime
        spikeEffect.SetActive(false);
    }

    void LookAtNearestWanderer()
    {
        // Find all wanderers
        GameObject[] wanderers = GameObject.FindGameObjectsWithTag("Wanderer");
        if (wanderers.Length == 0) return; // Exit if no wanderers are found

        // Find the nearest wanderer
        GameObject nearestWanderer = null;
        float nearestDistance = float.MaxValue;

        foreach (GameObject wanderer in wanderers)
        {
            float distance = Vector3.Distance(transform.position, wanderer.transform.position);
            if (distance < nearestDistance)
            {
                nearestDistance = distance;
                nearestWanderer = wanderer;
            }
        }

        if (nearestWanderer != null)
        {
            // Make the boss look at the nearest wanderer
            Vector3 direction = (nearestWanderer.transform.position - transform.position).normalized;
            Quaternion lookRotation = Quaternion.LookRotation(new Vector3(direction.x, 0, direction.z));
            transform.rotation = Quaternion.Slerp(transform.rotation, lookRotation, Time.deltaTime * 2f); // Smooth rotation
        }
    }
}