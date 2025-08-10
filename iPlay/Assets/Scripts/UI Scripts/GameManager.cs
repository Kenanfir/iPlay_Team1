using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public GameObject pausePanel;
    public GameObject overlayToWinOrLose;
    private bool isPaused = false;

    [Header("Dawn/Wave Bar")]
    public RectTransform bar;
    public Vector3[] positions; // start & intermediate positions
    public float barMoveDuration = 0.5f; // smooth move between wave segments

    [Header("Health UI")]
    public Image[] hearts; 
    public Sprite fullHeart;
    public Sprite halfHeart;
    public Sprite emptyHeart;
    private int health;

    [Header("Score UI")]
    public Text scoreText;
    private int score = 0;

    [Header("Fade/Win/Lose")]
    public Image fadeImage; 
    public float fadeDuration;
    public Image winTextWhenFade;
    public Image loseTextWhenFade;

    [Header("Audio")]
    public AudioClip resumeSound;
    private AudioSource audioSource;

    float startTime;
    float endTime;

    void Start()
    {
        startTime = Time.time;
        audioSource = GetComponent<AudioSource>();
        AudioManager.Instance.StartAudioBackground();

        StartHealthBar();
        StartScoreCounter();

        // Initialize bar at start position
        if (positions.Length > 0) bar.anchoredPosition = positions[0];
    }

    public void PauseGame()
    {
        pausePanel.SetActive(true);
        Time.timeScale = 0f;
        isPaused = true;
    }

    public void ResumeGame()
    {
        audioSource.PlayOneShot(resumeSound);
        pausePanel.SetActive(false);
        Time.timeScale = 1f;
        isPaused = false;
    }

    private IEnumerator FadeAndLoadScene(string sceneName, bool isWin)
    {
        overlayToWinOrLose.SetActive(true);
        Color c = isWin ? Color.white : Color.black;
        c.a = 0f;
        fadeImage.color = c;

        float elapsedTime = 0f;

        while (elapsedTime < fadeDuration)
        {
            elapsedTime += Time.deltaTime;
            c.a = Mathf.Clamp01(elapsedTime / fadeDuration);
            fadeImage.color = c;
            yield return null;
        }
        SceneManager.LoadScene(sceneName);
    }

    // ---------------- Dawn Bar Progress (Synced with Waves) ----------------
    public void SetBarProgress(float normalizedProgress)
    {
        if (positions.Length < 2) return;

        float totalPos = normalizedProgress * (positions.Length - 1);
        int index = Mathf.Clamp(Mathf.FloorToInt(totalPos), 0, positions.Length - 2);
        float t = totalPos - index;

        StopCoroutine(nameof(MoveBarSegment));
        StartCoroutine(MoveBarSegment(index, t));
    }

    private IEnumerator MoveBarSegment(int index, float targetT)
    {
        Vector3 startPos = bar.anchoredPosition;
        Vector3 endPos = Vector3.Lerp(positions[index], positions[index + 1], targetT);
        float elapsed = 0f;

        while (elapsed < barMoveDuration)
        {
            elapsed += Time.deltaTime;
            bar.anchoredPosition = Vector3.Lerp(startPos, endPos, elapsed / barMoveDuration);
            yield return null;
        }
    }

    // ---------------- Health Bar ----------------
    void StartHealthBar()
    {
        health = hearts.Length * 2;
        UpdateHearts();
    }

    public void TakeDamage()
    {
        if (health > 0)
        {
            health--;
            UpdateHearts();

            if (health == 0)
            {
                DurationCounter();
                AudioManager.Instance.PlayLoseSound();
                loseTextWhenFade.gameObject.SetActive(true);
                StartCoroutine(FadeAndLoadScene("Steven - Losing", false));
            }
        }
    }

    void UpdateHearts()
    {
        for (int i = 0; i < hearts.Length; i++)
        {
            int heartHealth = Mathf.Clamp(health - (i * 2), 0, 2);

            if (heartHealth == 2)
                hearts[i].sprite = fullHeart;
            else if (heartHealth == 1)
                hearts[i].sprite = halfHeart;
            else
                hearts[i].sprite = emptyHeart;
        }
    }

    // ---------------- Score Counter ----------------
    void StartScoreCounter()
    {
        UpdateScoreText();
    }

    public void AddScore()
    {
        score += 50;
        if (GameData.Instance != null)
            GameData.Instance.score = score;
        UpdateScoreText();
    }

    void UpdateScoreText()
    {
        scoreText.text = score.ToString();
    }

    // ---------------- Duration Counter ----------------
    void DurationCounter()
    {
        endTime = Time.time;
        if (GameData.Instance != null)
            GameData.Instance.gameplayDuration = endTime - startTime;
    }

    // ---------------- Win/Lose ----------------
    public void WinGame()
    {
        DurationCounter();
        AudioManager.Instance.PlayWinSound();
        winTextWhenFade.gameObject.SetActive(true);
        StartCoroutine(FadeAndLoadScene("Steven - Winning", true));
    }

    public void LoseGame()
    {
        DurationCounter();
        AudioManager.Instance.PlayLoseSound();
        loseTextWhenFade.gameObject.SetActive(true);
        StartCoroutine(FadeAndLoadScene("Steven - Losing", false));
    }
}
