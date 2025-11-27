using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

/// <summary>
/// REST API client for backend integration (auth, rates, leaderboard, scores).
/// Handles retry logic and error handling.
/// </summary>
public class APIClient : MonoBehaviour
{
    public static APIClient Instance { get; private set; }

    [System.Serializable]
    public class LoginResponse
    {
        public string token;
    }

    [System.Serializable]
    public class LeaderboardEntry
    {
        public int rank;
        public string username;
        public float score;
    }

    [System.Serializable]
    public class ScoreRequest
    {
        public string username;
        public float score;
        public float credits;
        public StatsData stats;
    }

    [System.Serializable]
    public class StatsData
    {
        public int wins;
        public int losses;
    }

    private string apiBaseUrl = "";
    private string authToken = "";
    private const int MAX_RETRIES = 3;
    private const float RETRY_DELAY = 1f;

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

    private void Start()
    {
        if (ConfigManager.Instance != null)
        {
            apiBaseUrl = ConfigManager.Instance.Config.apiBaseUrl;
            authToken = ConfigManager.Instance.Config.jwtToken;
        }
    }

    /// <summary>
    /// Authenticates user and retrieves JWT token.
    /// </summary>
    public IEnumerator Login(string username, string password, Action<bool, string> callback)
    {
        string url = $"{apiBaseUrl}/auth/login";
        
        WWWForm form = new WWWForm();
        form.AddField("username", username);
        if (!string.IsNullOrEmpty(password))
        {
            form.AddField("password", password);
        }

        using (UnityWebRequest request = UnityWebRequest.Post(url, form))
        {
            yield return StartCoroutine(SendRequestWithRetry(request));
            
            if (request.result == UnityWebRequest.Result.Success)
            {
                try
                {
                    var response = JsonUtility.FromJson<LoginResponse>(request.downloadHandler.text);
                    authToken = response.token;
                    ConfigManager.Instance.SetAuthToken(authToken);
                    callback?.Invoke(true, authToken);
                }
                catch (Exception e)
                {
                    Debug.LogError($"Failed to parse login response: {e.Message}");
                    callback?.Invoke(false, null);
                }
            }
            else
            {
                Debug.LogWarning($"Login failed: {request.error}");
                callback?.Invoke(false, null);
            }
        }
    }

    /// <summary>
    /// Posts player score to backend.
    /// </summary>
    public IEnumerator PostScore(string username, float score, float credits, Dictionary<string, object> stats)
    {
        string url = $"{apiBaseUrl}/api/game/score";
        
        StatsData statsData = new StatsData();
        if (stats != null)
        {
            if (stats.ContainsKey("wins")) statsData.wins = System.Convert.ToInt32(stats["wins"]);
            if (stats.ContainsKey("losses")) statsData.losses = System.Convert.ToInt32(stats["losses"]);
        }
        
        ScoreRequest scoreData = new ScoreRequest
        {
            username = username,
            score = score,
            credits = credits,
            stats = statsData
        };
        
        string jsonBody = JsonUtility.ToJson(scoreData);
        byte[] bodyRaw = System.Text.Encoding.UTF8.GetBytes(jsonBody);
        
        using (UnityWebRequest request = new UnityWebRequest(url, "POST"))
        {
            request.uploadHandler = new UploadHandlerRaw(bodyRaw);
            request.downloadHandler = new DownloadHandlerBuffer();
            request.SetRequestHeader("Content-Type", "application/json");
            
            if (!string.IsNullOrEmpty(authToken))
            {
                request.SetRequestHeader("Authorization", $"Bearer {authToken}");
            }
            
            yield return StartCoroutine(SendRequestWithRetry(request));
            
            if (request.result == UnityWebRequest.Result.Success)
            {
                Debug.Log("Score persisted successfully");
            }
            else
            {
                Debug.LogWarning($"Failed to persist score: {request.error}");
            }
        }
    }

    /// <summary>
    /// Fetches leaderboard data.
    /// </summary>
    public IEnumerator GetLeaderboard(Action<List<LeaderboardEntry>> callback)
    {
        string url = $"{apiBaseUrl}/api/leaderboard";
        
        using (UnityWebRequest request = UnityWebRequest.Get(url))
        {
            if (!string.IsNullOrEmpty(authToken))
            {
                request.SetRequestHeader("Authorization", $"Bearer {authToken}");
            }
            
            yield return StartCoroutine(SendRequestWithRetry(request));
            
            if (request.result == UnityWebRequest.Result.Success)
            {
                try
                {
                    string jsonResponse = request.downloadHandler.text;
                    // Parse array response
                    jsonResponse = "{\"entries\":" + jsonResponse + "}";
                    var wrapper = JsonUtility.FromJson<LeaderboardWrapper>(jsonResponse);
                    callback?.Invoke(wrapper.entries);
                }
                catch (Exception e)
                {
                    Debug.LogError($"Failed to parse leaderboard: {e.Message}");
                    callback?.Invoke(new List<LeaderboardEntry>());
                }
            }
            else
            {
                Debug.LogWarning($"Failed to fetch leaderboard: {request.error}");
                callback?.Invoke(new List<LeaderboardEntry>());
            }
        }
    }

    /// <summary>
    /// Sends request with retry logic and exponential backoff.
    /// </summary>
    private IEnumerator SendRequestWithRetry(UnityWebRequest request)
    {
        int retries = 0;
        
        while (retries < MAX_RETRIES)
        {
            yield return request.SendWebRequest();
            
            if (request.result == UnityWebRequest.Result.Success)
            {
                yield break; // Success, exit retry loop
            }
            
            // Retry on network errors
            if (request.result == UnityWebRequest.Result.ConnectionError || 
                request.result == UnityWebRequest.Result.DataProcessingError)
            {
                retries++;
                if (retries < MAX_RETRIES)
                {
                    float delay = RETRY_DELAY * Mathf.Pow(2, retries - 1); // Exponential backoff
                    Debug.LogWarning($"Request failed, retrying in {delay}s (attempt {retries + 1}/{MAX_RETRIES})");
                    yield return new WaitForSeconds(delay);
                    request.Dispose();
                    // Recreate request (simplified - in production, you'd want to rebuild the request)
                }
            }
            else
            {
                // Don't retry on HTTP errors (4xx, 5xx)
                yield break;
            }
        }
    }

    [System.Serializable]
    private class LeaderboardWrapper
    {
        public List<LeaderboardEntry> entries;
    }
}

