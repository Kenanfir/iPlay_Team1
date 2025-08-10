using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;

public class WinningManager : MonoBehaviour
{
    public GameObject scoreBoard;
    public GameObject credit;

    public Text scoreText;
    public Text durationText;

    int tapCount = 0;

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
            isNext();
            tapCount += 1;
        }
        //untuk testing
        if (Input.GetMouseButtonDown(0))
        {
            isNext();
            tapCount += 1;
        }
    }

    public void isNext()
    {
        scoreBoard.SetActive(false);
        credit.SetActive(true);

        if (tapCount >= 1)
        {
            SceneManager.LoadScene("Steven - Starting");
        }
    }
}
