using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using TMPro;

public class InteractiveAds : MonoBehaviour
{
    [System.Serializable]
    public class AdStep
    {
        public string stepName;
        [TextArea(3, 5)]
        public string dialogueText;
        public string choiceAText;
        public string choiceBText;
        public int nextStepA = -1; // -1 means this choice ends the ad
        public int nextStepB = -1;
    }

    [Header("UI References")]
    [SerializeField] private Canvas adCanvas; // Add this - drag the Canvas that contains the ad UI
    [SerializeField] private TextMeshProUGUI dialogueText;
    [SerializeField] private Button choiceAButton;
    [SerializeField] private Button choiceBButton;
    [SerializeField] private TextMeshProUGUI choiceALabel;
    [SerializeField] private TextMeshProUGUI choiceBLabel;
    


    [Header("Ad Steps")]
    [SerializeField] private List<AdStep> adSteps = new();

    [Header("Win Condition Settings")]
    [SerializeField] private int winStepNumber = 13; // Step number that triggers win condition

    private int currentStepIndex = 0;
    private Action onComplete;
    private InteractiveAdManager adManager;

    void Awake()
    {
        // Find the InteractiveAdManager in the scene
        adManager = FindFirstObjectByType<InteractiveAdManager>();
        if (adManager == null)
        {
            Debug.LogWarning("[InteractiveAd] No InteractiveAdManager found in scene!");
        }
    }

    public void StartInteractiveAd(Action onAdComplete)
    {
        Debug.Log("[InteractiveAd] Starting interactive ad");
        Debug.Log($"[InteractiveAd] Button references - A: {choiceAButton != null}, B: {choiceBButton != null}");

        currentStepIndex = 0;
        onComplete = onAdComplete;

        // Show the ad canvas
        if (adCanvas != null)
        {
            adCanvas.gameObject.SetActive(true);
            Debug.Log("[InteractiveAd] Ad canvas activated");
        }

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

        // Set dialogue and button texts
        if (dialogueText) dialogueText.text = step.dialogueText;
        if (choiceALabel) choiceALabel.text = step.choiceAText;
        if (choiceBLabel) choiceBLabel.text = step.choiceBText;

        // Set up Choice A button
        if (choiceAButton)
        {
            choiceAButton.onClick.RemoveAllListeners();
            choiceAButton.onClick.AddListener(() => MakeChoice(step.nextStepA, "A"));
            Debug.Log($"[InteractiveAd] Choice A button setup for step {currentStepIndex}");
        }
        else
        {
            Debug.LogError("[InteractiveAd] Choice A button is not assigned!");
        }

        // Set up Choice B button
        if (choiceBButton)
        {
            choiceBButton.onClick.RemoveAllListeners();
            choiceBButton.onClick.AddListener(() => MakeChoice(step.nextStepB, "B"));
            Debug.Log($"[InteractiveAd] Choice B button setup for step {currentStepIndex}");
        }
        else
        {
            Debug.LogError("[InteractiveAd] Choice B button is not assigned!");
        }
    }

    private void MakeChoice(int nextStep, string choiceLabel)
    {
        Debug.Log($"[InteractiveAd] Player chose option {choiceLabel} at step {currentStepIndex}");
        Debug.Log($"[InteractiveAd] NextStep value: {nextStep}");

        if (nextStep == -1)
        {
            Debug.Log("[InteractiveAd] NextStep is -1, ending ad.");
            EndAd();
        }
        else if (nextStep == winStepNumber)
        {
            Debug.Log($"[InteractiveAd] Reached win step {winStepNumber}, triggering win condition.");
            if (adManager != null)
            {
                adManager.TriggerWinCondition();
            }
            else
            {
                Debug.LogError("[InteractiveAd] No InteractiveAdManager found to trigger win condition!");
                EndAd(); // Fallback to ending the ad
            }
        }
        else if (nextStep >= 0 && nextStep < adSteps.Count)
        {
            Debug.Log($"[InteractiveAd] Moving from step {currentStepIndex} to step {nextStep}");
            currentStepIndex = nextStep;
            ShowCurrentStep();
        }
        else
        {
            Debug.LogError($"[InteractiveAd] Invalid nextStep: {nextStep}. Valid range is 0 to {adSteps.Count - 1}");
            EndAd();
        }
    }

    private void EndAd()
    {
        Debug.Log("[InteractiveAd] Ad complete.");
        
        // Hide the ad canvas
        if (adCanvas != null)
        {
            adCanvas.gameObject.SetActive(false);
            Debug.Log("[InteractiveAd] Ad canvas deactivated");
        }
        
        onComplete?.Invoke();
    }

    /// <summary>
    /// Hide the ad UI elements (called by InteractiveAdManager when win condition is triggered)
    /// </summary>
    public void HideAdUI()
    {
        if (choiceAButton) choiceAButton.gameObject.SetActive(false);
        if (choiceBButton) choiceBButton.gameObject.SetActive(false);
        if (dialogueText) dialogueText.gameObject.SetActive(false);
        Debug.Log("[InteractiveAd] Ad UI elements hidden for win condition");
    }
}
