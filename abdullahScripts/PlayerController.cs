using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.UI;
using TMPro;

public class PlayerController : MonoBehaviour
{

    CustomActions input;

    NavMeshAgent agent;
    Animator animator;

    public Slider healthBarSlider; // Reference to the UI slider
    public Text healthText; // Reference to display health as text (optional)
    
    public int hp;
    public int maxHp;
    public int xp;
    public int level;
    public int xpToNextLevel;
    public int ap;
    public int runeFragments;
    public int potions;

    const string IDLE = "Idle";
    const string WALK = "Walk";
    const string BASH = "Bash";
    const string IRON_MAELSTROM = "IronMaelstrom";
    const string CHARGE = "Charge";

    public GameObject GameOver;


    [Header("Movement")]
    [SerializeField] ParticleSystem clickEffect;
    [SerializeField] LayerMask clickableLayers;

    [Header("Abilities")]
    [SerializeField] LayerMask enemyLayer;
    [SerializeField] LayerMask destructibleLayer;
    [SerializeField] float attackRange = 1.5f;

    [Header("Bash")]
    [SerializeField] int bashDamage = 5;
    [SerializeField] float bashCooldown = 1f;

    [Header("Iron Maelstrom")]
    [SerializeField] float maelstromRange = 3f;
    [SerializeField] int maelstromDamage = 10;
    [SerializeField] float maelstromCooldown = 5f;
    [SerializeField] ParticleSystem maelstromEffect;

    public BossController boss;

    [Header("Charge")]
    [SerializeField] float chargeSpeed = 2f;
    [SerializeField] float chargeRange = 30f;
    [SerializeField] int mainLevelChargeDamage = int.MaxValue;
    [SerializeField] int bossLevelChargeDamage = 20;
    [SerializeField] float chargeCooldown = 20f;
    [SerializeField] ParticleSystem chargeEffect;
    [SerializeField] LayerMask walkableSurface;

    [Header("Shield")]
    [SerializeField] GameObject shieldObject; // The shield sphere object
    [SerializeField] float shieldDuration = 3f; // Duration of the shield
    [SerializeField] float shieldCooldown = 10f; // Cooldown before shield can be reused

    public GameObject leftDoor;      // Reference to the left door
    public GameObject rightDoor;     // Reference to the right door
    public float proximityDistance = 5f; // Distance within which doors will open
    public float doorMoveSpeed = 2f; // Speed of the door movement
    public float doorOpenOffset = 3f; // How far the doors will move when opening
    
    private Vector3 leftDoorClosedPosition;  // Initial position of the left door
    private Vector3 rightDoorClosedPosition; // Initial position of the right door
    private Vector3 leftDoorOpenPosition;    // Target open position for the left door
    private Vector3 rightDoorOpenPosition;   // Target open position for the right door

    private bool isShieldActive = false;
    private bool isShieldCooldown = false;

    public bool ability2Unlocked;
    public bool ability3Unlocked;
    public bool ability4Unlocked;

    public bool invincible = false;



    public TextMeshProUGUI hud;
    public TextMeshProUGUI abilities;

    public Button unlockAbility2;
    public Button unlockAbility3;
    public Button unlockAbility4;



    private bool isSlowMotion = false;

    

    public int ability;

    float lookRotationSpeed = 8f;
    bool isAttacking = false;
    bool isCooldown = false;
    bool hasAttacked = false;
    bool isMaelstromCooldown = false;
    bool isChargeCooldown = false;
    bool isCharging = false;
    bool finishedAttack = true;

    private EnemyController selectedEnemy;
    private Vector3 chargeDestination;
    public bool isInBossLevel = false;

   void Awake()
    {


        agent = GetComponent<NavMeshAgent>();
        animator = GetComponent<Animator>();

        hp = 100;
        xp = 0;
        level = 1;
        maxHp = 100 * level;
        xpToNextLevel = 100 * level;
        ap = 0;
        runeFragments = 0;
        potions = 0;

        ability2Unlocked = false;
        ability3Unlocked = false;
        ability4Unlocked = false;

        ability = 0;

        // Store initial positions
        leftDoorClosedPosition = leftDoor.transform.position;
        rightDoorClosedPosition = rightDoor.transform.position;

        // Calculate target open positions
        leftDoorOpenPosition = leftDoorClosedPosition + new Vector3(doorOpenOffset, 0, 0); // Move left
        rightDoorOpenPosition = rightDoorClosedPosition + new Vector3(-doorOpenOffset, 0, 0); // Move right

        UpdateHealthBar();
        input = new CustomActions();
        AssignInputs();
    }

    public void UnlockAbility(int ability)
    {
        if (ability == 2)
        {
            ability2Unlocked = true;
        }
        if (ability == 3)
        {
            ability3Unlocked = true;
        }
        if (ability == 4)
        {
            ability4Unlocked = true;
        }
        ap--;
    }


    

    void AssignInputs()
    {
        input.Main.Move.performed += ctx => ClickToMove();
        input.Main.Ability.performed += ctx => SelectEnemy();
        input.Main.WildCard.performed += ctx => TryIronMaelstrom();
        input.Main.Ultimate.performed += ctx => TryCharge();
        input.Main.Shield.performed += ctx => TryActivateShield(); // Bind shield activation
    }

     void UpdateHealthBar()
{
    if (healthBarSlider != null)
    {
        healthBarSlider.value = hp;

        // Change health bar color if shield is active
        if (isShieldActive)
        {
            healthBarSlider.fillRect.GetComponent<Image>().color = Color.cyan; // Shield active color
        }
        else
        {
            healthBarSlider.fillRect.GetComponent<Image>().color = Color.green; // Normal health color
        }
    }

    if (healthText != null)
    {
        healthText.text = $"{hp}/{maxHp}";
    }
}


    void TryActivateShield()
    {
        if (!isShieldCooldown && CanPerformAction()&& ability2Unlocked)
        {
            Debug.Log("Shield activated.");
            StartCoroutine(ActivateShield());
        }
        else
        {
            Debug.Log("Shield is on cooldown or cannot perform action.");
        }
    }

    IEnumerator ActivateShield()
    {
        isShieldActive = true;
        isShieldCooldown = true;

        // Activate the shield object
        shieldObject.SetActive(true);
        Debug.Log("Shield is active.");

        yield return new WaitForSeconds(shieldDuration);

        // Deactivate the shield object
        shieldObject.SetActive(false);
        isShieldActive = false;
        Debug.Log("Shield expired.");

        StartCoroutine(ShieldCooldown());
        ResetState();
    }

    IEnumerator ShieldCooldown()
    {
        Debug.Log("Shield cooldown started.");
        yield return new WaitForSeconds(shieldCooldown);
        isShieldCooldown = false;
        Debug.Log("Shield cooldown ended.");
    }

    public void TakeDamage(int damage)
    {
        if (isShieldActive)
        {
            Debug.Log("Shield blocked all damage.");
            return; // No damage taken when shield is active
        }

        // Apply damage logic (e.g., reduce health)
        Debug.Log($"Player takes {damage} damage.");
    }

    

    void TryCharge()
{
    if (!isChargeCooldown && CanPerformAction()&& ability4Unlocked)
    {
        Debug.Log("Charge initiated.");
        StartCoroutine(PrepareCharge());
    }
    else
    {
        Debug.Log("Charge is on cooldown or cannot perform action.");
    }
}

IEnumerator PrepareCharge()
{
    // Wait for right-click to select charge destination
    bool destinationSelected = false;
    Vector3 selectedDestination = Vector3.zero;

    while (!destinationSelected)
    {
        if (Input.GetMouseButtonDown(1)) // Right mouse button
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;

            // Check if the right-click is on any surface (no layer mask)
            if (Physics.Raycast(ray, out hit, chargeRange)) // no layer mask specified
            {
                selectedDestination = hit.point;
                destinationSelected = true;
                Debug.Log($"Charge destination selected at: {selectedDestination}");
            }
            else
            {
                Debug.Log("Invalid destination for charge.");
            }
        }
        yield return null;
    }

    // Perform the charge
    StartCoroutine(PerformCharge(selectedDestination));
}


    IEnumerator PerformCharge(Vector3 destination)
    {
        isAttacking = true;
        isCharging = true;
        isChargeCooldown = true;
        agent.isStopped = true;

        // Start running animation
        animator.SetBool("IsRunning", true);

        Vector3 chargeDirection = (destination - transform.position).normalized;
        float chargeDistance = Vector3.Distance(transform.position, destination);

        // Set a desired fixed charge speed
        float chargeSpeed = 5f; // Adjust this value as needed (lower value for slower charge speed)

        // Calculate the charge duration based on charge speed and distance, but ensure a minimum duration
        float chargeDuration = Mathf.Max(chargeDistance / chargeSpeed, 2f); // Minimum charge duration of 2 seconds

        float elapsedTime = 0f;
        Vector3 startPosition = transform.position;

        // Track already hit targets
        HashSet<MinionMovement> hitMinions = new HashSet<MinionMovement>();
        HashSet<GameObject> hitDestructibles = new HashSet<GameObject>();
        HashSet<BossController> hitBoss = new HashSet<BossController>();



        while (elapsedTime < chargeDuration)
        {
            animator.SetBool("IsRunning", true);
            // Move towards destination using the charge speed
            float progress = elapsedTime / chargeDuration;
            transform.position = Vector3.Lerp(startPosition, destination, progress);

            // Rotate to face the destination
            Vector3 directionToTarget = (destination - transform.position).normalized;
            if (directionToTarget.magnitude > 0.01f)
            {
                Quaternion lookRotation = Quaternion.LookRotation(directionToTarget);
                transform.rotation = Quaternion.Slerp(transform.rotation, lookRotation, Time.deltaTime * lookRotationSpeed);
            }

            // Check for minions and bosses along the path
            Collider[] hitColliders = Physics.OverlapSphere(transform.position, 1f, enemyLayer);
            foreach (var hitCollider in hitColliders)
            {
                // If it's a minion, apply damage
                if (hitCollider.CompareTag("enemy"))
                {
                    MinionMovement minion = hitCollider.GetComponent<MinionMovement>();
                    if (minion != null && !hitMinions.Contains(minion))
                    {
                        animator.SetTrigger("Charge");
                        hitMinions.Add(minion); // Mark minion as hit
                        int chargeDamage = isInBossLevel ? bossLevelChargeDamage : mainLevelChargeDamage;
                        Debug.Log($"Charging into minion: {minion.name}, dealing {chargeDamage} damage.");
                        minion.TakeDamage(chargeDamage);
                    }
                }

                // If it's a boss, apply damage
                if (hitCollider.CompareTag("enemy"))
                {
                    BossController boss = hitCollider.GetComponent<BossController>();
                    if (boss != null && !hitBoss.Contains(boss))
                    {
                        animator.SetTrigger("Charge");
                        hitBoss.Add(boss); // Mark minion as hit
                        boss.TakeDamage(20, gameObject);
                        Debug.Log($"Charging into boss: {boss.name}, dealing {20} damage.");
                    }
                }
            }

            // Check for destructibles
            Collider[] destructiblesHit = Physics.OverlapSphere(transform.position, 1f, destructibleLayer);
            foreach (var destructible in destructiblesHit)
            {
                if (!hitDestructibles.Contains(destructible.gameObject))
                {
                    animator.SetTrigger("Charge");
                    hitDestructibles.Add(destructible.gameObject); // Mark destructible as hit
                    Debug.Log($"Destroying destructible object: {destructible.name}");
                }
            }

            elapsedTime += Time.deltaTime;
            yield return null;
        }

        // Ensure final position is exactly at destination
        transform.position = destination;

        // Stop running animation
        animator.SetBool("IsRunning", false);

        // Start cooldown
        StartCoroutine(ChargeCooldown());

        // Reset state
        isAttacking = false;
        isCharging = false;
        StopMovementAndResetState();
    }





    IEnumerator ChargeCooldown()
{
    Debug.Log("Charge cooldown started.");
    yield return new WaitForSeconds(chargeCooldown);
    isChargeCooldown = false;
    Debug.Log("Charge cooldown completed.");
}
    

    void TryIronMaelstrom()
    {
        if (!isMaelstromCooldown && CanPerformAction() && ability3Unlocked)
        {
            StartCoroutine(PerformIronMaelstrom());
        }
    }

    IEnumerator PerformIronMaelstrom()
    {
        isAttacking = true;
        isMaelstromCooldown = true;

        agent.isStopped = true; // Stop movement
        animator.SetTrigger(IRON_MAELSTROM); // Play ability animation


        yield return new WaitForSeconds(1.2f); // Animation timing

        // Find all enemies within range
        //Collider[] enemies = Physics.OverlapSphere(transform.position, maelstromRange, enemyLayer);
        //foreach (var enemyCollider in enemies)
        //{
        //    EnemyController enemy = enemyCollider.GetComponent<EnemyController>();
        //    if (enemy != null)
        //    {
        //        enemy.TakeDamage(maelstromDamage);
        //    }
        //}

        Collider[] hitMinions = Physics.OverlapSphere(transform.position, maelstromRange, enemyLayer);
        foreach (var hitCollider in hitMinions)
        {
            if (hitCollider.CompareTag("enemy"))
            {
                DemonBehavior demon = hitCollider.GetComponent<DemonBehavior>();
                MinionMovement minion = hitCollider.GetComponent<MinionMovement>();
                if (minion != null)
                {
                    minion.TakeDamage(maelstromDamage); // Apply the same damage to the minion
                    Debug.Log($"Damaging minion {hitCollider.name} with {maelstromDamage} damage.");
                }
                if (demon != null)
                {
                    demon.TakeDamage(maelstromDamage); // Apply the same damage to the minion
                    Debug.Log($"Damaging minion {hitCollider.name} with {maelstromDamage} damage.");
                }
            }

                BossController boss = hitCollider.GetComponent<BossController>();
            // If the boss is in range, apply damage to the boss as well
        if (boss != null && Vector3.Distance(transform.position, boss.transform.position) <= maelstromRange)
        {
            Debug.Log("sssssssssssssssssssssssssssssssssssssss");
            boss.TakeDamage(maelstromDamage, gameObject);
            Debug.Log($"Damaging boss {boss.name} with {maelstromDamage} damage.");
        }
        }
        

        // If the boss is in range, apply damage to the boss as well
        if (boss != null && Vector3.Distance(transform.position, boss.transform.position) <= maelstromRange)
        {
            Debug.Log("sssssssssssssssssssssssssssssssssssssss");
            boss.TakeDamage(maelstromDamage, gameObject);
            Debug.Log($"Damaging boss {boss.name} with {maelstromDamage} damage.");
        }

        yield return new WaitForSeconds(0.5f); // Ensure ability animation completes

        StartCoroutine(MaelstromCooldown());
        isAttacking = false;
        StopMovementAndResetState();
        
    }

    IEnumerator MaelstromCooldown()
    {
        yield return new WaitForSeconds(maelstromCooldown);
        isMaelstromCooldown = false;
    }

     bool CanPerformAction()
    {
        return !isAttacking && !isCooldown  && !hasAttacked ;
    }

    // Method to set the current level type (can be called from game manager)
    public void SetLevelType(bool isBossLevel)
    {
        isInBossLevel = isBossLevel;
    }

    void ClickToMove()
{
    if (!CanPerformAction()) return;

    RaycastHit hit;
    if (Physics.Raycast(Camera.main.ScreenPointToRay(Input.mousePosition), out hit, 100, clickableLayers))
    {
        boss = null; // Clear any targeted enemy
        agent.destination = hit.point;

        if (clickEffect != null)
        {
            Instantiate(clickEffect, hit.point + Vector3.up * 0.1f, Quaternion.identity);
        }

        // Start walking animation
        animator.SetBool(WALK, true);
        agent.isStopped = false;
    }
}


    // Method to select the enemy when right-clicking
void SelectEnemy()
{
    if (!CanPerformAction()) return;

    Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
    if (Physics.Raycast(ray, out RaycastHit hit, 100f, enemyLayer))
    {
        boss = hit.transform.GetComponent<BossController>();
        Debug.Log($"Enemy selected: {boss.name}");

        // Now trigger the attack when an enemy is selected
        if (boss != null && !isAttacking && !isCooldown)
        {
            float distanceToEnemy = Vector3.Distance(transform.position, boss.transform.position);
            if (distanceToEnemy <= attackRange)
            {
                AttackEnemy();
            }
            else
            {
                Debug.Log("Enemy is out of range for attack.");
            }
        }
    }
}

// Attack logic with cooldown and single attack functionality
void AttackEnemy()
{
    if (!CanPerformAction() || isAttacking ) return;

    StartCoroutine(PerformAttack());
}

// Attack execution coroutine
IEnumerator PerformAttack()
{
    if (isCooldown) yield break; // Exit if on cooldown

    isAttacking = true;
    agent.isStopped = true; // Stop movement during the attack
    animator.SetTrigger(BASH); // Trigger the attack animation

    yield return new WaitForSeconds(1.2f); // Wait for the animation to complete

    // Apply damage if the enemy is still valid
    //if (boss != null)
    //{
    //    boss.TakeDamage(bashDamage,gameObject);

    //    if (boss.currentHealth < 0)
    //    {
    //        Debug.Log($"{boss.name} is defeated.");
    //        boss = null;
    //    }
    //}

        Collider[] hitMinions = Physics.OverlapSphere(transform.position, attackRange, enemyLayer);
        foreach (var hitCollider in hitMinions)
        {
            if (hitCollider.CompareTag("enemy"))
            {
                DemonBehavior demon = hitCollider.GetComponent<DemonBehavior>();
                MinionMovement minion = hitCollider.GetComponent<MinionMovement>();
                if (minion != null)
                {
                    minion.TakeDamage(bashDamage); // Apply the same damage to the minion
                    Debug.Log($"Damaging minion {hitCollider.name} with {bashDamage} damage.");
                }
                if (demon != null)
                {
                    demon.TakeDamage(bashDamage); // Apply the same damage to the minion
                    Debug.Log($"Damaging minion {hitCollider.name} with {bashDamage} damage.");
                }

            }
        }

        // If the boss is in range, apply damage to the boss as well
        if (boss != null && Vector3.Distance(transform.position, boss.transform.position) <= attackRange)
        {
            boss.TakeDamage(bashDamage,gameObject);
            Debug.Log($"Damaging boss {boss.name} with {bashDamage} damage.");
        }


        // Start cooldown after the attack
        StartCoroutine(BashCooldown());
    StopMovementAndResetState();
}

// Cooldown logic
IEnumerator BashCooldown()
{
    isCooldown = true;
    Debug.Log("Bash cooldown started.");
    yield return new WaitForSeconds(bashCooldown); // Wait for cooldown time
    Debug.Log("Bash cooldown ended.");
    isCooldown = false;
}

// Reset the attack state after cooldown
void ResetState()
{
    Debug.Log("Resetting player state");
    isAttacking = false;
    isCharging = false;
    isCooldown = false;
    hasAttacked = false; // Ensure the attack flag is reset
    agent.isStopped = false;
}



    void FaceTarget()
    {
        if (selectedEnemy != null)
        {
            Vector3 direction = (selectedEnemy.transform.position - transform.position).normalized;
            Quaternion lookRotation = Quaternion.LookRotation(new Vector3(direction.x, 0, direction.z));
            transform.rotation = Quaternion.Slerp(transform.rotation, lookRotation, Time.deltaTime * lookRotationSpeed);
        }
        else if (agent.velocity.sqrMagnitude > 0.1f)
        {
            Vector3 direction = agent.velocity.normalized;
            Quaternion lookRotation = Quaternion.LookRotation(new Vector3(direction.x, 0, direction.z));
            transform.rotation = Quaternion.Slerp(transform.rotation, lookRotation, Time.deltaTime * lookRotationSpeed);
        }
    }

    void StopMovementAndResetState()
{
    agent.isStopped = true;
    agent.ResetPath(); // Clear current path to avoid lingering behavior
    animator.ResetTrigger(BASH);
    ResetState(); // Ensure state is reset
}


    void OnEnable() { input.Enable(); }
    void OnDisable() { input.Disable(); }


public void Damage(int damage)
    {
       
        if(!isShieldActive&&!invincible){
        animator.SetTrigger("Damage");
        Debug.Log("Inside Player Damage");
        hp-=damage;
        UpdateHealthBar();
        }
        

    }
    void Die(){
        
        agent.enabled = false;
    }

    // Call this method when player heals
    public void Heal(int healAmount)
    {
        animator.SetTrigger("Heal");
        hp += healAmount;
        hp = Mathf.Clamp(hp, 0, maxHp);  // Ensure HP does not exceed max HP
        UpdateHealthBar();
    }

    void Update()
{
    // Handle left-click movement (if any, but here we may not need it anymore)
    if (Input.GetMouseButtonDown(0))
    {
        ClickToMove();
    }

    // Handle right-click enemy selection
    if (Input.GetMouseButtonDown(1))
    {
        SelectEnemy();
    }

    // Handle facing target or movement
    if (!isAttacking)
    {
        FaceTarget();
    }
    if (hp >= maxHp)
        {
            hp = maxHp;
        }
    
    if (xp >= xpToNextLevel)
            {
                level++;
                xp -= xpToNextLevel;
                xpToNextLevel = 100 * level;
                maxHp = 100 * level;
                hp = maxHp;
                ap++;
            }

            if(hp<=0){
            hp=0;
            animator.SetTrigger("Die");
            hud.text = "";
            abilities.text = "";
            StartCoroutine(HandlePlayerDeath());
        }

    if (ap > 0)
            {
                // enable buttons of locked abilities
                if (!ability2Unlocked)
                {
                    unlockAbility2.gameObject.SetActive(true);
                } else
                {
                    unlockAbility2.gameObject.SetActive(false);
                }
                if (!ability3Unlocked)
                {
                    unlockAbility3.gameObject.SetActive(true);
                } else
                {
                    unlockAbility3.gameObject.SetActive(false);
                }
                if (!ability4Unlocked)
                {
                    unlockAbility4.gameObject.SetActive(true);
                } else
                {
                    unlockAbility4.gameObject.SetActive(false);
                }
            } else
            {
                // disable all buttons
                unlockAbility2.gameObject.SetActive(false);
                unlockAbility3.gameObject.SetActive(false);
                unlockAbility4.gameObject.SetActive(false);
            }

            if (Input.GetKeyDown(KeyCode.I))
            {
                invincible = !invincible;
            }

            if (Input.GetKeyDown(KeyCode.M))
            {
                ToggleSlowMotion();
            }

            if (Input.GetKeyDown(KeyCode.C))
            {
                bashCooldown = 0;
                shieldCooldown = 0;
                maelstromCooldown = 0;
                chargeCooldown = 0;
            }
    if (Input.GetKeyDown(KeyCode.D))
    {
        Damage(20);
    }
    if (Input.GetKeyDown(KeyCode.H))  // Press 'H' to heal
    {
        animator.SetTrigger("Drink");
        Heal(20);
    }
    if (Input.GetKeyDown(KeyCode.X))
    {
        xp += 100;
        Debug.Log("Xppppppp : "+ xp);
        Debug.Log("xpToNextLevel : "+ xpToNextLevel);
    }
    if (Input.GetKeyDown(KeyCode.U))
            {
                ability2Unlocked = true;
                ability3Unlocked = true;
                ability4Unlocked = true;
            }

    if (Input.GetKeyDown(KeyCode.A))
    {
        ap++;
    }
    if (Input.GetKeyDown(KeyCode.F))
{
    if (hp < maxHp && potions > 0) // Check if you have potions left
    {
        potions--;
        hp += (int)(maxHp * 0.5f); // Explicit cast to int
        if (hp > maxHp)
        {
            hp = maxHp; // Ensure hp doesn't exceed maxHp
        }
    }
}

    if(runeFragments>3){

    }

    // HUD
            hud.text =
            $"HP: {hp}/{maxHp}\n" +
            $"XP: {xp}/{xpToNextLevel}\n" +
            $"Level: {level}\n" +
            $"Ability Points: {ap}\n" +
            $"Healing Potions: {potions}/3\n" +
            $"Rune Fragments: {runeFragments}/3\n";

            abilities.text =
            $"Ability 1 (Arrow): Unlocked\n" +
            $"Cooldown: {bashCooldown}\n" +
            $"Ability 2 (Shield): {(ability2Unlocked?"Unlocked":"Locked")}\n" +
            $"Cooldown: {shieldCooldown}\n" +
            $"Ability 3 (Iron Maelstrom): {(ability3Unlocked?"Unlocked":"Locked")}\n" +
            $"Cooldown: {maelstromCooldown}\n" +
            $"Ability 4 (Charge): {(ability4Unlocked?"Unlocked":"Locked")}\n" +
            $"Cooldown: {chargeCooldown}\n";


        // Check if the player is near either door
        float distanceToLeftDoor = Vector3.Distance(transform.position, leftDoor.transform.position);
        float distanceToRightDoor = Vector3.Distance(transform.position, rightDoor.transform.position);

        if ((distanceToLeftDoor < proximityDistance || distanceToRightDoor < proximityDistance) && runeFragments>=3)
        {
            // Open the doors
            leftDoor.transform.position = Vector3.MoveTowards(leftDoor.transform.position, leftDoorOpenPosition, doorMoveSpeed * Time.deltaTime);
            rightDoor.transform.position = Vector3.MoveTowards(rightDoor.transform.position, rightDoorOpenPosition, doorMoveSpeed * Time.deltaTime);
        }

    // Update animations
    SetAnimations();
}
private IEnumerator HandlePlayerDeath()
{
    // Delay for 5 seconds
    yield return new WaitForSeconds(5f);
        GameOver.SetActive(true);


    // Add logic after delay (e.g., restart game, show menu, etc.)
    // Example: Reload the scene
}

void OnDrawGizmosSelected()
    {
        // Visualize the proximity range in the editor
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, proximityDistance);
    }

    private void OnTriggerEnter(Collider other)
    {
        // Check if the object colliding with the rune fragment is the player
        if (other.CompareTag("Potion")) 
        {
            if (potions < 3){
                potions++;
                Destroy(other.gameObject);
            }
        }
        if (other.CompareTag("Rune Fragment")) 
        {
                runeFragments++;
                Destroy(other.gameObject);
        }
    }

void ToggleSlowMotion()
    {
        if (isSlowMotion)
        {
            // If slow motion is currently active, reset the time scale to normal speed (1)
            Time.timeScale = 1f;
            isSlowMotion = false;
            Debug.Log("Slow Motion OFF");
        }
        else
        {
            // If slow motion is not active, set time scale to 0.5 for half speed
            Time.timeScale = 0.5f;
            isSlowMotion = true;
            Debug.Log("Slow Motion ON");
        }
    }

   void SetAnimations()
{
    if (isAttacking) return; // Do not change animations during an attack

    // If the player is standing still
    if (agent.velocity.sqrMagnitude <= 0.1f)
    {
        animator.SetBool(WALK, false); // Transition to Idle animation
    }
    else
    {
        animator.SetBool(WALK, true); // Transition to Walk animation
    }
}


}