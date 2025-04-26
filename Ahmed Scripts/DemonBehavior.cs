using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEditor;
using UnityEngine;

public class DemonBehavior : MonoBehaviour
{
    public Transform waypointsParent;

    public float speed = 2f;
    public float waitTime = 2f;
    public float playerDetectionRange = 60f;
    public int health = 40;
    public float range = 5f;
    public GameObject explosivePrefab;
    public bool IsChasing { get; private set; } = false;
    public bool isDead = false;

    public float attackCooldown = 7f;
    public float swordRange = 1f;
    public float explosiveRange = 15f;

    private Transform[] waypoints;
    private int currentWaypointIndex = 0;
    private Animator animator;
    private Transform player;
    private float waypointWaitTimer = 1f;
    private float attackTimer = 0f;
    private bool isWaiting = false;
    private int swordHits = 0;
    private PlayerBehavior playerBehavior;

    public GameObject sorcererPrefab; // Reference to the Sorcerer prefab
    private SorcererHealth sorcererHealth; // Reference to SorcererHealth component
    private SorcererAbilities sorcererAbilities;

    public GameObject barbarianPrefab;
    private PlayerController barbarianHealth;

    public GameObject roguePrefab;
    private RogueController rogueHealth;

    public GameObject campCollider;
private CampPlayerEnter campPlayerEnter;

    // Start is called before the first frame update
    void Start()
    {
        
        animator = GetComponent<Animator>();
        player = GameObject.FindGameObjectWithTag("Wanderer").transform;
        playerBehavior = player.GetComponent<PlayerBehavior>();
        waypoints = new Transform[waypointsParent.childCount];
        campPlayerEnter = campCollider.GetComponent<CampPlayerEnter>();
        for (int i = 0; i < waypointsParent.childCount; i++)
        {
            waypoints[i] = waypointsParent.GetChild(i);
        }
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

        //StartCoroutine(Patrol());
    }


    // Update is called once per frame
    private void Update()
{

    


    if (!isDead)
    {

        if (IsChasing && campPlayerEnter.isPlayerInCamp)
        {
            StartChasing();
        }
        else if (!IsChasing || !campPlayerEnter.isPlayerInCamp)
        {
            Patrol();
        }
        if (attackTimer > 0f)
        {
            attackTimer -= Time.deltaTime;
        }
    }
    else
    {
        animator.SetBool("isDead", true);

    }
}

    public void StartChasing()
    {
        IsChasing = true;
        float distanceToPlayer = Vector3.Distance(transform.position, player.position);
        if (distanceToPlayer > swordRange)
        {
            animator.SetBool("isRunning", true);
            animator.SetBool("IsWalking", false);
            RunTowardsPlayer();
        }
        else if (distanceToPlayer <= swordRange * 1.5)
        {
            animator.SetBool("isRunning", false);
            if (attackTimer <= 0f && swordHits < 2)
            {
                Debug.Log("ATTACK");
                SwordAttack();
            }
        }
        if (distanceToPlayer <= explosiveRange && swordHits == 2 && attackTimer <= 0f)
        {
            Debug.Log("THROW");
            AttackWithExplosive();
        }
    }

    public void StopChasing()
    {
        IsChasing = false;
        animator.SetBool("isRunning", false);
        animator.SetBool("isPatrolling", true); // Go back to patrolling
        Patrol();
    }

    private void Patrol()
    {
        if (isWaiting)
        {
            waypointWaitTimer -= Time.deltaTime;
            if (waypointWaitTimer <= 0f)
            {
                isWaiting = false;
                currentWaypointIndex = (currentWaypointIndex + 1) % waypoints.Length;
            }
            return;
        }
        animator.SetBool("IsWalking", true);

        Transform targetWaypoint = waypoints[currentWaypointIndex];
        float distanceToWaypoint = Vector3.Distance(transform.position, targetWaypoint.position);

        if (distanceToWaypoint > 1f)
        {
            Vector3 direction = (targetWaypoint.position - transform.position).normalized;
            transform.position += direction * speed * Time.deltaTime;
            SmoothLookAt(targetWaypoint.position);
            animator.SetBool("IsWalking", true);
        }
        else
        {
            isWaiting = true;
            waypointWaitTimer = waitTime;
            animator.SetBool("IsWalking", false);
        }
    }


    private void RunTowardsPlayer()
    {
        Debug.Log("Running");
        Vector3 direction = (player.position - transform.position).normalized;
        transform.position += direction * speed * 2 * Time.deltaTime;
        SmoothLookAt(player.position);
    }

    private void SwordAttack()
    {
        float distanceToPlayer = Vector3.Distance(transform.position, player.position);
        attackTimer = attackCooldown;
        swordHits++;
        if (distanceToPlayer <= swordRange){
            
            if (sorcererHealth != null && sorcererHealth.currentHealth > 0)
            {
                Debug.Log("Attacking Sorcerer");
                animator.SetTrigger("SwingSword");
                sorcererHealth.TakeDamage(10);
            }
            if (barbarianHealth != null && barbarianHealth.hp > 0)
            {
                Debug.Log("Attacking Barbarian");
                animator.SetTrigger("SwingSword");
                barbarianHealth.Damage(10);
            }
            if (rogueHealth != null && rogueHealth.hp > 0)
            {
                Debug.Log("Attacking Rogue");
                animator.SetTrigger("SwingSword");
                rogueHealth.Damage(10);
            }
            
        }
    }

    private void SmoothLookAt(Vector3 targetPosition)
    {
        Vector3 direction = (targetPosition - transform.position).normalized;
        Quaternion lookRotation = Quaternion.LookRotation(direction);
        transform.rotation = Quaternion.Slerp(transform.rotation, lookRotation, Time.deltaTime * 5f);
    }

    private void AttackWithExplosive()
    {
        animator.SetTrigger("ThrowExplosion");
        attackTimer = attackCooldown;
        swordHits = 0;
        float distanceToPlayer = Vector3.Distance(transform.position, player.position);
        Vector3 spawnPosition = transform.position + transform.forward * 1.5f + transform.up * 1.2f;
        GameObject explosive = Instantiate(explosivePrefab, spawnPosition, Quaternion.identity);
        Rigidbody explosiveRb = explosive.GetComponent<Rigidbody>();
        Vector3 direction = (player.position - transform.position).normalized;

        if (distanceToPlayer < 3f)
            explosiveRb.AddForce(direction * 2f, ForceMode.Impulse);
        else
            explosiveRb.AddForce(direction * 7f, ForceMode.Impulse);

        StartCoroutine(TriggerExplosionAfterDelay(explosive));
    }

    IEnumerator TriggerExplosionAfterDelay(GameObject explosive)
    {
        yield return new WaitForSeconds(3f);

        ParticleSystem explosion = explosive.GetComponent<ParticleSystem>();

        if (explosion != null)
        {
            Debug.Log("BOOM");
            explosion.Play();
        }
        float distanceToPlayer = Vector3.Distance(explosive.transform.position, player.position);

        if (distanceToPlayer <= explosiveRange)
        {
            
            if (sorcererHealth != null && sorcererHealth.currentHealth > 0)
            {
                Debug.Log("Attacking Sorcerer");
                sorcererHealth.TakeDamage(15);
            }
            if (barbarianHealth != null && barbarianHealth.hp > 0)
            {
                Debug.Log("Attacking Barbarian");
                barbarianHealth.Damage(15);
            }
            if (rogueHealth != null && rogueHealth.hp > 0)
            {
                Debug.Log("Attacking Rogue");
                rogueHealth.Damage(15);
            }
            
        
        }

        yield return new WaitForSeconds(0.3f);
        Destroy(explosive);
    }

    public void TakeDamage(int damage)
    {
        health -= damage;
        animator.SetTrigger("Hit");
        if (health <= 0) // If health is 0 or below, the minion dies
        {
            
            if (sorcererHealth != null && sorcererHealth.currentHealth > 0)
            {
                sorcererAbilities.xp += 30 ;
            }
            if (barbarianHealth != null && barbarianHealth.hp > 0)
            {
                barbarianHealth.xp += 30;
   
            }
            if (rogueHealth != null && rogueHealth.hp > 0)
            {
                rogueHealth.xp += 30;
            }
            Die();

            //Destroy(gameObject); // Destroy the minion game object after a delay
        }
         Debug.Log("Ataaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaa"+health);
    }

    void Die()
    {
        animator.SetTrigger("Die");
        StopChasing();
        isDead = true;
        Destroy(gameObject, 5f);
    }
    
}