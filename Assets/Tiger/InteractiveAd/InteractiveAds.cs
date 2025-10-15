using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class InteractiveAds : MonoBehaviour
{
    [System.Serializable]
    public class AdStep
    {
        public string stepName;
        public Sprite image;
        public string choiceAText;
        public string choiceBText;
        public int nextStepA = -1; // -1 ends the ad
        public int nextStepB = -1;
    }

    [Header("UI References")]
    [SerializeField] private Image adImage;
    [SerializeField] private Button choiceAButton;
    [SerializeField] private Button choiceBButton;
    [SerializeField] private Text choiceALabel;
    [SerializeField] private Text choiceBLabel;

    [Header("Ad Steps")]
    [SerializeField] private List<AdStep> adSteps = new();

    private int currentStepIndex = 0;
    private Action onComplete;

    public void StartInteractiveAd(Action onAdComplete)
    {
        Debug.Log("[InteractiveAd] Starting interactive ad");

        currentStepIndex = 0;
        onComplete = onAdComplete;
        ShowCurrentStep();
    }

    private void ShowCurrentStep()
    {
        if (currentStepIndex < 0 || currentStepIndex >= adSteps.Count)
        {
            Debug.LogWarning("[InteractiveAd] Invalid step index: " + currentStepIndex);
            EndAd();
            return;
        }

        AdStep step = adSteps[currentStepIndex];

        Debug.Log($"[InteractiveAd] Showing step: {step.stepName} (Index {currentStepIndex})");

        if (adImage) adImage.sprite = step.image;

        if (choiceALabel) choiceALabel.text = step.choiceAText;
        if (choiceBLabel) choiceBLabel.text = step.choiceBText;

        if (choiceAButton)
        {
            choiceAButton.onClick.RemoveAllListeners();
            choiceAButton.onClick.AddListener(() => MakeChoice(step.nextStepA, "A"));
        }

        if (choiceBButton)
        {
            choiceBButton.onClick.RemoveAllListeners();
            choiceBButton.onClick.AddListener(() => MakeChoice(step.nextStepB, "B"));
        }
    }

    private void MakeChoice(int nextStep, string choiceLabel)
    {
        Debug.Log($"[InteractiveAd] Player chose option {choiceLabel} at step {currentStepIndex}");

        if (nextStep == -1)
        {
            Debug.Log("[InteractiveAd] Reached end of path. Finishing ad.");
            EndAd();
        }
        else
        {
            currentStepIndex = nextStep;
            ShowCurrentStep();
        }
    }

    private void EndAd()
    {
        Debug.Log("[InteractiveAd] Ad complete.");
        onComplete?.Invoke();
    }
}
