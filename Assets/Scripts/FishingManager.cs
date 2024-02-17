using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class FishingManager : MonoBehaviour
{
    public static FishingManager instance;

    private float totalAttempts = 0;
    private float successfulCatches = 0;
    [SerializeField] private float desiredRTP = 0.3f; // Desired Return to Player (RTP) rate
    private float originalCatchProbability = 0.3f;

    [SerializeField] private TextMeshProUGUI totalAttemptsTxt, successfulAttemptsTxt, rateTxt;
    [SerializeField] private TextMeshProUGUI caughtFishTypesTxt;
    private List<Fish.FishType> caughtFishTypes = new List<Fish.FishType>();

    private void Awake()
    {
        instance = this;
    }

    // Update the total attempts counter and check for successful catches
    public bool AttemptToCatchFish(Fish.FishType fishType)
    {
        totalAttempts++;
        bool isCought = false;

        // Generate a random number between 0 and 1
        float randomValue = Random.value;

        // Check if the random value falls within the desired RTP range
        if (randomValue < desiredRTP)
        {
            successfulCatches++;
            Debug.Log("Fish caught!");
            isCought = true;
            caughtFishTypes.Add(fishType);
            UpdateFishingLog();
        }
        else
        {
            Debug.Log("No fish caught.");
            isCought = false;
        }

        // Check if it's time to ensure the desired RTP rate
        if (totalAttempts % 10 == 0)
        {
            EnsureDesiredRTP();
        }

        // Update UI counters
        UpdateCountersUI();

        return isCought;
    }

    // Ensure that the current RTP meets the desired rate
    private void EnsureDesiredRTP()
    {
        float currentRTP = (float)successfulCatches / totalAttempts;

        // If current RTP is lower than desired RTP, adjust catch probability
        if (currentRTP < desiredRTP)
        {
            // Calculate the adjustment factor to maintain the desired RTP
            float adjustmentFactor = desiredRTP / currentRTP;

            // Adjust the catch probability for subsequent attempts
            AdjustCatchProbability(adjustmentFactor);
        }
    }

    private void AdjustCatchProbability(float adjustmentFactor)
    {
        // Adjust the catch probability based on the adjustment factor
        // For example, if the original catch probability was 0.3 (30%),
        // multiply it by the adjustment factor to maintain the desired RTP
        // Ensure that the adjusted catch probability is capped between 0 and 1
        originalCatchProbability *= adjustmentFactor;
        originalCatchProbability = Mathf.Clamp01(originalCatchProbability);

        Debug.Log("Adjusted catch probability: " + originalCatchProbability);
    }

    // Update UI counters
    private void UpdateCountersUI()
    {
        // Update UI to display successfulCatches and totalAttempts
        totalAttemptsTxt.text = totalAttempts.ToString();
        successfulAttemptsTxt.text = successfulCatches.ToString();
        float rate = (successfulCatches / totalAttempts) * 100;
        rateTxt.text = rate.ToString();
    }

    private void UpdateFishingLog()
    {
        int maxDisplayCount = 10;
        int startIndex = Mathf.Max(caughtFishTypes.Count - maxDisplayCount, 0);
        List<Fish.FishType> last10FishTypes = caughtFishTypes.GetRange(startIndex, Mathf.Min(maxDisplayCount, caughtFishTypes.Count - startIndex));

        string displayText = "";
        foreach (Fish.FishType fishType in last10FishTypes)
        {
            displayText += fishType.ToString() + " | ";
        }
        caughtFishTypesTxt.text = displayText;
    }
}