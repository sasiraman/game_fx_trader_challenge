using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;

/// <summary>
/// Controller for the results/rewards screen.
/// Displays prediction outcome, credits earned/lost, XP, and badges.
/// </summary>
public class ResultsScreenController : MonoBehaviour
{
    [Header("UI References")]
    public TMP_Text resultText;
    public TMP_Text creditsChangeText;
    public TMP_Text xpText;
    public TMP_Text currentCreditsText;
    public GameObject badgeUnlockedPanel;
    public TMP_Text badgeUnlockedText;
    public Button continueButton;
    public Button leaderboardButton;

    private void Start()
    {
        // Load result data
        bool wasCorrect = PlayerPrefs.GetInt("LastWasCorrect", 0) == 1;
        float creditsChange = PlayerPrefs.GetFloat("LastCreditsChange", 0f);
        int xpEarned = PlayerPrefs.GetInt("LastXP", 0);
        
        // Update UI
        if (resultText != null)
        {
            resultText.text = wasCorrect ? "WIN!" : "LOSS";
            resultText.color = wasCorrect ? Color.green : Color.red;
        }
        
        if (creditsChangeText != null)
        {
            string sign = creditsChange >= 0 ? "+" : "";
            creditsChangeText.text = $"Credits: {sign}{creditsChange:F2}";
            creditsChangeText.color = creditsChange >= 0 ? Color.green : Color.red;
        }
        
        if (xpText != null)
        {
            xpText.text = $"XP Earned: +{xpEarned}";
        }
        
        if (currentCreditsText != null && GameManager.Instance != null)
        {
            currentCreditsText.text = $"Total Credits: {GameManager.Instance.Stats.credits:F2}";
        }
        
        // Check for badge unlock
        if (GameManager.Instance != null && GameManager.Instance.Stats.badges.Count > 0)
        {
            // Get the most recently added badge (last in list)
            string latestBadge = GameManager.Instance.Stats.badges[GameManager.Instance.Stats.badges.Count - 1];
            ShowBadgeUnlocked(latestBadge);
        }
        
        // Setup buttons
        if (continueButton != null)
        {
            continueButton.onClick.AddListener(ContinueToMenu);
        }
        
        if (leaderboardButton != null)
        {
            leaderboardButton.onClick.AddListener(ShowLeaderboard);
        }
        
        // Hide badge panel initially
        if (badgeUnlockedPanel != null)
        {
            badgeUnlockedPanel.SetActive(false);
        }
    }

    /// <summary>
    /// Shows badge unlocked notification.
    /// </summary>
    private void ShowBadgeUnlocked(string badgeName)
    {
        if (badgeUnlockedPanel != null)
        {
            badgeUnlockedPanel.SetActive(true);
        }
        
        if (badgeUnlockedText != null)
        {
            badgeUnlockedText.text = $"Badge Unlocked: {badgeName.Replace("_", " ")}";
        }
    }

    /// <summary>
    /// Continues to main menu.
    /// </summary>
    private void ContinueToMenu()
    {
        SceneManager.LoadScene("MainMenu");
    }

    /// <summary>
    /// Shows leaderboard screen.
    /// </summary>
    private void ShowLeaderboard()
    {
        SceneManager.LoadScene("Leaderboard");
    }
}

