using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(LineRenderer))] // Ensures a Line Renderer is always present
[RequireComponent(typeof(AudioSource))] // Ensures an Audio Source is always present
public class CompanionController : MonoBehaviour
{
    public enum CompanionState { Idle, Chasing, Attacking, Cooldown, Dead }

    [Header("Companion Stats")]
    public int maxHealth = 6;
    public float moveSpeed = 2f;
    public float attackRange = 0.5f;
    public int attackDamage = 1;
    [Tooltip("How long the companion is invincible after being hit.")]
    public float invincibilityDuration = 1.5f;
    private int currentHealth;
    private float lastDamageTime;

    [Header("UI")]
    [Tooltip("The UI Slider for the companion's health bar.")]
    public Slider healthBar;

    [Header("Attack Timings")]
    public float attackInterval = 0.2f;
    public float attackCooldown = 0.5f;

    [Header("SFX")]
    public AudioClip deathSound;
    public AudioClip attackSound;
    public AudioClip hurtSound;

    // State Machine
    private CompanionState currentState;
    private bool isLitByPlayer;
    private Transform targetEnemy;
    private bool isLockedOnToTarget = false;

    // Attacking
    private int slashesLeft;
    private float lastActionTime;

    // Components & References
    private Rigidbody2D rb;
    private Animator animator;
    private LightDetector lightCone;
    private LineRenderer lineRenderer;
    private SpriteRenderer[] spriteRenderers;
    private Color[] originalColors;
    private AudioSource audioSource;
    private bool isDead = false;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponentInChildren<Animator>();
        lineRenderer = GetComponent<LineRenderer>();
        spriteRenderers = GetComponentsInChildren<SpriteRenderer>();
        audioSource = GetComponent<AudioSource>();
    }

    void Start()
    {
        currentHealth = maxHealth;
        currentState = CompanionState.Idle;
        slashesLeft = 2;
        
        originalColors = new Color[spriteRenderers.Length];
        for (int i = 0; i < spriteRenderers.Length; i++)
        {
            originalColors[i] = spriteRenderers[i].color;
        }

        lineRenderer.positionCount = 0;

        if (healthBar != null)
        {
            healthBar.maxValue = maxHealth;
            healthBar.value = currentHealth;
        }
    }

    void Update()
    {
        if (isDead) return;

        switch (currentState)
        {
            case CompanionState.Idle: HandleIdleState(); break;
            case CompanionState.Chasing: HandleChasingState(); break;
            case CompanionState.Attacking: HandleAttackingState(); break;
            case CompanionState.Cooldown: HandleCooldownState(); break;
        }
    }
    
    void OnCollisionEnter2D(Collision2D collision)
    {
        if (isDead) return;
        if (collision.gameObject.CompareTag("Enemy"))
        {
            TakeDamage(1);
        }
    }

    public void TakeDamage(int damageAmount)
    {
        if (isDead) return;
        if (Time.time < lastDamageTime + invincibilityDuration)
        {
            return;
        }

        lastDamageTime = Time.time;
        currentHealth -= damageAmount;

        if (healthBar != null)
        {
            healthBar.value = currentHealth;
        }
        
        if (!isLitByPlayer)
        {
            if(animator != null) animator.SetTrigger("isDamaged");
            if (hurtSound != null && audioSource != null)
            {
                audioSource.PlayOneShot(hurtSound);
            }
        }
        
        StartCoroutine(HurtFlashCoroutine());

        if (currentHealth <= 0)
        {
            Die();
        }
    }

    private void HandleIdleState()
    {
        rb.velocity = Vector2.zero;
        if(animator != null) animator.SetBool("isChasing", false);
        lineRenderer.positionCount = 0;

        if (isLitByPlayer && !isLockedOnToTarget)
        {
            if (lightCone == null)
            {
                lightCone = FindObjectOfType<LightDetector>();
                if (lightCone == null) return;
            }
            FindClosestEnemyInLight();
            if (targetEnemy != null)
            {
                isLockedOnToTarget = true;
                currentState = CompanionState.Chasing;
            }
        }
    }

    private void HandleChasingState()
    {
        if (targetEnemy == null)
        {
            currentState = CompanionState.Idle;
            isLockedOnToTarget = false; // Unlock since the target is gone
            return;
        }

        if(animator != null) animator.SetBool("isChasing", true);

        Vector2 direction = (targetEnemy.position - transform.position).normalized;
        rb.velocity = direction * moveSpeed;

        lineRenderer.positionCount = 2;
        lineRenderer.SetPosition(0, transform.position);
        lineRenderer.SetPosition(1, targetEnemy.position);

        if (Vector2.Distance(transform.position, targetEnemy.position) <= attackRange)
        {
            slashesLeft = 2; // --- NEW: Reset attack combo before attacking ---
            currentState = CompanionState.Attacking;
            rb.velocity = Vector2.zero;
        }
    }

    private void HandleAttackingState()
    {
        lineRenderer.positionCount = 0;
        if (targetEnemy == null) // Check if enemy died mid-combo
        {
            currentState = CompanionState.Idle;
            isLockedOnToTarget = false;
            return;
        }
        
        if (Vector2.Distance(transform.position, targetEnemy.position) > attackRange)
        {
            currentState = CompanionState.Chasing;
            return;
        }

        if (Time.time > lastActionTime + attackInterval)
        {
            lastActionTime = Time.time;
            slashesLeft--;
            if(animator != null) animator.SetTrigger("isAttacking");

            if (attackSound != null && audioSource != null)
            {
                audioSource.PlayOneShot(attackSound);
            }

            EnemyController enemy = targetEnemy.GetComponent<EnemyController>();
            if (enemy != null)
            {
                enemy.TakeDamage(attackDamage);
            }

            if (slashesLeft <= 0)
            {
                currentState = CompanionState.Cooldown;
            }
        }
    }

    // --- UPDATED: This state now re-engages the enemy ---
    private void HandleCooldownState()
    {
        // If the enemy dies during our cooldown, go back to idle.
        if (targetEnemy == null)
        {
            isLockedOnToTarget = false;
            currentState = CompanionState.Idle;
            return;
        }

        // After the cooldown, immediately go back to chasing the same target.
        if (Time.time > lastActionTime + attackCooldown)
        {
            currentState = CompanionState.Chasing;
        }
    }

    private void Die()
    {
        isDead = true;
        currentState = CompanionState.Dead;
        Debug.Log("Companion has died.");

        if (deathSound != null && audioSource != null)
        {
            audioSource.PlayOneShot(deathSound);
        }

        rb.velocity = Vector2.zero;
        GetComponent<Collider2D>().enabled = false;
        lineRenderer.positionCount = 0;

        if (healthBar != null)
        {
            healthBar.gameObject.SetActive(false);
        }

        if(animator != null) animator.SetTrigger("isDead");

        StartCoroutine(DeathSequenceCoroutine());
    }

    private IEnumerator HurtFlashCoroutine()
    {
        for (int i = 0; i < spriteRenderers.Length; i++)
        {
            spriteRenderers[i].color = Color.red;
        }
        yield return new WaitForSeconds(1f);
        if (!isDead)
        {
            for (int i = 0; i < spriteRenderers.Length; i++)
            {
                spriteRenderers[i].color = originalColors[i];
            }
        }
    }
    
    private IEnumerator DeathSequenceCoroutine()
    {
        yield return new WaitForSeconds(1f);

        float fadeDuration = 1f;
        float elapsed = 0f;

        while (elapsed < fadeDuration)
        {
            elapsed += Time.deltaTime;
            float alpha = Mathf.Lerp(1f, 0f, elapsed / fadeDuration);
            for (int i = 0; i < spriteRenderers.Length; i++)
            {
                spriteRenderers[i].color = new Color(spriteRenderers[i].color.r, spriteRenderers[i].color.g, spriteRenderers[i].color.b, alpha);
            }
            yield return null;
        }

        Destroy(gameObject);
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (isDead) return;
        if (other.CompareTag("PlayerLight"))
        {
            isLitByPlayer = true;
        }
    }

    void OnTriggerExit2D(Collider2D other)
    {
        if (isDead) return;
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


    

   
    