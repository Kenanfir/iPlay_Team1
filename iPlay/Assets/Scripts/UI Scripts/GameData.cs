using UnityEngine;

public class GameData : MonoBehaviour
{
    public static GameData Instance;
    public int score;
    public float gameplayDuration;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }
}