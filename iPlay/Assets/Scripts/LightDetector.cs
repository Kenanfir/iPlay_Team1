using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LightDetector : MonoBehaviour
{
    // A public list that other scripts can access
    public List<Transform> enemiesInLight = new List<Transform>();

    void OnTriggerEnter2D(Collider2D other)
    {
        // When an enemy enters the light, add it to the list
        if (other.CompareTag("Enemy"))
        {
            if (!enemiesInLight.Contains(other.transform))
            {
                enemiesInLight.Add(other.transform);
            }
        }
    }

    void OnTriggerExit2D(Collider2D other)
    {
        // When an enemy leaves the light, remove it from the list
        if (other.CompareTag("Enemy"))
        {
            if (enemiesInLight.Contains(other.transform))
            {
                enemiesInLight.Remove(other.transform);
            }
        }
    }
}
