using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;

/// <summary>
/// Controller for the leaderboard screen.
/// Displays top players and user's rank.
/// </summary>
public class LeaderboardController : MonoBehaviour
{
    [Header("UI References")]
    public Transform leaderboardContainer;
    public GameObject leaderboardEntryPrefab;
    public TMP_Text playerRankText;
    public Button refreshButton;
    public Button backButton;

    private void Start()
    {
        if (refreshButton != null)
        {
            refreshButton.onClick.AddListener(RefreshLeaderboard);
        }
        
        if (backButton != null)
        {
            backButton.onClick.AddListener(GoBackToMenu);
        }
        
        LoadLeaderboard();
    }

    /// <summary>
    /// Loads leaderboard data from backend or shows mock data.
    /// </summary>
    private void LoadLeaderboard()
    {
        if (ConfigManager.Instance != null && ConfigManager.Instance.Config.useBackend && APIClient.Instance != null)
        {
            StartCoroutine(LoadLeaderboardFromBackend());
        }
        else
        {
            ShowMockLeaderboard();
        }
    }

    /// <summary>
    /// Loads leaderboard from backend API.
    /// </summary>
    private IEnumerator LoadLeaderboardFromBackend()
    {
        List<APIClient.LeaderboardEntry> entries = null;
        
        yield return StartCoroutine(APIClient.Instance.GetLeaderboard((e) =>
        {
            entries = e;
        }));
        
        if (entries != null && entries.Count > 0)
        {
            DisplayLeaderboard(entries);
        }
        else
        {
            ShowMockLeaderboard();
        }
    }

    /// <summary>
    /// Shows mock leaderboard data (for offline/mock mode).
    /// </summary>
    private void ShowMockLeaderboard()
    {
        var mockEntries = new List<APIClient.LeaderboardEntry>
        {
            new APIClient.LeaderboardEntry { rank = 1, username = "alice", score = 12500 },
            new APIClient.LeaderboardEntry { rank = 2, username = "bob", score = 11000 },
            new APIClient.LeaderboardEntry { rank = 3, username = "charlie", score = 9500 },
            new APIClient.LeaderboardEntry { rank = 4, username = "diana", score = 8200 },
            new APIClient.LeaderboardEntry { rank = 5, username = "eve", score = 7500 }
        };
        
        // Add current player if available
        if (GameManager.Instance != null)
        {
            mockEntries.Add(new APIClient.LeaderboardEntry
            {
                rank = mockEntries.Count + 1,
                username = GameManager.Instance.Stats.username,
                score = GameManager.Instance.Stats.credits
            });
        }
        
        DisplayLeaderboard(mockEntries);
    }

    /// <summary>
    /// Displays leaderboard entries in UI.
    /// </summary>
    private void DisplayLeaderboard(List<APIClient.LeaderboardEntry> entries)
    {
        if (leaderboardContainer == null)
        {
            return;
        }
        
        // Clear existing entries
        foreach (Transform child in leaderboardContainer)
        {
            Destroy(child.gameObject);
        }
        
        // Create entry UI elements
        foreach (var entry in entries)
        {
            GameObject entryObj;
            
            if (leaderboardEntryPrefab != null)
            {
                entryObj = Instantiate(leaderboardEntryPrefab, leaderboardContainer);
            }
            else
            {
                entryObj = new GameObject($"Entry_{entry.rank}");
                entryObj.transform.SetParent(leaderboardContainer, false);
                entryObj.AddComponent<RectTransform>();
            }
            
            // Set entry text
            TMP_Text[] texts = entryObj.GetComponentsInChildren<TMP_Text>();
            if (texts.Length >= 3)
            {
                texts[0].text = $"#{entry.rank}";
                texts[1].text = entry.username;
                texts[2].text = entry.score.ToString("F2");
            }
            else
            {
                // Create text components if prefab doesn't have them
                TMP_Text rankText = entryObj.AddComponent<TextMeshProUGUI>();
                rankText.text = $"#{entry.rank} {entry.username} - {entry.score:F2}";
            }
            
            // Highlight current player
            if (GameManager.Instance != null && entry.username == GameManager.Instance.Stats.username)
            {
                Image img = entryObj.GetComponent<Image>();
                if (img == null) img = entryObj.AddComponent<Image>();
                img.color = new Color(1f, 1f, 0f, 0.3f); // Yellow highlight
                
                if (playerRankText != null)
                {
                    playerRankText.text = $"Your Rank: #{entry.rank}";
                }
            }
        }
    }

    /// <summary>
    /// Refreshes leaderboard data.
    /// </summary>
    private void RefreshLeaderboard()
    {
        LoadLeaderboard();
    }

    /// <summary>
    /// Returns to main menu.
    /// </summary>
    private void GoBackToMenu()
    {
        SceneManager.LoadScene("MainMenu");
    }
}

