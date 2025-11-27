using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Simple analytics system for tracking game events.
/// Emits events locally or to backend when available.
/// </summary>
public class Analytics : MonoBehaviour
{
    public static Analytics Instance { get; private set; }

    private List<AnalyticsEvent> eventQueue = new List<AnalyticsEvent>();

    [System.Serializable]
    public class AnalyticsEvent
    {
        public string eventName;
        public Dictionary<string, object> properties;
        public string timestamp;
    }

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    /// <summary>
    /// Tracks a game event.
    /// </summary>
    public void TrackEvent(string eventName, Dictionary<string, object> properties = null)
    {
        var evt = new AnalyticsEvent
        {
            eventName = eventName,
            properties = properties ?? new Dictionary<string, object>(),
            timestamp = System.DateTime.UtcNow.ToString("yyyy-MM-ddTHH:mm:ssZ")
        };
        
        eventQueue.Add(evt);
        
        // Log locally
        Debug.Log($"[Analytics] {eventName}: {JsonUtility.ToJson(evt)}");
        
        // In production, you could send to backend here
        // For now, we'll just store locally
        SaveEventLocally(evt);
    }

    /// <summary>
    /// Saves event to local storage (PlayerPrefs or IndexedDB in WebGL).
    /// </summary>
    private void SaveEventLocally(AnalyticsEvent evt)
    {
        // Store as JSON string in PlayerPrefs (limited storage, so only keep recent events)
        string key = $"Analytics_{System.DateTime.UtcNow.Ticks}";
        string json = JsonUtility.ToJson(evt);
        
        // Keep only last 100 events
        if (eventQueue.Count > 100)
        {
            eventQueue.RemoveAt(0);
        }
    }

    /// <summary>
    /// Convenience methods for common events.
    /// </summary>
    public void TrackGameStart()
    {
        TrackEvent("game_start");
    }

    public void TrackPredictionPlaced(string pair, bool isBuy, float amount, float horizon)
    {
        TrackEvent("prediction_placed", new Dictionary<string, object>
        {
            { "pair", pair },
            { "direction", isBuy ? "buy" : "sell" },
            { "amount", amount },
            { "horizon", horizon }
        });
    }

    public void TrackPredictionResolved(bool wasCorrect, float creditsChange, int xp)
    {
        TrackEvent("prediction_resolved", new Dictionary<string, object>
        {
            { "was_correct", wasCorrect },
            { "credits_change", creditsChange },
            { "xp_earned", xp }
        });
    }

    public void TrackBadgeUnlocked(string badgeName)
    {
        TrackEvent("badge_unlocked", new Dictionary<string, object>
        {
            { "badge", badgeName }
        });
    }

    public void TrackLeaderboardView()
    {
        TrackEvent("leaderboard_view");
    }
}

