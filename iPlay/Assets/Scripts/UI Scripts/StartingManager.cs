using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class StartingManager : MonoBehaviour
{
    public Image[] startingScreen;
    public GameObject MainMenuScreen;

    private int currentIndex = 0;

    void Start()
    {
        ShowCurrentScreen();
        MainMenuScreen.SetActive(false); 
    }

    void Update()
    {
        if (Input.touchCount > 0 && Input.GetTouch(0).phase == TouchPhase.Began)
        {
            NextScreen();
        }

        if (Input.GetMouseButtonDown(0))
        {
            NextScreen();
        }
    }

    void NextScreen()
    {
        currentIndex++;
        //Debug.Log("Current Index" + currentIndex);

        if (currentIndex == 7)
        {
            // masuk ke scene game
            SceneManager.LoadScene("");
        }

        if (currentIndex == startingScreen.Length)
        {
            foreach (var img in startingScreen)
                img.gameObject.SetActive(false);

            MainMenuScreen.SetActive(true);
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
