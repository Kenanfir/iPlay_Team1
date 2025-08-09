using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraController : MonoBehaviour
{
    [Header("Target")]
    [Tooltip("The target the camera will follow. Assign your Player object here.")]
    public Transform target;

    [Header("Settings")]
    [Tooltip("How smoothly the camera follows the target. Smaller values are slower.")]
    [Range(0.01f, 1.0f)]
    public float smoothSpeed = 0.125f;

    [Tooltip("The offset of the camera from the target. Keep Z at -10 for a 2D camera.")]
    public Vector3 offset = new Vector3(0f, 0f, -10f);

    void LateUpdate()
    {
        // Check if a target has been assigned
        if (target == null)
        {
            Debug.LogWarning("Camera controller does not have a target assigned.");
            return;
        }

        // Calculate the desired position for the camera
        Vector3 desiredPosition = target.position + offset;
        
        // Use Vector3.Lerp to smoothly move from the camera's current position to the desired position
        Vector3 smoothedPosition = Vector3.Lerp(transform.position, desiredPosition, smoothSpeed);
        
        // Apply the new position to the camera
        transform.position = smoothedPosition;
    }
}

