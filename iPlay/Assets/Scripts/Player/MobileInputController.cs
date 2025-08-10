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

    [Header("Input Settings")]
    [Tooltip("The vertical portion of the screen (from the bottom, 0 to 1) that will be used for joystick input.")]
    [Range(0.1f, 1.0f)]
    public float touchAreaHeight = 0.5f; // Default to the bottom 50% of the screen

    // Private variables
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
                Debug.LogError("Player Controller could not be found!");
                enabled = false;
                return;
            }
        }

        moveJoystickInitialPos = moveJoystickHandle.anchoredPosition;
        aimJoystickInitialPos = aimJoystickHandle.anchoredPosition;

        moveJoystickBG.gameObject.SetActive(false);
        aimJoystickBG.gameObject.SetActive(false);
    }

    void Update()
    {
        moveInput = Vector2.zero;
        aimInput = Vector2.zero;

        for (int i = 0; i < Input.touchCount; i++)
        {
            Touch touch = Input.GetTouch(i);

            // --- NEW: Check if the touch is within the allowed vertical area ---
            if (touch.position.y > Screen.height * touchAreaHeight)
            {
                continue; // Ignore touches in the top part of the screen
            }

            // --- LEFT SIDE (MOVEMENT) ---
            if (touch.position.x < Screen.width / 2)
            {
                if (leftTouchId == -1 && touch.phase == TouchPhase.Began)
                {
                    leftTouchId = touch.fingerId;
                    moveJoystickBG.gameObject.SetActive(true);
                    moveJoystickBG.position = touch.position;
                }
                else if (touch.fingerId == leftTouchId)
                {
                    HandleJoystick(touch, moveJoystickBG, moveJoystickHandle, ref moveInput);

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
                if (rightTouchId == -1 && touch.phase == TouchPhase.Began)
                {
                    rightTouchId = touch.fingerId;
                    aimJoystickBG.gameObject.SetActive(true);
                    aimJoystickBG.position = touch.position;
                }
                else if (touch.fingerId == rightTouchId)
                {
                    HandleJoystick(touch, aimJoystickBG, aimJoystickHandle, ref aimInput);

                    if (touch.phase == TouchPhase.Ended || touch.phase == TouchPhase.Canceled)
                    {
                        rightTouchId = -1;
                        aimJoystickBG.gameObject.SetActive(false);
                        ResetJoystick(aimJoystickHandle, aimJoystickInitialPos);
                    }
                }
            }
        }

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
