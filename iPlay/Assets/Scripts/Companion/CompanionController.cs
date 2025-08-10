using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(LineRenderer))]
public class CompanionController : MonoBehaviour
{
    public enum CompanionState { Idle, Chasing, Attacking, Cooldown }

    [Header("Companion Stats")]
    public int maxHealth = 6;
    public float moveSpeed = 2f;
    public float attackRange = 0.5f;
    private int currentHealth;

    [Header("Attack Timings")]
    public float attackInterval = 0.2f;
    public float attackCooldown = 0.5f;

    // State Machine
    private CompanionState currentState;
    private bool isLitByPlayer;
    private Transform targetEnemy;

    // Attacking
    private int slashesLeft;
    private float lastActionTime;

    // Components & References
    private Rigidbody2D rb;
    private Animator animator;
    private SpriteRenderer spriteRenderer;
    private Color originalColor;
    private LightDetector lightCone;
    private LineRenderer lineRenderer; // Check path to enemy

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponentInChildren<Animator>();
        spriteRenderer = GetComponentInChildren<SpriteRenderer>();
        lineRenderer = GetComponent<LineRenderer>();
    }

    void Start()
    {
        currentHealth = maxHealth;
        currentState = CompanionState.Idle;
        slashesLeft = 2;
        if (spriteRenderer != null)
        {
            originalColor = spriteRenderer.color;
        }
        lineRenderer.positionCount = 0;
    }

    void Update()
    {
        switch (currentState)
        {
            case CompanionState.Idle: HandleIdleState(); break;
            case CompanionState.Chasing: HandleChasingState(); break;
            case CompanionState.Attacking: HandleAttackingState(); break;
            case CompanionState.Cooldown: HandleCooldownState(); break;
        }
    }

    private void HandleIdleState()
    {
        rb.velocity = Vector2.zero;
        if(animator != null) animator.SetBool("isChasing", false);
        lineRenderer.positionCount = 0; 

        if (isLitByPlayer)
        {
            if (lightCone == null)
            {
                lightCone = FindObjectOfType<LightDetector>();
                if (lightCone == null) return;
            }
            FindClosestEnemyInLight();
            if (targetEnemy != null)
            {
                currentState = CompanionState.Chasing;
            }
        }
    }

    private void HandleChasingState()
    {
        if (targetEnemy == null || lightCone == null || !lightCone.enemiesInLight.Contains(targetEnemy))
        {
            currentState = CompanionState.Idle;
            targetEnemy = null;
            return;
        }

        if(animator != null) animator.SetBool("isChasing", true);

        Vector2 direction = (targetEnemy.position - transform.position).normalized;
        rb.velocity = direction * moveSpeed;

        // --- NEW: Draw the path line ---
        lineRenderer.positionCount = 2;
        lineRenderer.SetPosition(0, transform.position);
        lineRenderer.SetPosition(1, targetEnemy.position);

        if (Vector2.Distance(transform.position, targetEnemy.position) <= attackRange)
        {
            currentState = CompanionState.Attacking;
            rb.velocity = Vector2.zero;
        }
    }

    private void HandleAttackingState()
    {
        lineRenderer.positionCount = 0;
        if (targetEnemy == null || Vector2.Distance(transform.position, targetEnemy.position) > attackRange)
        {
            currentState = CompanionState.Chasing;
            return;
        }

        if (Time.time > lastActionTime + attackInterval)
        {
            lastActionTime = Time.time;
            slashesLeft--;
            if(animator != null) animator.SetTrigger("isAttacking");

            if (slashesLeft <= 0)
            {
                currentState = CompanionState.Cooldown;
            }
        }
    }

    private void HandleCooldownState()
    {
        if (Time.time > lastActionTime + attackCooldown)
        {
            slashesLeft = 2;
            currentState = CompanionState.Idle;
        }
    }

    // --- HELPER & DETECTION METHODS ---
    // (The rest of the script remains the same)

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("PlayerLight"))
        {
            isLitByPlayer = true;
        }
    }

    void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("PlayerLight"))
        {
            isLitByPlayer = false;
        }
    }
    
    private void FindClosestEnemyInLight()
    {
        targetEnemy = null;
        float closestDistance = Mathf.Infinity;

        if (lightCone != null && lightCone.enemiesInLight.Count > 0)
        {
            foreach (Transform enemy in lightCone.enemiesInLight)
            {
                if(enemy != null)
                {
                    float distance = Vector2.Distance(transform.position, enemy.position);
                    if (distance < closestDistance)
                    {
                        closestDistance = distance;
                        targetEnemy = enemy;
                    }
                }
            }
        }
    }

    public void TakeDamage(int damageAmount)
    {
        currentHealth -= damageAmount;
        Debug.Log("Companion took damage! Health is now " + currentHealth);

        if (animator != null)
        {
            animator.SetTrigger("isDamaged");
        }

        StartCoroutine(HurtFlashCoroutine());

        if (currentHealth <= 0)
        {
            // Death Logic
            
        }
    }

    private IEnumerator HurtFlashCoroutine()
    {
        if (spriteRenderer != null)
        {
            spriteRenderer.color = Color.red;
            yield return new WaitForSeconds(1f);
            spriteRenderer.color = originalColor;
        }
    }
}

    

   
    