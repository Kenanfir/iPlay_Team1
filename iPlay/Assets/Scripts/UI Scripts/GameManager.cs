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

    public RectTransform bar;
    public Vector3[] positions;
    public float barMoveDuration;
    public float barPauseDuration;

    public Image[] hearts; // Assign in Inspector
    public Sprite fullHeart;
    public Sprite halfHeart;
    public Sprite emptyHeart;
    private int health;

    public Text scoreText;
    private int score = 0;

    float startTime;
    float endTime;

    public Image fadeImage; 
    public float fadeDuration;
    public Image winTextWhenFade;
    public Image loseTextWhenFade;

    public AudioClip resumeSound;
    private AudioSource audioSource;

    void Start()
    {
        startTime = Time.time;
        audioSource = GetComponent<AudioSource>();
        AudioManager.Instance.StartAudioBackground();
        StartDawnBar();
        StartHealthBar();
        StartScoreCounter();
    }
    void Update()
    {

    }

    public void PauseGame()
    {
        Debug.Log("kepencet kok");
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


    #region Dawn bar

    void StartDawnBar()
    {
        if (positions.Length < 2) return;
        bar.anchoredPosition = positions[0];
        StartCoroutine(MoveBar());
    }

    IEnumerator MoveBar()
    {
        for (int i = 0; i < positions.Length - 1; i++)
        {
            // Move
            float elapsed = 0f;
            while (elapsed < barMoveDuration)
            {
                elapsed += Time.deltaTime;
                float t = Mathf.Clamp01(elapsed / barMoveDuration);
                bar.anchoredPosition = Vector3.Lerp(positions[i], positions[i + 1], t);
                yield return null;
            }

            // Pause
            if (i < positions.Length - 2)
            {
                yield return new WaitForSeconds(barPauseDuration);
            }
        }
        DurationCounter();
        AudioManager.Instance.PlayWinSound();
        winTextWhenFade.gameObject.SetActive(true);
        StartCoroutine(FadeAndLoadScene("Steven - Winning", true));
    }
    #endregion

    #region Health Bar


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
    #endregion

    #region Score Counter

    void StartScoreCounter()
    {
        UpdateScoreText();
    }

    public void AddScore()
    {
        score += 50;

        if (GameData.Instance != null)
        {
            GameData.Instance.score = score;
        }

        UpdateScoreText();
    }

    void UpdateScoreText()
    {
        scoreText.text = score.ToString();

    }

    #endregion

    #region Duration Counter
    void DurationCounter()
    {
        endTime = Time.time;
        GameData.Instance.gameplayDuration = endTime - startTime;
    }
    #endregion
    
}
