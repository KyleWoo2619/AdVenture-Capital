using UnityEngine;
using TMPro;

// Written by Kyle
// This script displays the current score from SliceGameManager in the UI using TextMeshPro
// Attach this to a GameObject with a TextMeshPro component in your Canvas

public class ScoreDisplay : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private TextMeshProUGUI scoreText; // Assign the TextMeshPro component

    [Header("Score Settings")]
    [SerializeField] private string scorePrefix = "Score: "; // Text to show before the score number
    [SerializeField] private bool animateScoreChange = true; // Should score changes animate?
    [SerializeField] private float animationSpeed = 2f; // Speed of score animation

    private int currentDisplayedScore = 0; // The score currently being displayed
    private int targetScore = 0; // The score we want to display
    private bool isAnimating = false; // Is the score currently animating?

    void Start()
    {
        // Get the TextMeshPro component if not assigned
        if (scoreText == null)
            scoreText = GetComponent<TextMeshProUGUI>();

        // Initialize with starting score
        if (SliceGameManager.instance1 != null)
        {
            targetScore = SliceGameManager.instance1.GetScore();
            currentDisplayedScore = targetScore;
            UpdateScoreText();
        }
    }

    void Update()
    {
        // Check if SliceGameManager exists
        if (SliceGameManager.instance1 == null) return;

        // Get the current score from the game manager
        int gameScore = SliceGameManager.instance1.GetScore();

        // If the score has changed, update our target
        if (gameScore != targetScore)
        {
            targetScore = gameScore;
            
            if (animateScoreChange)
            {
                isAnimating = true;
            }
            else
            {
                // Instantly update if animation is disabled
                currentDisplayedScore = targetScore;
                UpdateScoreText();
            }
        }

        // Animate the score if needed
        if (isAnimating && animateScoreChange)
        {
            // Move towards target score
            if (currentDisplayedScore < targetScore)
            {
                currentDisplayedScore = Mathf.Min(targetScore, 
                    currentDisplayedScore + Mathf.CeilToInt(animationSpeed * Time.unscaledDeltaTime * 10));
                UpdateScoreText();

                // Stop animating when we reach the target
                if (currentDisplayedScore >= targetScore)
                {
                    currentDisplayedScore = targetScore;
                    isAnimating = false;
                }
            }
        }
    }

    // Updates the text display with current score
    private void UpdateScoreText()
    {
        if (scoreText != null)
        {
            scoreText.text = scorePrefix + currentDisplayedScore.ToString();
        }
    }

    // Public method to force update the score display (useful for testing)
    public void ForceUpdateScore()
    {
        if (SliceGameManager.instance1 != null)
        {
            targetScore = SliceGameManager.instance1.GetScore();
            currentDisplayedScore = targetScore;
            UpdateScoreText();
        }
    }

    // Public method to set custom score text formatting
    public void SetScorePrefix(string newPrefix)
    {
        scorePrefix = newPrefix;
        UpdateScoreText();
    }
}