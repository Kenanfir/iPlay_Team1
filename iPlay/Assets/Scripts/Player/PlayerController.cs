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

    [Header("Movement")]
    [Tooltip("How fast the player moves.")]
    public float moveSpeed = 5f;

    [Header("Aiming")]
    [Tooltip("How far the flashlight is from the player's center.")]
    public float aimDistance = 1.5f;

    // Private components
    private Rigidbody2D rb;
    private GameObject playerSpriteInstance;
    private GameObject flashlightInstance;
    private Transform aimTransform;
    private Vector2 moveDirection;
    private Vector2 aimDirection;

    void Awake()
    {
        // Ensure the player has a Rigidbody2D for physics
        rb = GetComponent<Rigidbody2D>();
        if (rb == null)
        {
            rb = gameObject.AddComponent<Rigidbody2D>();
        }
        rb.gravityScale = 0; // Top-down games usually don't have gravity
        rb.drag = 10f; // Add some drag to make movement feel tighter
        rb.freezeRotation = true; // Prevent the player from spinning from collisions
    }

    void Start()
    {
        // Create the player's visual sprite from the prefab
        if (playerSpritePrefab != null)
        {
            playerSpriteInstance = Instantiate(playerSpritePrefab, transform.position, Quaternion.identity, transform);
        }
        else
        {
            Debug.LogError("Player Sprite Prefab is not assigned in the PlayerController!");
        }

        // Create the flashlight from its prefab
        if (flashlightPrefab != null)
        {
            // We still parent it to the player so it moves with the player
            flashlightInstance = Instantiate(flashlightPrefab, transform.position, Quaternion.identity, transform);
            aimTransform = flashlightInstance.transform; // Get the transform for aiming
        }
        else
        {
            Debug.LogError("Flashlight Prefab is not assigned in the PlayerController!");
        }

        aimDirection = Vector2.right;
        HandleAiming();
    }

    void Update()
    {
        // Handle aiming logic in Update for responsiveness
        HandleAiming();
    }

    void FixedUpdate()
    {
        // Handle movement logic in FixedUpdate for smooth physics
        HandleMovement();
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
        if (aimTransform != null && aimDirection != Vector2.zero)
        {
            // Calculate the angle from the aim input vector
            float angle = Mathf.Atan2(aimDirection.y, aimDirection.x) * Mathf.Rad2Deg;


            aimTransform.rotation = Quaternion.Euler(0, 0, angle);

            Vector2 offset = aimDirection.normalized * aimDistance;

            aimTransform.position = (Vector2)transform.position + offset;
        }
    }
}
