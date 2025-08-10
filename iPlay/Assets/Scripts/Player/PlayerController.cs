using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [Header("Player Visuals")]
    [Tooltip("The prefab that contains the player's sprite and animations.")]
    public GameObject playerSpritePrefab;
    [Tooltip("The prefab for the flashlight/weapon object.")]
    public GameObject flashlightPrefab;

    [Header("Player Stats")]
    public int maxHealth = 6;
    private int startingHealth = 6;
    private int currentHealth;
    public float invincibilityDuration = 2f;
    private float lastDamageTime;

    [Header("SFX")]
    [Tooltip("The sound effect to play when the player gets hit.")]
    public AudioClip damagedSound; 

    [Header("Movement")]
    [Tooltip("How fast the player moves.")]
    public float moveSpeed = 5f;

    [Header("Aiming")]
    [Tooltip("How far the flashlight is from the player's center.")]
    public float aimDistance = 1.5f;
    [Tooltip("How smoothly the flashlight rotates to the aim direction. Smaller is slower.")]
    public float aimSpeed = 10f;

    public CameraController cameraController;

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
        if (rb == null)
        {
            rb = gameObject.AddComponent<Rigidbody2D>();
        }
        rb.gravityScale = 0;
        rb.drag = 10f;
        rb.freezeRotation = true;
        
        audioSource = GetComponent<AudioSource>();
    }

    void Start()
    {
        currentHealth = startingHealth; // Initialize currentHealth
        // Create the player's visual sprite from the prefab
        if (playerSpritePrefab != null)
        {
            playerSpriteInstance = Instantiate(playerSpritePrefab, transform.position, Quaternion.identity, transform);
            animator = playerSpriteInstance.GetComponent<Animator>();
        }
        else
        {
            Debug.LogError("Player Sprite Prefab is not assigned in the PlayerController!");
        }

        // Create the flashlight from its prefab
        if (flashlightPrefab != null)
        {
            
            flashlightInstance = Instantiate(flashlightPrefab, transform.position, Quaternion.identity, transform);
            aimTransform = flashlightInstance.transform; // transform for aiming
        }
        else
        {
            Debug.LogError("Flashlight Prefab is not assigned in the PlayerController!");
        }

        // Initialize flashlight aim directions
        aimDirection = Vector2.right;
        HandleAiming();
    }

    void Update()
    {
        // Handle aiming logic in Update for responsiveness
        HandleAiming();
        UpdateAnimations();
    }

    void FixedUpdate()
    {
        // Handle movement logic in FixedUpdate for smooth physics
        HandleMovement();
    }

    void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Enemy"))
        {
            TakeDamage(1);
        }
    }

    public void TakeDamage(int damageAmount)
    {
        if (Time.time >= lastDamageTime + invincibilityDuration)
        {
            lastDamageTime = Time.time;
            currentHealth -= damageAmount;
            Debug.Log("Player took " + damageAmount + " damage. Current health: " + currentHealth);
            
            if (damagedSound != null && audioSource != null)
            {
                audioSource.PlayOneShot(damagedSound);
            }

            if (cameraController != null)
            {
                cameraController.TriggerDamageEffects();
            }
        
            // Trigger haptic feedback
            Handheld.Vibrate();

            animator?.SetTrigger("isDamaged");

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

    private void UpdateAnimations()
    {
        if (animator != null)
        {
            // Check if the moveDirection vector has any length. 
            // sqrMagnitude is more efficient than magnitude.
            if (moveDirection.sqrMagnitude > 0.01f)
            {
                animator.SetBool("isMoving", true);
            }
            else
            {
                animator.SetBool("isMoving", false);
            }
        }
    }
    // This method will be called by the Input Controller
    public void SetMoveDirection(Vector2 newDirection)
    {
        moveDirection = newDirection;
    }

    // This method will be called by the Input Controller
    public void SetAimDirection(Vector2 newDirection)
    {
        aimDirection = newDirection;
    }

    private void HandleMovement()
    {
        // Apply force to move the player
        rb.velocity = moveDirection * moveSpeed;
    }

    private void HandleAiming()
    {
        if (aimTransform != null)
        {
            // Define the target direction from the input
            Vector2 targetDirection = aimDirection;

            // Get the flashlight's current forward direction (we use .right because the sprite points right)
            Vector2 currentDirection = aimTransform.right;

            // Smoothly interpolate between the current and target directions using Slerp
            Vector2 smoothedDirection = Vector3.Slerp(currentDirection, targetDirection, aimSpeed * Time.deltaTime);

            // Calculate the angle from the new smoothed direction
            float angle = Mathf.Atan2(smoothedDirection.y, smoothedDirection.x) * Mathf.Rad2Deg;
            
            // Apply the new rotation
            aimTransform.rotation = Quaternion.Euler(0, 0, angle);

            // Position the flashlight using the smoothed direction to avoid jitter
            Vector2 offset = smoothedDirection.normalized * aimDistance;
            aimTransform.position = (Vector2)transform.position + offset;
        }
    }
}
