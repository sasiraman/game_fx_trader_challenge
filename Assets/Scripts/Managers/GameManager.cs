using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Core game manager handling bets, predictions, resolution, credits, XP, and badges.
/// Implements the exact payout formulas specified in requirements.
/// </summary>
public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [System.Serializable]
    public class PlayerStats
    {
        public string username = "guest";
        public float credits = 10000f;
        public int wins = 0;
        public int losses = 0;
        public int currentStreak = 0;
        public int bestStreak = 0;
        public int totalXP = 0;
        public List<string> badges = new List<string>();
    }

    [System.Serializable]
    public class Prediction
    {
        public string pair;
        public bool isBuy; // true = BUY (predicting rate goes up), false = SELL (predicting rate goes down)
        public float betAmount;
        public float rateAtStart;
        public DateTime startTime;
        public float timeHorizonSeconds;
        public DateTime endTime;
        public bool isResolved = false;
        public float? rateAtEnd = null;
        public bool? wasCorrect = null;
        public float? creditsChange = null;
        public int? xpEarned = null;
    }

    public PlayerStats Stats { get; private set; }
    public Prediction CurrentPrediction { get; private set; }
    
    public event Action<PlayerStats> OnStatsUpdated;
    public event Action<Prediction> OnPredictionPlaced;
    public event Action<Prediction> OnPredictionResolved;
    public event Action<string> OnBadgeUnlocked; // badge name

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            InitializeStats();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    /// <summary>
    /// Initializes player stats from saved data or defaults.
    /// </summary>
    private void InitializeStats()
    {
        Stats = new PlayerStats();
        
        // Load from PlayerPrefs (local storage)
        if (PlayerPrefs.HasKey("PlayerCredits"))
        {
            Stats.credits = PlayerPrefs.GetFloat("PlayerCredits");
            Stats.wins = PlayerPrefs.GetInt("PlayerWins");
            Stats.losses = PlayerPrefs.GetInt("PlayerLosses");
            Stats.currentStreak = PlayerPrefs.GetInt("PlayerStreak");
            Stats.bestStreak = PlayerPrefs.GetInt("PlayerBestStreak");
            Stats.totalXP = PlayerPrefs.GetInt("PlayerXP");
            Stats.username = PlayerPrefs.GetString("PlayerUsername", "guest");
        }
        else
        {
            // Use initial credits from config
            if (ConfigManager.Instance != null)
            {
                Stats.credits = ConfigManager.Instance.Config.initialCredits;
            }
            SaveStats();
        }
        
        OnStatsUpdated?.Invoke(Stats);
    }

    /// <summary>
    /// Places a prediction/bet.
    /// </summary>
    public bool PlacePrediction(string pair, bool isBuy, float betAmount, float timeHorizonSeconds)
    {
        // Validate bet
        if (betAmount <= 0 || betAmount > Stats.credits)
        {
            Debug.LogWarning($"Invalid bet amount: {betAmount}. Credits: {Stats.credits}");
            return false;
        }

        if (CurrentPrediction != null && !CurrentPrediction.isResolved)
        {
            Debug.LogWarning("Cannot place new prediction while one is active.");
            return false;
        }

        // Get current rate
        float currentRate = FXFeedManager.Instance.GetCurrentRate(pair);
        if (currentRate <= 0)
        {
            Debug.LogError($"Invalid rate for pair {pair}");
            return false;
        }

        // Deduct bet amount
        Stats.credits -= betAmount;
        SaveStats();

        // Create prediction
        CurrentPrediction = new Prediction
        {
            pair = pair,
            isBuy = isBuy,
            betAmount = betAmount,
            rateAtStart = currentRate,
            startTime = DateTime.UtcNow,
            timeHorizonSeconds = timeHorizonSeconds,
            endTime = DateTime.UtcNow.AddSeconds(timeHorizonSeconds)
        };

        OnPredictionPlaced?.Invoke(CurrentPrediction);
        StartCoroutine(ResolvePredictionAfterDelay(timeHorizonSeconds));
        
        return true;
    }

    /// <summary>
    /// Coroutine that waits for the time horizon and then resolves the prediction.
    /// </summary>
    private IEnumerator ResolvePredictionAfterDelay(float delaySeconds)
    {
        yield return new WaitForSeconds(delaySeconds);
        ResolvePrediction();
    }

    /// <summary>
    /// Resolves the current prediction based on rate change.
    /// </summary>
    private void ResolvePrediction()
    {
        if (CurrentPrediction == null || CurrentPrediction.isResolved)
        {
            return;
        }

        // Get rate at end time
        float rateAtEnd = FXFeedManager.Instance.GetRateAtTime(
            CurrentPrediction.pair, 
            CurrentPrediction.endTime
        );
        
        CurrentPrediction.rateAtEnd = rateAtEnd;
        
        // Calculate percent move
        float percentMove = (rateAtEnd - CurrentPrediction.rateAtStart) / CurrentPrediction.rateAtStart;
        
        // Determine if prediction was correct
        bool wasCorrect = false;
        if (CurrentPrediction.isBuy)
        {
            wasCorrect = percentMove > 0; // BUY wins if rate goes up
        }
        else
        {
            wasCorrect = percentMove < 0; // SELL wins if rate goes down
        }
        
        CurrentPrediction.wasCorrect = wasCorrect;
        
        // Calculate payout/loss
        float creditsChange = 0f;
        int xpEarned = 0;
        
        if (wasCorrect)
        {
            // Payout formula: bet_amount * (1 + min(5, abs(percent_move) * 10))
            float multiplier = 1f + Mathf.Min(5f, Mathf.Abs(percentMove) * 10f);
            creditsChange = CurrentPrediction.betAmount * multiplier;
            Stats.credits += creditsChange;
            Stats.wins++;
            Stats.currentStreak++;
            
            if (Stats.currentStreak > Stats.bestStreak)
            {
                Stats.bestStreak = Stats.currentStreak;
            }
        }
        else
        {
            // Loss: player loses the bet amount
            creditsChange = -CurrentPrediction.betAmount;
            Stats.losses++;
            Stats.currentStreak = 0;
        }
        
        CurrentPrediction.creditsChange = creditsChange;
        
        // Calculate XP: floor(10 * sqrt(abs(percent_move) * 100))
        xpEarned = Mathf.FloorToInt(10f * Mathf.Sqrt(Mathf.Abs(percentMove) * 100f));
        Stats.totalXP += xpEarned;
        CurrentPrediction.xpEarned = xpEarned;
        
        // Round credits to 2 decimal places
        Stats.credits = (float)Math.Round(Stats.credits, 2);
        
        // Check for badge unlocks
        CheckBadgeUnlocks();
        
        CurrentPrediction.isResolved = true;
        SaveStats();
        
        OnPredictionResolved?.Invoke(CurrentPrediction);
        OnStatsUpdated?.Invoke(Stats);
        
        // Persist score to backend if enabled
        if (ConfigManager.Instance.Config.useBackend)
        {
            StartCoroutine(PersistScoreToBackend());
        }
    }

    /// <summary>
    /// Checks and unlocks badges based on achievements.
    /// </summary>
    private void CheckBadgeUnlocks()
    {
        // First Win badge
        if (Stats.wins == 1 && !Stats.badges.Contains("first_win"))
        {
            UnlockBadge("first_win");
        }
        
        // Streak badges
        if (Stats.currentStreak == 3 && !Stats.badges.Contains("streak_3"))
        {
            UnlockBadge("streak_3");
        }
        if (Stats.currentStreak == 5 && !Stats.badges.Contains("streak_5"))
        {
            UnlockBadge("streak_5");
        }
        if (Stats.currentStreak == 10 && !Stats.badges.Contains("streak_10"))
        {
            UnlockBadge("streak_10");
        }
        
        // XP milestones
        if (Stats.totalXP >= 100 && !Stats.badges.Contains("xp_100"))
        {
            UnlockBadge("xp_100");
        }
        if (Stats.totalXP >= 500 && !Stats.badges.Contains("xp_500"))
        {
            UnlockBadge("xp_500");
        }
        if (Stats.totalXP >= 1000 && !Stats.badges.Contains("xp_1000"))
        {
            UnlockBadge("xp_1000");
        }
        
        // Credit milestones
        if (Stats.credits >= 20000 && !Stats.badges.Contains("credits_20k"))
        {
            UnlockBadge("credits_20k");
        }
        if (Stats.credits >= 50000 && !Stats.badges.Contains("credits_50k"))
        {
            UnlockBadge("credits_50k");
        }
    }

    /// <summary>
    /// Unlocks a badge.
    /// </summary>
    private void UnlockBadge(string badgeName)
    {
        if (!Stats.badges.Contains(badgeName))
        {
            Stats.badges.Add(badgeName);
            OnBadgeUnlocked?.Invoke(badgeName);
            Debug.Log($"Badge unlocked: {badgeName}");
        }
    }

    /// <summary>
    /// Saves player stats to local storage.
    /// </summary>
    private void SaveStats()
    {
        PlayerPrefs.SetFloat("PlayerCredits", Stats.credits);
        PlayerPrefs.SetInt("PlayerWins", Stats.wins);
        PlayerPrefs.SetInt("PlayerLosses", Stats.losses);
        PlayerPrefs.SetInt("PlayerStreak", Stats.currentStreak);
        PlayerPrefs.SetInt("PlayerBestStreak", Stats.bestStreak);
        PlayerPrefs.SetInt("PlayerXP", Stats.totalXP);
        PlayerPrefs.SetString("PlayerUsername", Stats.username);
        PlayerPrefs.Save();
    }

    /// <summary>
    /// Persists score to backend API.
    /// </summary>
    private IEnumerator PersistScoreToBackend()
    {
        if (APIClient.Instance != null)
        {
            yield return StartCoroutine(APIClient.Instance.PostScore(
                Stats.username,
                Stats.totalXP,
                Stats.credits,
                new Dictionary<string, object>
                {
                    { "wins", Stats.wins },
                    { "losses", Stats.losses }
                }
            ));
        }
    }

    /// <summary>
    /// Resets player stats (for testing or new game).
    /// </summary>
    public void ResetStats()
    {
        Stats = new PlayerStats();
        if (ConfigManager.Instance != null)
        {
            Stats.credits = ConfigManager.Instance.Config.initialCredits;
        }
        CurrentPrediction = null;
        SaveStats();
        OnStatsUpdated?.Invoke(Stats);
    }
}

