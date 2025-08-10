using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class StartingManager : MonoBehaviour
{
    public Image[] startingScreen;
    public GameObject MainMenuScreen;
    public float autoChangeDelay = 5f; // 5 seconds per image

    private int currentIndex = 0;
    private bool autoChangeActive = true;

    void Start()
    {
        ShowCurrentScreen();
        MainMenuScreen.SetActive(false);
        StartCoroutine(AutoChangeScreens());
    }

    void Update()
    {
        // Manual skip by tap or click
        if (Input.touchCount > 0 && Input.GetTouch(0).phase == TouchPhase.Began)
        {
            ManualSkip();
        }
        if (Input.GetMouseButtonDown(0))
        {
            ManualSkip();
        }
    }

    IEnumerator AutoChangeScreens()
    {
        while (autoChangeActive)
        {
            yield return new WaitForSeconds(autoChangeDelay);
            NextScreen();
        }
    }

    void ManualSkip()
    {
        // If still auto-changing, stop and let user control
        if (autoChangeActive)
        {
            autoChangeActive = false;
            StopCoroutine(AutoChangeScreens());
        }
        NextScreen();
    }

    void NextScreen()
    {
        currentIndex++;
        Debug.Log("Current Index: " + currentIndex);

        // Go to game scene after index 7
        if (currentIndex == 7)
        {
            SceneManager.LoadScene("Game Scene");
            return;
        }

        // If reached end, show main menu and stop auto-change
        if (currentIndex >= startingScreen.Length)
        {
            foreach (var img in startingScreen)
                img.gameObject.SetActive(false);

            MainMenuScreen.SetActive(true);
            autoChangeActive = false; // Stop auto cycling
        }
        else
        {
            ShowCurrentScreen();
        }
    }

    void ShowCurrentScreen()
    {
        foreach (var img in startingScreen)
            img.gameObject.SetActive(false);

        startingScreen[currentIndex].gameObject.SetActive(true);
    }
}
