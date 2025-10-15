using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class InteractiveAds : MonoBehaviour
{
    [System.Serializable]
    public class AdStep
    {
        public string stepName;
        public Sprite image;
        public string choiceAText;
        public string choiceBText;
        public int nextStepA = -1; // -1 means this choice ends the ad
        public int nextStepB = -1;
    }

    [Header("UI References")]
    [SerializeField] private Image adImage;
    [SerializeField] private Button choiceAButton;
    [SerializeField] private Button choiceBButton;
    [SerializeField] private TextMeshProUGUI choiceALabel;
    [SerializeField] private TextMeshProUGUI choiceBLabel;

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
        // If the step index is invalid, just end the ad
        if (currentStepIndex < 0 || currentStepIndex >= adSteps.Count)
        {
            Debug.LogWarning("[InteractiveAd] Invalid step index: " + currentStepIndex);
            EndAd();
            return;
        }

        AdStep step = adSteps[currentStepIndex];

        Debug.Log($"[InteractiveAd] Showing step: {step.stepName} (Index {currentStepIndex})");

        // Set image and button texts
        if (adImage) adImage.sprite = step.image;
        if (choiceALabel) choiceALabel.text = step.choiceAText;
        if (choiceBLabel) choiceBLabel.text = step.choiceBText;

        // Set up Choice A button
        if (choiceAButton)
        {
            choiceAButton.onClick.RemoveAllListeners();
            choiceAButton.onClick.AddListener(() => MakeChoice(step.nextStepA, "A"));
        }

        // Set up Choice B button
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
            Debug.Log("[InteractiveAd] no index after this choice, Closing !.");
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
