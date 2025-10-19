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
        public Sprite image;
        public string choiceAText;
        public string choiceBText;
        public int nextStepA = -1; // -1 means this choice ends the ad
        public int nextStepB = -1;
    }

    [Header("UI References")]
    [SerializeField] private Canvas adCanvas; // Add this - drag the Canvas that contains the ad UI
    [SerializeField] private Image adImage;
    [SerializeField] private Button choiceAButton;
    [SerializeField] private Button choiceBButton;
    [SerializeField] private TextMeshProUGUI choiceALabel;
    [SerializeField] private TextMeshProUGUI choiceBLabel;

    [Header("Ad Steps")]
    [SerializeField] private List<AdStep> adSteps = new();

    [Header("Hotkey Support")]
    [SerializeField] private bool hotkeysEnabled = true;
    [SerializeField] private KeyCode hotkeyChoiceA = KeyCode.H; // press H -> Choice A
    [SerializeField] private KeyCode hotkeyChoiceB = KeyCode.J; // press J -> Choice B

    private int currentStepIndex = 0;
    private Action onComplete;

    [Header("Inspector Callbacks (optional)")]
    [SerializeField] private UnityEvent onChoiceA; // Optional: assign inspector callbacks for Choice A
    [SerializeField] private UnityEvent onChoiceB; // Optional: assign inspector callbacks for Choice B

    // Test method - you can call this from inspector or another script
    public void TestButtonA()
    {
        Debug.Log("[InteractiveAd] TEST - Button A clicked!");
    }

    public void TestButtonB()
    {
        Debug.Log("[InteractiveAd] TEST - Button B clicked!");
    }

    private void Update()
    {
        if (!hotkeysEnabled) return;

        // Check if H is pressed and ad is not showing - start the ad
        if (Input.GetKeyDown(hotkeyChoiceA) && adCanvas != null && !adCanvas.gameObject.activeSelf)
        {
            Debug.Log("[InteractiveAd] Hotkey H -> Starting Interactive Ad");
            StartInteractiveAd(null);
            return; // prevent also firing the click on the same frame
        }

        // Check if H is pressed and ad is showing - trigger Choice A
        if (Input.GetKeyDown(hotkeyChoiceA))
        {
            // Simulate clicking the Choice A button
            if (choiceAButton != null && choiceAButton.interactable)
            {
                Debug.Log("[InteractiveAd] Hotkey H -> Choice A");
                choiceAButton.onClick.Invoke();
            }
        }

        // Check if J is pressed - trigger Choice B
        if (Input.GetKeyDown(hotkeyChoiceB))
        {
            if (choiceBButton != null && choiceBButton.interactable)
            {
                Debug.Log("[InteractiveAd] Hotkey J -> Choice B");
                choiceBButton.onClick.Invoke();
            }
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

        // Set image and button texts
        if (adImage) adImage.sprite = step.image;
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

        // Fire optional inspector callbacks so designers can hook behavior without code
        if (choiceLabel == "A")
        {
            try { onChoiceA?.Invoke(); } catch (Exception ex) { Debug.LogWarning($"onChoiceA invoke failed: {ex.Message}"); }
        }
        else if (choiceLabel == "B")
        {
            try { onChoiceB?.Invoke(); } catch (Exception ex) { Debug.LogWarning($"onChoiceB invoke failed: {ex.Message}"); }
        }

        if (nextStep == -1)
        {
            Debug.Log("[InteractiveAd] NextStep is -1, ending ad.");
            EndAd();
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
}
