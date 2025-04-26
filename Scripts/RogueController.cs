using UnityEngine;
using UnityEngine.AI;
using TMPro;
using System.Collections;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using System.Net.Sockets;
//using UnityEditor.Playables;

public class RogueController : MonoBehaviour
{
    private NavMeshAgent agent;
    private Animator animator;

    public GameObject GameOver;

    public int hp;
    public int maxHp;
    public int xp;
    public int level;
    public int xpToNextLevel;
    public int ap;
    public int runeFragments;
    public int potions;

    public bool ability2Unlocked;
    public bool ability3Unlocked;
    public bool ability4Unlocked;

    private Camera mainCamera;

    public TextMeshProUGUI hud;
    public TextMeshProUGUI abilities;
    public TextMeshProUGUI hint;

    public Button unlockAbility2;
    public Button unlockAbility3;
    public Button unlockAbility4;

    public int ability1Cooldown;
    public int ability2Cooldown;
    public int ability3Cooldown;
    public int ability4Cooldown;
    public bool allAbilitiesCooldown;

    public GameObject arrowPrefab;
    public Transform arrowAttackPosition;

    public bool alive;

    public bool selecting;

    public int ability;

    [SerializeField] LayerMask clickableLayers;

    public GameObject smokeBombPrefab; // Reference to the Smoke Bomb prefab
    public float smokeBombRadius = 1.5f; // Radius of the smoke bomb effect
    public GameObject arrowRainPrefab;
    public float arrowRainRadius = 1.5f;

    public bool invincible = false;

    public GameObject leftDoor;      // Reference to the left door
    public GameObject rightDoor;     // Reference to the right door
    public float proximityDistance = 5f; // Distance within which doors will open
    public float doorMoveSpeed = 2f; // Speed of the door movement
    public float doorOpenOffset = 3f; // How far the doors will move when opening
    
    private Vector3 leftDoorClosedPosition;  // Initial position of the left door
    private Vector3 rightDoorClosedPosition; // Initial position of the right door
    private Vector3 leftDoorOpenPosition;    // Target open position for the left door
    private Vector3 rightDoorOpenPosition;   // Target open position for the right door

    //public BossController boss;

    void Start()
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

        mainCamera = Camera.main;

        ability1Cooldown = 0;
        ability2Cooldown = 0;
        ability3Cooldown = 0;
        ability4Cooldown = 0;
        allAbilitiesCooldown = true;

        alive = true;

        selecting = false;

        ability = 0;

        // Store initial positions
        leftDoorClosedPosition = leftDoor.transform.position;
        rightDoorClosedPosition = rightDoor.transform.position;

        // Calculate target open positions
        leftDoorOpenPosition = leftDoorClosedPosition + new Vector3(doorOpenOffset, 0, 0); // Move left
        rightDoorOpenPosition = rightDoorClosedPosition + new Vector3(-doorOpenOffset, 0, 0); // Move right

        

        StartCoroutine(Wait1Sec());
    }



    void Update()
    {
        // Check if the player is near either door
        float distanceToLeftDoor = Vector3.Distance(transform.position, leftDoor.transform.position);
        float distanceToRightDoor = Vector3.Distance(transform.position, rightDoor.transform.position);

        if ((distanceToLeftDoor < proximityDistance || distanceToRightDoor < proximityDistance) && runeFragments>=3)
        {
            // Open the doors
            leftDoor.transform.position = Vector3.MoveTowards(leftDoor.transform.position, leftDoorOpenPosition, doorMoveSpeed * Time.deltaTime);
            rightDoor.transform.position = Vector3.MoveTowards(rightDoor.transform.position, rightDoorOpenPosition, doorMoveSpeed * Time.deltaTime);
        }

        // HP
        if (hp <= 0)
{
    hp = 0;
    alive = false;
    animator.SetBool("Die", true);
    hud.text = "";
    abilities.text = "";
    hint.text = "";
    
    StartCoroutine(HandlePlayerDeath());
}

        if (hp >= maxHp)
        {
            hp = maxHp;
        }

        if (alive)
        {
            if (Input.GetMouseButtonDown(0) && allAbilitiesCooldown && !selecting)
            {
                 RaycastHit hit;
    if (Physics.Raycast(Camera.main.ScreenPointToRay(Input.mousePosition), out hit, 100, clickableLayers))
    {
        agent.destination = hit.point;


        // Start walking animation
        animator.SetBool("moving", true);
        agent.isStopped = false;
    }
            }
          // Update Speed parameter based on agent velocity
            animator.SetFloat("Speed", agent.velocity.magnitude);

            // Leveling
            if (xp >= xpToNextLevel)
            {
                level++;
                xp -= xpToNextLevel;
                xpToNextLevel = 100 * level;
                maxHp = 100 * level;
                hp = maxHp;
                ap++;
            }

            // Use potion
            if (Input.GetKeyDown(KeyCode.F))
            {
                if (potions > 0)
                {
                    animator.SetTrigger("Potion");
                    potions--;
                    if (hp < maxHp)
                    {
                        hp += (maxHp / 2);
                    }
                }
            }

            // Pause and Resume
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                // Pause and Resume
            }

            // Attack
            if (Input.GetMouseButtonDown(1) && ability1Cooldown == 0 && allAbilitiesCooldown && !selecting) // Right mouse button
            {
                animator.SetTrigger("Arrow");
                FireArrow();
                // Fire arrow audio
                ability1Cooldown = 1;
                StartCoroutine(AllAbilitiesCooldown());
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
            $"Cooldown: {ability1Cooldown}\n" +
            $"Ability 2 (Smoke Bomb): {(ability2Unlocked?"Unlocked":"Locked")}\n" +
            $"Cooldown: {ability2Cooldown}\n" +
            $"Ability 3 (Dash): {(ability3Unlocked?"Unlocked":"Locked")}\n" +
            $"Cooldown: {ability3Cooldown}\n" +
            $"Ability 4 (Shower of Arrows): {(ability4Unlocked?"Unlocked":"Locked")}\n" +
            $"Cooldown: {ability4Cooldown}\n";

            if (selecting)
            {
                hint.text = "Please select a position on the screen";
            }
            else
            {
                hint.text = "";
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

            // Smoke Bomb
            if (Input.GetKeyDown(KeyCode.W) && ability2Cooldown == 0 && ability2Unlocked)
            {
                animator.SetTrigger("Throw");
                DropSmokeBomb();
                // Smoke bomb audio
                ability2Cooldown = 10;
                StartCoroutine(AllAbilitiesCooldown());
            }

            // Dash
            if ((Input.GetKeyDown(KeyCode.Q) && allAbilitiesCooldown && ability3Cooldown == 0 && !selecting && ability3Unlocked) || (selecting && ability == 3))
            {
                selecting = true;
                ability = 3;
                if(Input.GetMouseButtonDown(1) && !EventSystem.current.IsPointerOverGameObject())
                {
                    // Ray ray = mainCamera.ScreenPointToRay(Input.mousePosition);
                    // if (Physics.Raycast(ray, out RaycastHit hit))
                    // {
                    //     // Dash audio
                    //     StartCoroutine(DashToPosition(hit.point));
                    // }
                    RaycastHit hit;
    if (Physics.Raycast(Camera.main.ScreenPointToRay(Input.mousePosition), out hit, 100, clickableLayers))
    {
        StartCoroutine(DashToPosition(hit.point));
    }

                    selecting = false;
                    ability = 0;
                    ability3Cooldown = 5;
                    StartCoroutine(AllAbilitiesCooldown());
                }
            }

            // Shower of Arrows
            if ((Input.GetKeyDown(KeyCode.E) && allAbilitiesCooldown && ability4Cooldown == 0 && !selecting && ability4Unlocked) || (selecting && ability == 4))
            {
                selecting = true;
                ability = 4;
                if (Input.GetMouseButtonDown(1) && !EventSystem.current.IsPointerOverGameObject())
                {
                    animator.SetTrigger("Arrow");
                    Ray ray = mainCamera.ScreenPointToRay(Input.mousePosition);
                    if (Physics.Raycast(ray, out RaycastHit hit)) {
                        // Ultimate audio
                        ShowerOfArrows(hit.point);
                    }
                    selecting = false;
                    ability = 0;
                    ability4Cooldown = 10;
                    StartCoroutine(AllAbilitiesCooldown());
                }
            }

            // Cheats

            if (Input.GetKeyDown(KeyCode.H))
            {
                hp += 20;
            }

            if (Input.GetKeyDown(KeyCode.D))
            {
                Damage(20);
            }

            if (Input.GetKeyDown(KeyCode.I))
            {
                invincible = !invincible;
            }

            if (Input.GetKeyDown(KeyCode.M))
            {
                // Slow motion
            }

            if (Input.GetKeyDown(KeyCode.C))
            {
                ability1Cooldown = 0;
                ability2Cooldown = 0;
                ability3Cooldown = 0;
                ability4Cooldown = 0;
                allAbilitiesCooldown = true;
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

            if (Input.GetKeyDown(KeyCode.X))
            {
                xp += 100;
            }

        }
        

    }
    
    private IEnumerator HandlePlayerDeath()
{
    // Delay for 5 seconds
    yield return new WaitForSeconds(5f);
        GameOver.SetActive(true);


    // Add logic after delay (e.g., restart game, show menu, etc.)
    // Example: Reload the scene
}


    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Potion")){
            if (potions < 3)
            {
                potions++;
                other.gameObject.SetActive(false);
                // Pick up item audio
            }
        }
        if (other.CompareTag("Rune Fragment"))
        {
            runeFragments++;
            other.gameObject.SetActive(false);
            // Pick up item audio
        }
    }

    IEnumerator Wait1Sec()
    {
        yield return new WaitForSeconds(1);
        ability1Cooldown--;
        ability2Cooldown--;
        ability3Cooldown--;
        ability4Cooldown--;
        if (ability1Cooldown < 0)
        {
            ability1Cooldown = 0;
        }
        if (ability2Cooldown < 0)
        {
            ability2Cooldown = 0;
        }
        if (ability3Cooldown < 0)
        {
            ability3Cooldown = 0;
        }
        if (ability4Cooldown < 0)
        {
            ability4Cooldown = 0;
        }
        StartCoroutine(Wait1Sec());
    }

    IEnumerator AllAbilitiesCooldown()
    {
        allAbilitiesCooldown = false;
        yield return new WaitForSeconds(1f); //
        allAbilitiesCooldown = true;
    }

    void FireArrow()
    {
        Ray ray = mainCamera.ScreenPointToRay(Input.mousePosition);
        if (Physics.Raycast(ray, out RaycastHit hit))
        {
            Vector3 direction = (hit.point - arrowAttackPosition.position).normalized;

            GameObject arrow = Instantiate(arrowPrefab, arrowAttackPosition.position, Quaternion.LookRotation(direction));
            
            
            //boss.TakeDamage(5, gameObject);
        }
    }

    public void UnlockAbility(int ability)
    {
        Debug.Log("Before" + "\n" +
        "AP: " + ap + "\n" +
        "Ability 2: " + ability2Unlocked + "\n" +
        "Ability 3: " + ability3Unlocked + "\n" +
        "Ability 4: " + ability4Unlocked);
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
        Debug.Log("After" + "\n" +
        "AP: " + ap + "\n" +
        "Ability 2: " + ability2Unlocked + "\n" +
        "Ability 3: " + ability3Unlocked + "\n" +
        "Ability 4: " + ability4Unlocked);
    }

    IEnumerator DashToPosition(Vector3 targetPosition)
    {
        Vector3 startPosition = transform.position;
        float dashSpeed = agent.speed * 2f; // Double the speed
        float dashDuration = Vector3.Distance(startPosition, targetPosition) / dashSpeed;

        agent.speed = dashSpeed; // Temporarily set the agent's speed to double
        agent.SetDestination(targetPosition);

        // Ensure the running animation is active
        animator.SetFloat("Speed", dashSpeed);
        animator.SetBool("Running", true);

        // Wait until the dash ends
        float elapsedTime = 0f;
        while (elapsedTime < dashDuration)
        {
            elapsedTime += Time.deltaTime;

            // Stop if the player is close enough to the target position
            if (Vector3.Distance(transform.position, targetPosition) < 0.5f)
            {
                break;
            }

            yield return null;
        }

        // Reset agent speed and animation
        agent.speed = agent.speed / 2f; // Back to normal
        animator.SetFloat("Speed", 0); // Stop running animation
        animator.SetBool("Running", false);
        agent.ResetPath(); // Stop any additional movement
    }

    void DropSmokeBomb()
    {
        // Instantiate the smoke bomb at the player's position
        GameObject smokeBomb = Instantiate(smokeBombPrefab, transform.position, Quaternion.identity);

        // Destroy the smoke bomb after 3 seconds (adjust duration as needed)
        Destroy(smokeBomb, 5f);

        // Detect enemies in the radius
        Collider[] hitColliders = Physics.OverlapSphere(transform.position, smokeBombRadius);

        foreach (Collider hitCollider in hitColliders)
        {
            // Check tags
            if (hitCollider.CompareTag("enemy"))
            {
                // Call the Stunned function on the enemy script
                var enemy = hitCollider.GetComponent<EnemyController1>();
                if (enemy != null)
                {
                    enemy.Stunned();
                }
            }
        }
    }

    void ShowerOfArrows(Vector3 targetPosition)
    {
        // Adjust the spawn position to be higher (e.g., 10 units above the target)
        Vector3 spawnPosition = new Vector3(targetPosition.x, targetPosition.y + 5f, targetPosition.z);

        // Instantiate visual effect
        GameObject arrowRain = Instantiate(arrowRainPrefab, spawnPosition, Quaternion.identity);

        // Scale visual radius to match functional radius
        ParticleSystem ps = arrowRain.GetComponent<ParticleSystem>();
        var shape = ps.shape;
        //shape.radius = arrowRainRadius;
        shape.radius = 15;

        // Destroy visual effect after 3 seconds
        Destroy(arrowRain, 3f);

        // Detect enemies in the radius
        Collider[] hitColliders = Physics.OverlapSphere(targetPosition, arrowRainRadius);

        foreach (Collider hitCollider in hitColliders)
        {
            if (hitCollider.CompareTag("enemy"))
            {
                var boss = hitCollider.GetComponent<BossController>();
                if (boss != null)
                {
                    boss.TakeDamage(10,gameObject); // Fixed damage or use scaled damage
                    //boss.SlowDown();    // Slow down enemy
                }

                var minon = hitCollider.GetComponent<MinionMovement>();
                var demon = hitCollider.GetComponent<DemonBehavior>();
                if (minon != null)
                {
                    minon.TakeDamage(10); // Fixed damage or use scaled damage
                    Debug.Log("Xpppppppppppp: "+xp); 

                    //boss.SlowDown();    // Slow down enemy
                }
                if (demon != null)
                {
                    demon.TakeDamage(10); // Fixed damage or use scaled damage
                    Debug.Log("Xpppppppppppp: "+xp); 
                    //boss.SlowDown();    // Slow down enemy
                }
            }
        }
    }

    public void Damage(int damage)
    {
        if (!invincible)
        {
            animator.SetTrigger("Damage");
            Debug.Log("This was rogue health" + hp);
            hp-=damage;
            Debug.Log("This is now rogue health"+hp);
        }
    }


}
