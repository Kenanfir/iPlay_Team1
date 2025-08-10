using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [Header("Player Stats")]
    public int maxHealth = 6;
    public float invincibilityDuration = 2f;
    private int currentHealth;
    private float lastDamageTime;

    [Header("SFX")]
    [Tooltip("The sound effect to play when the player gets hit.")]
    public AudioClip damageSound;

    [Header("References")]
    public CameraController cameraController;

    [Header("Player Visuals")]
    public GameObject playerSpritePrefab;
    public GameObject flashlightPrefab;

    [Header("Movement")]
    public float moveSpeed = 5f;

    [Header("Aiming")]
    public float aimDistance = 0.5f;
    public float aimSpeed = 10f;
    [Tooltip("An additional offset to fine-tune the flashlight's position.")]
    public Vector2 flashlightOffset; // --- NEW: The offset variable ---

    // Private components
    private Rigidbody2D rb;
    private GameObject playerSpriteInstance;
    private GameObject flashlightInstance;
    private Transform aimTransform;
    private Vector2 moveDirection;
    private Vector2 aimDirection;
    private Animator animator;
    private AudioSource audioSource;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        rb.gravityScale = 0;
        rb.drag = 10f;
        rb.freezeRotation = true;
        audioSource = GetComponent<AudioSource>();
    }

    void Start()
    {
        currentHealth = maxHealth;

        if (playerSpritePrefab != null)
        {
            playerSpriteInstance = Instantiate(playerSpritePrefab, transform.position, Quaternion.identity, transform);
            animator = playerSpriteInstance.GetComponent<Animator>();
        }
        else
        {
            Debug.LogError("Player Sprite Prefab is not assigned!");
        }

        if (flashlightPrefab != null)
        {
            flashlightInstance = Instantiate(flashlightPrefab, transform.position, Quaternion.identity, transform);
            aimTransform = flashlightInstance.transform;
        }
        else
        {
            Debug.LogError("Flashlight Prefab is not assigned!");
        }

        aimDirection = Vector2.right;
        HandleAiming();
    }

    void Update()
    {
        HandleAiming();
        UpdateAnimations();
    }

    void FixedUpdate()
    {
        HandleMovement();
    }
    
    void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Enemy"))
        {
            TakeDamage(1);
        }
    }

    private void UpdateAnimations()
    {
        if (animator != null)
        {
            animator.SetBool("isMoving", moveDirection.sqrMagnitude > 0.01f);
        }
    }

    public void TakeDamage(int damageAmount)
    {
        if (Time.time >= lastDamageTime + invincibilityDuration)
        {
            lastDamageTime = Time.time;
            currentHealth -= damageAmount;

            if (damageSound != null && audioSource != null)
            FindObjectOfType<GameManager>()?.TakeDamage();
            Debug.Log("Player took " + damageAmount + " damage. Current health: " + currentHealth);
            
            if (damagedSound != null && audioSource != null)
            {
                audioSource.PlayOneShot(damageSound);
            }
            
            if (cameraController != null)
            {
                cameraController.TriggerDamageEffects();
            }
            
            Handheld.Vibrate();

            if (animator != null)
            {
                animator.SetTrigger("isDamaged");
            }
            
            if (currentHealth <= 0)
            {
                Die();
            }
        }
    }

    private void Die()
    {
        Debug.Log("Player Died!");
    }

    public void SetMoveDirection(Vector2 newDirection)
    {
        moveDirection = newDirection;
    }

    public void SetAimDirection(Vector2 newDirection)
    {
        if (newDirection != Vector2.zero)
        {
            aimDirection = newDirection;
        }
    }

    private void HandleMovement()
    {
        rb.velocity = moveDirection * moveSpeed;
    }

    private void HandleAiming()
    {
        if (aimTransform != null)
        {
            Vector2 targetDirection = aimDirection;
            Vector2 currentDirection = aimTransform.right;
            Vector2 smoothedDirection = Vector3.Slerp(currentDirection, targetDirection, aimSpeed * Time.deltaTime);
            float angle = Mathf.Atan2(smoothedDirection.y, smoothedDirection.x) * Mathf.Rad2Deg;
            aimTransform.rotation = Quaternion.Euler(0, 0, angle);
            Vector2 offset = smoothedDirection.normalized * aimDistance;
            
            // --- UPDATED: Apply the new offset to the final position ---
            aimTransform.position = (Vector2)transform.position + offset + flashlightOffset;
        }
    }
}
