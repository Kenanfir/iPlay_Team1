using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// public class EnemyController : MonoBehaviour
// {
//     [Header("Enemy Stats")]
//     public int maxHealth = 10;
//     private int currentHealth;

//     void Start()
//     {
//         currentHealth = maxHealth;
//     }

//     // This public method can be called by other scripts (like the Companion)
//     public void TakeDamage(int damageAmount)
//     {
//         currentHealth -= damageAmount;
//         Debug.Log(gameObject.name + " took " + damageAmount + " damage. Health is now " + currentHealth);

//         // Optional: Add a hurt flash effect here if you want

//         if (currentHealth <= 0)
//         {
//             Die();
//         }
//     }

//     private void Die()
//     {
//         Debug.Log(gameObject.name + " has been defeated.");
//         // Destroy the enemy object
//         Destroy(gameObject);
//     }
// }


public class EnemyController : MonoBehaviour
{
    private enum EnemyState { Chasing, Repositioning, Dead }
    private EnemyState currentState;

    [Header("Enemy Stats")]
    public int maxHealth = 10;
    public float moveSpeed = 1.5f;
    [Tooltip("How far the enemy can 'see' to find a target.")]
    public float sightRadius = 10f;
    public int attackDamage = 1;
    private int currentHealth;
    private bool isDead = false;

    [Header("Attack Timings")]
    [Tooltip("How often the enemy can attack, in seconds.")]
    public float attackCooldown = 1.5f;
    [Tooltip("How fast the enemy moves backward after an attack.")]
    public float repositionSpeed = 3f;
    [Tooltip("How long the enemy moves backward after an attack.")]
    public float repositionDuration = 0.2f;

    // AI State
    private Transform target;
    private float lastAttackTime;

    // Components
    private Rigidbody2D rb;
    private SpriteRenderer spriteRenderer;
    private Color originalColor;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    void Start()
    {
        currentHealth = maxHealth;
        currentState = EnemyState.Chasing;
        if (spriteRenderer != null)
        {
            originalColor = spriteRenderer.color;
        }
    }

    void Update()
    {
        if (isDead) return;

        switch (currentState)
        {
            case EnemyState.Chasing:
                HandleChasingState();
                break;
            case EnemyState.Repositioning:
                HandleRepositioningState();
                break;
        }
    }

    // This public method can be called by other scripts
    public void TakeDamage(int damageAmount)
    {
        if (isDead) return;

        currentHealth -= damageAmount;
        Debug.Log(gameObject.name + " took " + damageAmount + " damage. Health is now " + currentHealth);

        StartCoroutine(HurtFlashCoroutine());

        if (currentHealth <= 0)
        {
            Die();
        }
    }

    private void Die()
    {
        isDead = true;
        currentState = EnemyState.Dead;
        Debug.Log(gameObject.name + " has been defeated.");

        // Disable physics and stop movement
        GetComponent<Collider2D>().enabled = false;
        rb.velocity = Vector2.zero;
        
        // Optional: Play a death animation or particle effect here
        
        // Destroy the enemy object after a short delay
        Destroy(gameObject, 1f);
    }

    private IEnumerator HurtFlashCoroutine()
    {
        spriteRenderer.color = Color.red;
        yield return new WaitForSeconds(0.1f);
        if (!isDead)
        {
            spriteRenderer.color = originalColor;
        }
    }

    private void HandleChasingState()
    {
        if (target == null)
        {
            FindNearestTarget();
        }

        if (target != null)
        {
            MoveTowardsTarget();
        }
        else
        {
            rb.velocity = Vector2.zero;
        }
    }

    private void HandleRepositioningState()
    {
        if (Time.time < lastAttackTime + repositionDuration)
        {
            // Keep moving backward
        }
        else
        {
            rb.velocity = Vector2.zero;
        }

        if (Time.time >= lastAttackTime + attackCooldown)
        {
            currentState = EnemyState.Chasing;
        }
    }

    private void FindNearestTarget()
    {
        Transform closestTarget = null;
        float closestDistance = Mathf.Infinity;

        int layerMask = LayerMask.GetMask("Player", "Companion");
        Collider2D[] potentialTargets = Physics2D.OverlapCircleAll(transform.position, sightRadius, layerMask);

        foreach (Collider2D potentialTarget in potentialTargets)
        {
            float distance = Vector2.Distance(transform.position, potentialTarget.transform.position);
            if (distance < closestDistance)
            {
                closestDistance = distance;
                closestTarget = potentialTarget.transform;
            }
        }
        target = closestTarget;
    }

    private void MoveTowardsTarget()
    {
        if (target == null)
        {
            rb.velocity = Vector2.zero;
            return;
        }
        Vector2 direction = (target.position - transform.position).normalized;
        rb.velocity = direction * moveSpeed;
    }

    void OnCollisionEnter2D(Collision2D collision)
    {
        if (currentState != EnemyState.Chasing || isDead)
        {
            return;
        }

        if (collision.transform == target)
        {
            lastAttackTime = Time.time;
            currentState = EnemyState.Repositioning;

            Vector2 knockbackDir = (transform.position - collision.transform.position).normalized;
            rb.velocity = knockbackDir * repositionSpeed;

            PlayerController player = collision.gameObject.GetComponent<PlayerController>();
            if (player != null)
            {
                player.TakeDamage(attackDamage);
                return;
            }

            CompanionController companion = collision.gameObject.GetComponent<CompanionController>();
            if (companion != null)
            {
                companion.TakeDamage(attackDamage);
            }
        }
    }
}

