using TMPro;
using UnityEngine;

public class DisplayScore : MonoBehaviour
{
    private static TextMeshProUGUI scoreText;
    void Start()
    {
        scoreText = GetComponent<TextMeshProUGUI>();
        
    }

    

    // Update is called once per frame
    void Update()
    {

    }

    public static void UpdateScore()
    {
        scoreText.text = GameManager.instance.score.ToString();
    }
}
