using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CompanionController : MonoBehaviour
{
    // Define the states the companion can be in
    public enum CompanionState
    {
        Idle,
        Chasing,
        Attacking,
        Cooldown
    }

    [Header("Companion Stats")]
    public int maxHealth = 6;
    public float moveSpeed = 2f;
    public float attackRange = 0.5f; // Keep this value small

    [Header("Attack Timings")]
    [Tooltip("Time between the two slashes in a combo.")]
    public float attackInterval = 0.2f;
    [Tooltip("Time after a combo before the companion can attack again.")]
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
    private LightDetector lightCone;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponentInChildren<Animator>();
    }

    void Start()
    {
        currentState = CompanionState.Idle;
        slashesLeft = 2;
    }

    void Update()
    {
        switch (currentState)
        {
            case CompanionState.Idle:
                HandleIdleState();
                break;
            case CompanionState.Chasing:
                HandleChasingState();
                break;
            case CompanionState.Attacking:
                HandleAttackingState();
                break;
            case CompanionState.Cooldown:
                HandleCooldownState();
                break;
        }
    }

    private void HandleIdleState()
    {
        rb.velocity = Vector2.zero;
        if(animator != null) animator.SetBool("isChasing", false);

        if (isLitByPlayer)
        {
            if (lightCone == null)
            {
                lightCone = FindObjectOfType<LightDetector>();
                if (lightCone == null)
                {
                    return; 
                }
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
            rb.velocity = Vector2.zero;
            return;
        }

        if(animator != null) animator.SetBool("isChasing", true);

        Vector2 direction = (targetEnemy.position - transform.position).normalized;
        rb.velocity = direction * moveSpeed;
        // Debug.Log("Chasing enemy");

        if (Vector2.Distance(transform.position, targetEnemy.position) <= attackRange)
        {
            currentState = CompanionState.Attacking;
            rb.velocity = Vector2.zero;
        }
    }

    private void HandleAttackingState()
    {
        if (targetEnemy == null || Vector2.Distance(transform.position, targetEnemy.position) > attackRange)
        {
            currentState = CompanionState.Chasing;
            return;
        }

        if (Time.time > lastActionTime + attackInterval)
        {
            lastActionTime = Time.time;
            slashesLeft--;

            // --- FIX: Trigger the attack animation ---
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
}
