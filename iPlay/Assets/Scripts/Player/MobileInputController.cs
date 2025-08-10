using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MobileInputController : MonoBehaviour
{
    [Header("Player Controller Reference")]
    [Tooltip("The PlayerController script that this script will send commands to.")]
    public PlayerController playerController;

    [Header("UI Joysticks")]
    [Tooltip("The background image of the movement joystick.")]
    public RectTransform moveJoystickBG;
    [Tooltip("The handle image of the movement joystick.")]
    public RectTransform moveJoystickHandle;

    [Tooltip("The background image of the aiming joystick.")]
    public RectTransform aimJoystickBG;
    [Tooltip("The handle image of the aiming joystick.")]
    public RectTransform aimJoystickHandle;

    // Private variables to store input data
    private Vector2 moveInput;
    private Vector2 aimInput;

    private int leftTouchId = -1;
    private int rightTouchId = -1;

    private Vector2 moveJoystickInitialPos;
    private Vector2 aimJoystickInitialPos;

    void Start()
    {
        if (playerController == null)
        {
            playerController = GetComponent<PlayerController>();
            if (playerController == null)
            {
                Debug.LogError("Player Controller could not be found! This script needs a PlayerController to function.");
                enabled = false;
                return;
            }
        }

        // Store the initial positions of the joystick handles for resetting
        moveJoystickInitialPos = moveJoystickHandle.anchoredPosition;
        aimJoystickInitialPos = aimJoystickHandle.anchoredPosition;

        // --- NEW: Hide the joysticks at the start ---
        moveJoystickBG.gameObject.SetActive(false);
        aimJoystickBG.gameObject.SetActive(false);
    }

    void Update()
    {
        // Reset inputs for this frame
        moveInput = Vector2.zero;
        aimInput = Vector2.zero;

        // Process all current touches
        for (int i = 0; i < Input.touchCount; i++)
        {
            Touch touch = Input.GetTouch(i);

            // --- LEFT SIDE (MOVEMENT) ---
            if (touch.position.x < Screen.width / 2)
            {
                // If this is a new touch on the left side
                if (leftTouchId == -1 && touch.phase == TouchPhase.Began)
                {
                    leftTouchId = touch.fingerId;
                    // Show the joystick and move it to the touch position
                    moveJoystickBG.gameObject.SetActive(true);
                    moveJoystickBG.position = touch.position;
                }
                // If this is a continuing touch from our tracked finger
                else if (touch.fingerId == leftTouchId)
                {
                    HandleJoystick(touch, moveJoystickBG, moveJoystickHandle, ref moveInput);

                    // If the finger is lifted, hide the joystick
                    if (touch.phase == TouchPhase.Ended || touch.phase == TouchPhase.Canceled)
                    {
                        leftTouchId = -1;
                        moveJoystickBG.gameObject.SetActive(false);
                        ResetJoystick(moveJoystickHandle, moveJoystickInitialPos);
                    }
                }
            }
            // --- RIGHT SIDE (AIMING) ---
            else
            {
                // If this is a new touch on the right side
                if (rightTouchId == -1 && touch.phase == TouchPhase.Began)
                {
                    rightTouchId = touch.fingerId;
                    // Show the joystick and move it to the touch position
                    aimJoystickBG.gameObject.SetActive(true);
                    aimJoystickBG.position = touch.position;
                }
                // If this is a continuing touch from our tracked finger
                else if (touch.fingerId == rightTouchId)
                {
                    HandleJoystick(touch, aimJoystickBG, aimJoystickHandle, ref aimInput);

                    // If the finger is lifted, hide the joystick
                    if (touch.phase == TouchPhase.Ended || touch.phase == TouchPhase.Canceled)
                    {
                        rightTouchId = -1;
                        aimJoystickBG.gameObject.SetActive(false);
                        ResetJoystick(aimJoystickHandle, aimJoystickInitialPos);
                    }
                }
            }
        }

        // Send the final input values to the PlayerController
        playerController.SetMoveDirection(moveInput);
        playerController.SetAimDirection(aimInput);
    }

    private void HandleJoystick(Touch touch, RectTransform joystickBG, RectTransform joystickHandle, ref Vector2 inputVector)
    {
        Vector2 localPoint;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(joystickBG, touch.position, null, out localPoint);

        float joystickRadius = joystickBG.sizeDelta.x / 2;
        Vector2 direction = Vector2.ClampMagnitude(localPoint, joystickRadius);
        
        joystickHandle.anchoredPosition = direction;
        inputVector = direction / joystickRadius;
    }

    private void ResetJoystick(RectTransform joystickHandle, Vector2 initialPos)
    {
        joystickHandle.anchoredPosition = initialPos;
    }
}
