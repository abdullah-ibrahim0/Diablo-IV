using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class MinionMovement : MonoBehaviour
{
    private Transform player; // Reference to the player's transform
    private Transform clone;
    public float rotationSpeed = 5f; // Speed of rotation towards the player
    public float moveSpeed = 2f;
    public float stoppingDistance = 0.5f;
    public float detectionRange = 10f;
    public float distanceToAttack = 1f;
    public Animator animator;
    public Slider healthbar;
    public int health = 20;

    public GameObject sorcererPrefab; // Reference to the Sorcerer prefab
    private SorcererHealth sorcererHealth; // Reference to SorcererHealth component
    private SorcererAbilities sorcererAbilities;

    private float attackCooldown = 3f; // Time (in seconds) between attacks
    private float lastAttackTime = -Mathf.Infinity;

    private bool notDead;
    private bool isAttacking;

    // Static counter to track the number of minions attacking
    private static int activeAttackers = 0;
    private static int maxAttackers = 5;

    private static int activeChasers = 0;
    private static int maxChasers = 5;

    //public WandererController barbarian;
    //public WandererController rogue;

    public GameObject barbarianPrefab;
    private PlayerController barbarianHealth;

    public GameObject roguePrefab;
    private RogueController rogueHealth;

    private BossController bossController;

    [SerializeField] AudioClip audioSource;
    public AudioSource audo;

    private static readonly object lockObject = new object();

    void Start()
    {
        animator = GetComponent<Animator>();
        healthbar.maxValue = health;

        // Find the active Wanderer in the scene
        GameObject activeWanderer = GameObject.FindGameObjectWithTag("Wanderer");
        if (activeWanderer != null)
        {
            player = activeWanderer.transform;

            // Determine if it's a Sorcerer or Barbarian
            sorcererHealth = activeWanderer.GetComponent<SorcererHealth>();
            sorcererAbilities = activeWanderer.GetComponent<SorcererAbilities>();
            barbarianHealth = activeWanderer.GetComponent<PlayerController>();
            rogueHealth = activeWanderer.GetComponent<RogueController>();

            if (sorcererHealth == null && barbarianHealth == null && rogueHealth == null)
            {
                Debug.LogError("No valid health component found on the active Wanderer!");
            }
        }
        else
        {
            Debug.LogError("No Wanderer found in the scene!");
        }

        GameObject[] enemies = GameObject.FindGameObjectsWithTag("enemy");

        foreach (GameObject enemy in enemies)
        {
            // Check if the enemy has a BossController script
            BossController boss = enemy.GetComponent<BossController>();
            if (boss != null)
            {
                bossController = boss; // Assign the BossController reference
                break; // Exit the loop once the boss is found
            }
        }

    }

    void Update()
    {
        Transform currentTarget = GetCurrentTarget(); // Get the active target (player or clone)
        Debug.Log($"Active Chasers: {activeChasers}, Max Chasers: {maxChasers}");
        Debug.Log($"Active Attackers: {activeAttackers}, Max Attackers: {maxAttackers}");


        // Ensure the target reference is still valid
        if (currentTarget == null)
        {
            Debug.LogError("Target reference lost! Searching again...");
            Start(); // Reinitialize
            return;
        }

        // Handle attack logic
if (player != null && playerInRange(player))
{
    // Allow attacking only if activeAttackers is within limit
    if (activeAttackers < maxAttackers && !isAttacking && Time.time - lastAttackTime >= attackCooldown)
    {
        StartAttack();
    }

    if (isAttacking && demonAttack(currentTarget) && !notDead && Time.time - lastAttackTime >= attackCooldown)
    {
        lastAttackTime = Time.time; // Record the time of this attack

        if (currentTarget == clone)
        {
            Debug.Log("Attacking clone. No damage applied.");
            animator.SetTrigger("hit");
        }
        else
        {
            if (sorcererHealth != null && sorcererHealth.currentHealth > 0)
            {
                Debug.Log("Attacking Sorcerer");
                animator.SetTrigger("hit");
                sorcererHealth.TakeDamage(5);
            }
            if (barbarianHealth != null && barbarianHealth.hp > 0)
            {
                Debug.Log("Attacking Barbarian");
                animator.SetTrigger("hit");
                barbarianHealth.Damage(5);
            }
            if (rogueHealth != null && rogueHealth.hp > 0)
            {
                Debug.Log("Attacking Rogue");
                animator.SetTrigger("hit");
                rogueHealth.Damage(5);
            }
            audo.PlayOneShot(audioSource);
        }
    }
    else if (isAttacking && Time.time - lastAttackTime >= attackCooldown)
    {
        StopAttack();
    }
}




        // Movement logic
        if (playerInRange(currentTarget) && !notDead)
{
    if (activeChasers < maxChasers && !animator.GetBool("detected"))
    {
        IncrementChasers(); // Increment chaser count only if not already chasing
        animator.SetBool("detected", true); // Mark this minion as chasing
    }

    if (animator.GetBool("detected"))
    {
        MoveTowardsTarget(currentTarget); // Continue moving if chasing
    }
}
else
{
    if (animator.GetBool("detected"))
    {
        DecrementChasers(); // Decrement chaser count if this minion stops chasing
        animator.SetBool("detected", false); // Mark this minion as no longer chasing
    }
}


        // Handle death
        if (health <= 0 && !notDead) // If health is 0 or below, and the minion isn't marked as dead
        {
            animator.SetTrigger("die"); // Trigger death animation
            if (bossController != null)
            {
                bossController.MinionDefeated(gameObject); // Notify the boss
            }
            StartCoroutine(dieDelay1()); // Add a delay before destroying the object
            notDead = true; // Prevent further death logic from triggering
        }


        // Smooth rotation towards the target
        RotateTowardsTarget(currentTarget);
    }

    void LateUpdate()
    {
        if (animator.GetBool("getHit"))
        {
            animator.SetBool("getHit", false);
        }
    }

    void IncrementAttackers()
{
    lock (lockObject)
    {
        if (activeAttackers < maxAttackers)
        {
            activeAttackers++;
            Debug.Log($"Incrementing activeAttackers: {activeAttackers}");
        }
    }
}

void DecrementAttackers()
{
    lock (lockObject)
    {
        if (activeAttackers > 0)
        {
            activeAttackers--;
            Debug.Log($"Decrementing activeAttackers: {activeAttackers}");
        }
    }
}

void IncrementChasers()
{
    lock (lockObject)
    {
        if (activeChasers < maxChasers)
        {
            activeChasers++;
        }
    }
}

void DecrementChasers()
{
    lock (lockObject)
    {
        if (activeChasers > 0)
        {
            activeChasers--;
        }
    }
}

    void StartAttack()
{
    IncrementAttackers(); // Increment the global count of attackers
    isAttacking = true; // Mark this minion as attacking
    lastAttackTime = Time.time; // Reset the attack cooldown timer
}

void StopAttack()
{
    DecrementAttackers(); // Decrement the global count of attackers
    isAttacking = false; // Mark this minion as no longer attacking
}

void OnDestroy()
{
    if (isAttacking)
    {
        StopAttack(); // Decrement activeAttackers
    }
    else if (activeChasers > 0)
    {
        DecrementChasers(); // Decrement activeChasers if destroyed while chasing
    }
}


    void RotateTowardsTarget(Transform target)
    {
        if (target == null) return;

        Vector3 direction = (target.position - transform.position).normalized;
        Quaternion targetRotation = Quaternion.LookRotation(direction);
        transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * rotationSpeed);
    }

    bool demonAttack(Transform target)
    {
        if (target == null) return false;

        float distance = Vector3.Distance(transform.position, target.position);
        return distance <= distanceToAttack;
    }

    bool playerInRange(Transform target)
    {
        if (target == null) return false;

        float distance = Vector3.Distance(transform.position, target.position);
        return distance <= detectionRange;
    }

    void MoveTowardsTarget(Transform target)
    {
        if (target == null) return;

        float distance = Vector3.Distance(transform.position, target.position);
        if (distance > stoppingDistance)
        {
            Vector3 direction = (target.position - transform.position).normalized;
            transform.position += direction * moveSpeed * Time.deltaTime;
        }
    }



    public void TakeDamage(int damage)
    {
        health -= damage; // Subtract health by the amount of damage received
        health = Mathf.Clamp(health, 0, health); // Ensure health doesn't drop below 0

        healthbar.value = health; // Update health slider

        animator.SetBool("getHit", true); // Trigger the "getHit" animation when damage is taken

        if (health <= 0) // If health is 0 or below, the minion dies
        {
            animator.SetTrigger("die"); // Trigger death animation
            if (sorcererHealth != null && sorcererHealth.currentHealth > 0)
            {
                sorcererAbilities.xp += 10 ;
            }
            if (barbarianHealth != null && barbarianHealth.hp > 0)
            {
                barbarianHealth.xp += 10;
   
            }
            if (rogueHealth != null && rogueHealth.hp > 0)
            {
                rogueHealth.xp += 10;
            }
            StartCoroutine(dieDelay1());

            //Destroy(gameObject); // Destroy the minion game object after a delay
        }
    }

    IEnumerator dieDelay1()
    {
        yield return new WaitForSeconds(5f);
        Destroy(gameObject); // Destroy the minion after the delay

    }

    Transform GetCurrentTarget()
    {
        // If the clone exists, prioritize it as the target. Otherwise, target the player.
        return clone != null ? clone : player;
    }

    public void SetClone(Transform newClone)
    {
        clone = newClone;
        Debug.Log("Clone set as target for minions.");

    }

    public void ClearClone()
    {
        clone = null;
        Debug.Log("Clone cleared. Targeting original player.");
    }

}