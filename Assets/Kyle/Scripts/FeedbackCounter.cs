using TMPro;
using UnityEngine;

public class FeedbackCounter : MonoBehaviour
{
    [SerializeField] private TMP_InputField feedbackField;
    [SerializeField] private TMP_Text counterText;

    private void Start()
    {
        UpdateCounter(feedbackField.text);
        feedbackField.onValueChanged.AddListener(UpdateCounter);
    }

    private void UpdateCounter(string text)
    {
        counterText.text = $"{text.Length}/300";
    }
}
