using UnityEngine;
using UnityEngine.UI;

[System.Serializable]
public class GhostFacePair
{
    public Sprite aliveFace;  // The alive ghost face sprite
    public Sprite deathFace;  // The corresponding death face sprite
    
    public bool IsValid()
    {
        return aliveFace != null && deathFace != null;
    }
}

public class RandomEnemyFace : MonoBehaviour
{
    [Header("Ghost Face Pairs")]
    [SerializeField] private GhostFacePair[] ghostFacePairs = new GhostFacePair[6]; // Array of 6 ghost face pairs
    
    [Header("Target Image")]
    [SerializeField] private Image enemyImage; // The Image component to display the selected face

    private int selectedPairIndex = -1; // Track which face pair is currently selected
    private GhostFacePair currentPair; // Reference to the currently selected pair

    void Start()
    {
        SetRandomGhostlyFace();
    }

    void SetRandomGhostlyFace()
    {
        // Check if we have face pairs assigned and an image component
        if (ghostFacePairs == null || ghostFacePairs.Length == 0)
        {
            Debug.LogWarning("No ghost face pairs assigned to RandomEnemyFace script!");
            return;
        }

        if (enemyImage == null)
        {
            Debug.LogWarning("No enemy image component assigned to RandomEnemyFace script!");
            return;
        }

        // Find valid face pairs (both alive and death faces are not null)
        var validIndices = new System.Collections.Generic.List<int>();
        for (int i = 0; i < ghostFacePairs.Length; i++)
        {
            if (ghostFacePairs[i] != null && ghostFacePairs[i].IsValid())
            {
                validIndices.Add(i);
            }
        }

        if (validIndices.Count == 0)
        {
            Debug.LogWarning("No valid ghost face pairs found! Make sure both alive and death faces are assigned for each pair.");
            return;
        }

        // Randomly select a valid face pair
        selectedPairIndex = validIndices[Random.Range(0, validIndices.Count)];
        currentPair = ghostFacePairs[selectedPairIndex];
        
        // Apply the alive face to the enemy image
        enemyImage.sprite = currentPair.aliveFace;

        Debug.Log($"Selected ghost pair #{selectedPairIndex}: Alive={currentPair.aliveFace.name}, Death={currentPair.deathFace.name}");
    }

    // Public method to switch to death face (call this when enemy dies)
    public void ShowDeathFace()
    {
        if (selectedPairIndex == -1 || currentPair == null)
        {
            Debug.LogWarning("No ghost face pair selected yet! Call SetRandomGhostlyFace() first.");
            return;
        }

        if (currentPair.deathFace == null)
        {
            Debug.LogWarning($"Death face for selected pair is null!");
            return;
        }

        if (enemyImage != null)
        {
            enemyImage.sprite = currentPair.deathFace;
            Debug.Log($"Switched to death face: {currentPair.deathFace.name}");
        }
    }

    // Public method to manually change face (useful for testing or special events)
    public void ChangeToRandomFace()
    {
        SetRandomGhostlyFace();
    }

    // Public method to set a specific face pair by index (0-5)
    public void SetSpecificFacePair(int pairIndex)
    {
        if (ghostFacePairs == null || pairIndex < 0 || pairIndex >= ghostFacePairs.Length)
        {
            Debug.LogWarning($"Invalid pair index: {pairIndex}");
            return;
        }

        if (ghostFacePairs[pairIndex] == null || !ghostFacePairs[pairIndex].IsValid())
        {
            Debug.LogWarning($"Ghost face pair at index {pairIndex} is invalid or has null sprites!");
            return;
        }

        selectedPairIndex = pairIndex;
        currentPair = ghostFacePairs[pairIndex];
        
        if (enemyImage != null)
        {
            enemyImage.sprite = currentPair.aliveFace;
            Debug.Log($"Set alive face to pair {pairIndex}: {currentPair.aliveFace.name}");
        }
    }
}