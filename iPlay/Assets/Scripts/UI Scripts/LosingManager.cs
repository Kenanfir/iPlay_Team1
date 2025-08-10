using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class LosingManager : MonoBehaviour
{
    public Text scoreText;
    public Text durationText;

    void Start()
    {
        scoreText.text = GameData.Instance.score.ToString();
        int minutes = Mathf.FloorToInt(GameData.Instance.gameplayDuration / 60f);
        int seconds = Mathf.FloorToInt(GameData.Instance.gameplayDuration % 60f);
        int milliseconds = Mathf.FloorToInt((GameData.Instance.gameplayDuration * 1000f) % 1000f / 10f); // two digits

        durationText.text = $"{minutes:00}:{seconds:00}:{milliseconds:00}";
    }

    void Update()
    {
        if (Input.touchCount > 0 && Input.GetTouch(0).phase == TouchPhase.Began)
        {
            Vector2 touchPosition = Input.GetTouch(0).position;

            if (touchPosition.x < Screen.width / 2f)
            {
                SceneManager.LoadScene("Steven - Starting");
            }
            else
            {
                //scenemanager ke game
            }
        }
        if (Input.GetMouseButtonDown(0))
        {
            Vector2 mousePosition = Input.mousePosition;

            if (mousePosition.x < Screen.width / 2f)
            {
                SceneManager.LoadScene("Steven - Starting");
            }
            else
            {
                //scenemanager ke game
            }
        }
    }
}
