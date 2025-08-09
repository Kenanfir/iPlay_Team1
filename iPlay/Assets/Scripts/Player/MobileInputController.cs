using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MobileInputController : MonoBehaviour
{
    [Header("Player Controller Reference")]
    [Tooltip("The PlayerController script that this script will send commands to.")]
    public PlayerController playerController;

    [Header("Input Settings")]
    [Tooltip("How far the player must drag their finger to reach maximum input speed.")]
    public float touchDragRadius = 100f;

    // Input vectors
    private Vector2 moveInput;
    private Vector2 aimInput;

    private int leftTouchId = -1;
    private int rightTouchId = -1;

    private Vector2 leftTouchStartPosition;
    private Vector2 rightTouchStartPosition;

    void Start()
    {
        if (playerController == null)
        {
            Debug.LogWarning("Player Controller not assigned! Trying to find it on the same GameObject.");
            playerController = GetComponent<PlayerController>();
            if (playerController == null)
            {
                Debug.LogError("Player Controller could not be found! This script needs a PlayerController to function.");
                enabled = false;
                return;
            }
        }
    }

    void Update()
    {
        // Reset inputs at the start of the frame
        moveInput = Vector2.zero;
        aimInput = Vector2.zero;

        // Process all current touches
        for (int i = 0; i < Input.touchCount; i++)
        {
            Touch touch = Input.GetTouch(i);

            // Check if the touch is on the left or right side of the screen
            if (touch.position.x < Screen.width / 2)
            {
                // --- LEFT SIDE (MOVEMENT) ---
                if (leftTouchId == -1 && touch.phase == TouchPhase.Began)
                {
                    leftTouchId = touch.fingerId;
                    leftTouchStartPosition = touch.position;
                }

                if (touch.fingerId == leftTouchId)
                {
                    // Calculate the vector from the start position to the current position
                    Vector2 dragVector = touch.position - leftTouchStartPosition;
                    // Clamp the magnitude and normalize to get the input vector
                    moveInput = Vector2.ClampMagnitude(dragVector, touchDragRadius) / touchDragRadius;

                    if (touch.phase == TouchPhase.Ended || touch.phase == TouchPhase.Canceled)
                    {
                        leftTouchId = -1; // Release the touch ID
                    }
                }
            }
            else
            {
                // --- RIGHT SIDE (AIMING) ---
                if (rightTouchId == -1 && touch.phase == TouchPhase.Began)
                {
                    rightTouchId = touch.fingerId;
                    rightTouchStartPosition = touch.position;
                }

                if (touch.fingerId == rightTouchId)
                {
                     // Calculate the vector from the start position to the current position
                    Vector2 dragVector = touch.position - rightTouchStartPosition;
                    // Clamp the magnitude and normalize to get the input vector
                    aimInput = Vector2.ClampMagnitude(dragVector, touchDragRadius) / touchDragRadius;

                    if (touch.phase == TouchPhase.Ended || touch.phase == TouchPhase.Canceled)
                    {
                        rightTouchId = -1; // Release the touch ID
                    }
                }
            }
        }

        // Send the final input values to the PlayerController
        playerController.SetMoveDirection(moveInput);
        
        // Ensure the player keeps aiming even if the finger is held still
        if(aimInput == Vector2.zero && rightTouchId != -1)
        {
             // If we are aiming but not moving the finger, we need to get the last known direction
             // This part is tricky without storing last direction, so for now, we just pass the raw input
        }

        playerController.SetAimDirection(aimInput);
    }
}
