using UnityEngine;
using TMPro;

[DefaultExecutionOrder(-100)] // initialize early
public class ScoreManager : MonoBehaviour
{
    public static ScoreManager Instance { get; private set; }

    [Header("UI")]
    public TMP_Text scoreText;     // drag your ScoreText here

    [Header("Scoring")]
    public float pointsPerSecond = 10f; // time score rate
    public int dodgeBonus = 25;         // per obstacle that you survive

    private float score;

    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
    }

    void Update()
    {
        // time-based score
        score += pointsPerSecond * Time.deltaTime;
        UpdateUI();
    }

    public void AddDodge(int count = 1)
    {
        score += dodgeBonus * count;
        UpdateUI();
    }

    public void AddScore(int points)
    {
        score += points;
        UpdateUI();
    }

    public void ResetScore()
    {
        score = 0f;
        UpdateUI();
    }

    void UpdateUI()
    {
        if (scoreText != null)
            scoreText.text = Mathf.FloorToInt(score).ToString();
    }

    public int GetScoreInt() => Mathf.FloorToInt(score);
    public float GetScore() => score;
}
