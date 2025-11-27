using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;

/// <summary>
/// Controller for the main menu screen.
/// Displays credits, badges, corridors, and navigation options.
/// </summary>
public class MainMenuController : MonoBehaviour
{
    [Header("UI References")]
    public TMP_Text creditsText;
    public TMP_Text usernameText;
    public TMP_Text winsText;
    public TMP_Text lossesText;
    public TMP_Text streakText;
    public TMP_Text xpText;
    public Transform corridorButtonParent;
    public GameObject corridorButtonPrefab;
    public Button leaderboardButton;
    public Button playButton;
    public GameObject badgesPanel;
    public Transform badgesContainer;

    private string selectedCorridor = "";

    private void Start()
    {
        // Setup button listeners
        if (leaderboardButton != null)
        {
            leaderboardButton.onClick.AddListener(ShowLeaderboard);
        }
        
        if (playButton != null)
        {
            playButton.onClick.AddListener(StartGame);
        }
        
        // Subscribe to stats updates
        if (GameManager.Instance != null)
        {
            GameManager.Instance.OnStatsUpdated += UpdateUI;
            UpdateUI(GameManager.Instance.Stats);
        }
        
        // Load available corridors
        LoadCorridors();
        
        // Load badges
        UpdateBadges();
    }

    private void OnDestroy()
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.OnStatsUpdated -= UpdateUI;
        }
    }

    /// <summary>
    /// Updates UI with current player stats.
    /// </summary>
    private void UpdateUI(GameManager.PlayerStats stats)
    {
        if (creditsText != null)
        {
            creditsText.text = $"Credits: {stats.credits:F2}";
        }
        
        if (usernameText != null)
        {
            usernameText.text = stats.username;
        }
        
        if (winsText != null)
        {
            winsText.text = $"Wins: {stats.wins}";
        }
        
        if (lossesText != null)
        {
            lossesText.text = $"Losses: {stats.losses}";
        }
        
        if (streakText != null)
        {
            streakText.text = $"Streak: {stats.currentStreak} (Best: {stats.bestStreak})";
        }
        
        if (xpText != null)
        {
            xpText.text = $"XP: {stats.totalXP}";
        }
        
        UpdateBadges();
    }

    /// <summary>
    /// Loads available currency corridors and creates buttons.
    /// </summary>
    private void LoadCorridors()
    {
        if (corridorButtonParent == null || corridorButtonPrefab == null)
        {
            return;
        }
        
        string[] corridors = new string[] { "USD_SGD", "USD_INR", "EUR_USD" };
        
        if (ConfigManager.Instance != null)
        {
            corridors = ConfigManager.Instance.Config.availableCorridors;
        }
        
        foreach (string corridor in corridors)
        {
            GameObject buttonObj = Instantiate(corridorButtonPrefab, corridorButtonParent);
            Button button = buttonObj.GetComponent<Button>();
            TMP_Text label = buttonObj.GetComponentInChildren<TMP_Text>();
            
            if (label != null)
            {
                label.text = corridor.Replace("_", " → ");
            }
            
            string corridorCopy = corridor; // Capture for closure
            button.onClick.AddListener(() => SelectCorridor(corridorCopy));
        }
    }

    /// <summary>
    /// Selects a currency corridor.
    /// </summary>
    private void SelectCorridor(string corridor)
    {
        selectedCorridor = corridor;
        Debug.Log($"Selected corridor: {corridor}");
        
        // Update button visuals (highlight selected)
        // Implementation: update button colors/interactable states
    }

    /// <summary>
    /// Starts the game with selected corridor.
    /// </summary>
    private void StartGame()
    {
        if (string.IsNullOrEmpty(selectedCorridor))
        {
            Debug.LogWarning("Please select a currency corridor first.");
            return;
        }
        
        // Store selected corridor in a static variable or PlayerPrefs
        PlayerPrefs.SetString("SelectedCorridor", selectedCorridor);
        SceneManager.LoadScene("GamePlay");
    }

    /// <summary>
    /// Shows leaderboard screen.
    /// </summary>
    private void ShowLeaderboard()
    {
        SceneManager.LoadScene("Leaderboard");
    }

    /// <summary>
    /// Updates badges display.
    /// </summary>
    private void UpdateBadges()
    {
        if (badgesContainer == null || GameManager.Instance == null)
        {
            return;
        }
        
        // Clear existing badges
        foreach (Transform child in badgesContainer)
        {
            Destroy(child.gameObject);
        }
        
        // Create badge UI elements
        foreach (string badge in GameManager.Instance.Stats.badges)
        {
            GameObject badgeObj = new GameObject($"Badge_{badge}");
            badgeObj.transform.SetParent(badgesContainer, false);
            
            Image image = badgeObj.AddComponent<Image>();
            image.color = Color.yellow; // Placeholder
            
            TMP_Text label = badgeObj.AddComponent<TextMeshProUGUI>();
            label.text = badge.Replace("_", " ");
            label.fontSize = 12;
            label.alignment = TextAlignmentOptions.Center;
        }
    }
}

