using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI; 

public class CameraController : MonoBehaviour
{
    [Header("Target")]
    [Tooltip("The target the camera will follow. Assign your Player object here.")]
    public Transform target;

    [Header("Follow Settings")]
    [Tooltip("How smoothly the camera follows the target. Smaller values are slower.")]
    [Range(0.01f, 1.0f)]
    public float smoothSpeed = 0.125f;
    [Tooltip("The offset of the camera from the target. Keep Z at -10 for a 2D camera.")]
    public Vector3 offset = new Vector3(0f, 0f, -10f);

    [Header("Damage Effects")]
    [Tooltip("The UI Image for the red damage vignette.")]
    public Image damageVignette;
    [Tooltip("How long the screen shakes when the player is hit.")]
    public float shakeDuration = 0.2f;
    [Tooltip("How intense the screen shake is.")]
    public float shakeMagnitude = 0.1f;
    public float vignetteFadeSpeed = 2f;

    private Vector3 originalPosition;

    void Start()
    {
        // Store the camera's original local position for screen shake
        originalPosition = transform.localPosition;
    }

    void LateUpdate()
    {
        if (target == null)
        {
            Debug.LogWarning("Camera controller does not have a target assigned.");
            return;
        }

        // The follow logic is now separate from the shake logic
        Vector3 desiredPosition = target.position + offset;
        Vector3 smoothedPosition = Vector3.Lerp(transform.position, desiredPosition, smoothSpeed);
        transform.position = smoothedPosition;
    }

    public void TriggerDamageEffects()
    {
        StartCoroutine(DamageEffectsCoroutine());
    }

    private IEnumerator DamageEffectsCoroutine()
    {
        // --- VIGNETTE FADE IN ---
        float fadeInDuration = 1f / vignetteFadeSpeed;
        float elapsed = 0f;
        
        if (damageVignette != null)
        {
            Color originalColor = damageVignette.color;
            while (elapsed < fadeInDuration)
            {
                elapsed += Time.deltaTime;
                float alpha = Mathf.Lerp(0f, 1f, elapsed / fadeInDuration);
                damageVignette.color = new Color(originalColor.r, originalColor.g, originalColor.b, alpha);
                yield return null;
            }
        }

        // --- SCREEN SHAKE ---
        originalPosition = transform.localPosition;
        elapsed = 0.0f;
        while (elapsed < shakeDuration)
        {
            float x = Random.Range(-1f, 1f) * shakeMagnitude;
            float y = Random.Range(-1f, 1f) * shakeMagnitude;
            transform.localPosition = new Vector3(originalPosition.x + x, originalPosition.y + y, originalPosition.z);
            elapsed += Time.deltaTime;
            yield return null;
        }
        transform.localPosition = originalPosition;

        // --- VIGNETTE FADE OUT ---
        // Wait for the vignette to be visible for a moment
        yield return new WaitForSeconds(0.5f); 

        float fadeOutDuration = 1f / vignetteFadeSpeed;
        elapsed = 0f;

        if (damageVignette != null)
        {
            Color originalColor = damageVignette.color;
            while (elapsed < fadeOutDuration)
            {
                elapsed += Time.deltaTime;
                float alpha = Mathf.Lerp(1f, 0f, elapsed / fadeOutDuration);
                damageVignette.color = new Color(originalColor.r, originalColor.g, originalColor.b, alpha);
                yield return null;
            }
            // Ensure it's fully transparent at the end
            damageVignette.color = new Color(originalColor.r, originalColor.g, originalColor.b, 0);
        }
    }
}

