using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;

/// <summary>
/// Controller for the gameplay screen.
/// Handles bet placement, timer, chart display, and prediction resolution.
/// </summary>
public class GamePlayController : MonoBehaviour
{
    [Header("UI References")]
    public TMP_Text pairLabel;
    public TMP_Text currentRateText;
    public TMP_Text creditsText;
    public ChartRenderer chartRenderer;
    public Button buyButton;
    public Button sellButton;
    public Slider betAmountSlider;
    public TMP_Text betAmountText;
    public TMP_Dropdown timeHorizonDropdown;
    public TMP_Text timerText;
    public GameObject predictionPanel;
    public TMP_Text predictionStatusText;
    public Button backButton;

    private string currentPair = "";
    private float currentRate = 0f;
    private bool predictionActive = false;
    private float timeRemaining = 0f;

    private void Start()
    {
        // Load selected corridor
        currentPair = PlayerPrefs.GetString("SelectedCorridor", "USD_SGD");
        
        if (pairLabel != null)
        {
            pairLabel.text = currentPair.Replace("_", " → ");
        }
        
        // Setup UI
        if (betAmountSlider != null)
        {
            betAmountSlider.minValue = 10f;
            if (GameManager.Instance != null)
            {
                betAmountSlider.maxValue = GameManager.Instance.Stats.credits;
            }
            betAmountSlider.value = betAmountSlider.minValue;
            betAmountSlider.onValueChanged.AddListener(OnBetAmountChanged);
        }
        
        if (buyButton != null)
        {
            buyButton.onClick.AddListener(() => PlaceBet(true));
        }
        
        if (sellButton != null)
        {
            sellButton.onClick.AddListener(() => PlaceBet(false));
        }
        
        if (backButton != null)
        {
            backButton.onClick.AddListener(GoBackToMenu);
        }
        
        // Setup time horizon dropdown
        if (timeHorizonDropdown != null)
        {
            timeHorizonDropdown.ClearOptions();
            timeHorizonDropdown.AddOptions(new System.Collections.Generic.List<string>
            {
                "30 seconds",
                "60 seconds",
                "5 minutes"
            });
        }
        
        // Initialize chart
        if (chartRenderer != null)
        {
            chartRenderer.SetCurrencyPair(currentPair);
        }
        
        // Subscribe to events
        if (FXFeedManager.Instance != null)
        {
            FXFeedManager.Instance.OnRateChanged += OnRateUpdated;
        }
        
        if (GameManager.Instance != null)
        {
            GameManager.Instance.OnPredictionPlaced += OnPredictionPlaced;
            GameManager.Instance.OnPredictionResolved += OnPredictionResolved;
            GameManager.Instance.OnStatsUpdated += OnStatsUpdated;
        }
        
        // Update initial UI
        UpdateRateDisplay();
        UpdateCreditsDisplay();
        
        // Hide prediction panel initially
        if (predictionPanel != null)
        {
            predictionPanel.SetActive(false);
        }
    }

    private void OnDestroy()
    {
        if (FXFeedManager.Instance != null)
        {
            FXFeedManager.Instance.OnRateChanged -= OnRateUpdated;
        }
        
        if (GameManager.Instance != null)
        {
            GameManager.Instance.OnPredictionPlaced -= OnPredictionPlaced;
            GameManager.Instance.OnPredictionResolved -= OnPredictionResolved;
            GameManager.Instance.OnStatsUpdated -= OnStatsUpdated;
        }
    }

    private void Update()
    {
        if (predictionActive && timeRemaining > 0)
        {
            timeRemaining -= Time.deltaTime;
            UpdateTimerDisplay();
            
            if (timeRemaining <= 0)
            {
                timeRemaining = 0;
                predictionActive = false;
            }
        }
    }

    /// <summary>
    /// Called when FX rate updates.
    /// </summary>
    private void OnRateUpdated(string pair, float rate)
    {
        if (pair == currentPair)
        {
            currentRate = rate;
            UpdateRateDisplay();
        }
    }

    /// <summary>
    /// Updates the current rate display.
    /// </summary>
    private void UpdateRateDisplay()
    {
        if (currentRateText != null)
        {
            currentRate = FXFeedManager.Instance.GetCurrentRate(currentPair);
            currentRateText.text = $"Current Rate: {currentRate:F4}";
        }
    }

    /// <summary>
    /// Updates credits display.
    /// </summary>
    private void OnStatsUpdated(GameManager.PlayerStats stats)
    {
        UpdateCreditsDisplay();
    }

    private void UpdateCreditsDisplay()
    {
        if (creditsText != null && GameManager.Instance != null)
        {
            creditsText.text = $"Credits: {GameManager.Instance.Stats.credits:F2}";
        }
    }

    /// <summary>
    /// Called when bet amount slider changes.
    /// </summary>
    private void OnBetAmountChanged(float value)
    {
        if (betAmountText != null)
        {
            betAmountText.text = $"Bet Amount: {value:F2}";
        }
    }

    /// <summary>
    /// Places a bet (BUY or SELL).
    /// </summary>
    private void PlaceBet(bool isBuy)
    {
        if (predictionActive)
        {
            Debug.LogWarning("Prediction already active!");
            return;
        }
        
        if (GameManager.Instance == null)
        {
            Debug.LogError("GameManager not found!");
            return;
        }
        
        float betAmount = betAmountSlider != null ? betAmountSlider.value : 10f;
        float timeHorizon = GetSelectedTimeHorizon();
        
        bool success = GameManager.Instance.PlacePrediction(currentPair, isBuy, betAmount, timeHorizon);
        
        if (success)
        {
            // Disable betting controls
            if (buyButton != null) buyButton.interactable = false;
            if (sellButton != null) sellButton.interactable = false;
            if (betAmountSlider != null) betAmountSlider.interactable = false;
        }
    }

    /// <summary>
    /// Gets selected time horizon in seconds.
    /// </summary>
    private float GetSelectedTimeHorizon()
    {
        if (timeHorizonDropdown != null)
        {
            int index = timeHorizonDropdown.value;
            switch (index)
            {
                case 0: return 30f;
                case 1: return 60f;
                case 2: return 300f; // 5 minutes
            }
        }
        return 30f; // Default
    }

    /// <summary>
    /// Called when prediction is placed.
    /// </summary>
    private void OnPredictionPlaced(GameManager.Prediction prediction)
    {
        predictionActive = true;
        timeRemaining = prediction.timeHorizonSeconds;
        
        if (predictionPanel != null)
        {
            predictionPanel.SetActive(true);
        }
        
        UpdatePredictionStatus();
    }

    /// <summary>
    /// Called when prediction is resolved.
    /// </summary>
    private void OnPredictionResolved(GameManager.Prediction prediction)
    {
        predictionActive = false;
        
        // Show result
        if (predictionStatusText != null)
        {
            string result = prediction.wasCorrect.Value ? "WIN!" : "LOSS";
            string creditsChange = prediction.creditsChange.Value >= 0 ? 
                $"+{prediction.creditsChange.Value:F2}" : 
                $"{prediction.creditsChange.Value:F2}";
            
            predictionStatusText.text = $"{result}\nCredits: {creditsChange}\nXP: +{prediction.xpEarned.Value}";
        }
        
        // Re-enable betting controls after a delay
        StartCoroutine(ReenableControlsAfterDelay(3f));
        
        // Navigate to results screen after delay
        StartCoroutine(NavigateToResultsAfterDelay(5f, prediction));
    }

    /// <summary>
    /// Updates prediction status display.
    /// </summary>
    private void UpdatePredictionStatus()
    {
        if (predictionStatusText != null && GameManager.Instance != null && GameManager.Instance.CurrentPrediction != null)
        {
            var pred = GameManager.Instance.CurrentPrediction;
            string direction = pred.isBuy ? "BUY" : "SELL";
            predictionStatusText.text = $"Prediction: {direction}\nRate: {pred.rateAtStart:F4}\nWaiting...";
        }
    }

    /// <summary>
    /// Updates timer display.
    /// </summary>
    private void UpdateTimerDisplay()
    {
        if (timerText != null)
        {
            int minutes = Mathf.FloorToInt(timeRemaining / 60f);
            int seconds = Mathf.FloorToInt(timeRemaining % 60f);
            timerText.text = $"Time Remaining: {minutes:00}:{seconds:00}";
        }
    }

    /// <summary>
    /// Re-enables betting controls after delay.
    /// </summary>
    private IEnumerator ReenableControlsAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        
        if (buyButton != null) buyButton.interactable = true;
        if (sellButton != null) sellButton.interactable = true;
        if (betAmountSlider != null) betAmountSlider.interactable = true;
    }

    /// <summary>
    /// Navigates to results screen after delay.
    /// </summary>
    private IEnumerator NavigateToResultsAfterDelay(float delay, GameManager.Prediction prediction)
    {
        yield return new WaitForSeconds(delay);
        
        // Store prediction result for results screen
        PlayerPrefs.SetFloat("LastCreditsChange", prediction.creditsChange.Value);
        PlayerPrefs.SetInt("LastXP", prediction.xpEarned.Value);
        PlayerPrefs.SetInt("LastWasCorrect", prediction.wasCorrect.Value ? 1 : 0);
        
        SceneManager.LoadScene("Results");
    }

    /// <summary>
    /// Returns to main menu.
    /// </summary>
    private void GoBackToMenu()
    {
        SceneManager.LoadScene("MainMenu");
    }
}

