using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

/// <summary>
/// Manages FX rate feeds from either backend API or local mock generator.
/// Supports deterministic mock mode for testing and real-time backend mode.
/// </summary>
public class FXFeedManager : MonoBehaviour
{
    public static FXFeedManager Instance { get; private set; }

    [System.Serializable]
    public class FXRateResponse
    {
        public string timestamp;
        public RatesData rates;
    }

    [System.Serializable]
    public class RatesData
    {
        public float USD_SGD;
        public float USD_INR;
        public float EUR_USD;
        
        public Dictionary<string, float> ToDictionary()
        {
            var dict = new Dictionary<string, float>();
            if (USD_SGD > 0) dict["USD_SGD"] = USD_SGD;
            if (USD_INR > 0) dict["USD_INR"] = USD_INR;
            if (EUR_USD > 0) dict["EUR_USD"] = EUR_USD;
            return dict;
        }
    }

    [System.Serializable]
    public class FXRateData
    {
        public string pair;
        public float rate;
        public string timestamp;
    }

    public event Action<Dictionary<string, float>> OnRatesUpdated;
    public event Action<string, float> OnRateChanged; // pair, newRate

    private Dictionary<string, float> currentRates = new Dictionary<string, float>();
    private Dictionary<string, List<FXRateData>> rateHistory = new Dictionary<string, List<FXRateData>>();
    
    private bool isBackendMode = false;
    private string apiBaseUrl = "";
    private string authToken = "";
    
    // Mock mode variables
    private System.Random mockRNG;
    private Dictionary<string, float> mockBaseRates = new Dictionary<string, float>();
    private Dictionary<string, float> mockCurrentRates = new Dictionary<string, float>();
    private float lastUpdateTime = 0f;
    private const float UPDATE_INTERVAL = 1f; // Update every second in mock mode

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
        InitializeFeed();
    }

    /// <summary>
    /// Initializes the feed based on configuration.
    /// </summary>
    public void InitializeFeed()
    {
        if (ConfigManager.Instance == null)
        {
            Debug.LogError("ConfigManager not found!");
            return;
        }

        var config = ConfigManager.Instance.Config;
        isBackendMode = config.useBackend;
        apiBaseUrl = config.apiBaseUrl;
        authToken = config.jwtToken;

        if (isBackendMode)
        {
            Debug.Log($"FXFeedManager: Backend mode enabled. API: {apiBaseUrl}");
            StartCoroutine(FetchRatesFromBackend());
            InvokeRepeating(nameof(FetchRatesFromBackend), 5f, 5f); // Fetch every 5 seconds
        }
        else
        {
            Debug.Log($"FXFeedManager: Mock mode enabled. Seed: {config.mockSeed}");
            InitializeMockMode(config.mockSeed);
            StartCoroutine(MockRateUpdateLoop());
        }
    }

    /// <summary>
    /// Initializes mock mode with seeded RNG for deterministic behavior.
    /// </summary>
    private void InitializeMockMode(int seed)
    {
        mockRNG = new System.Random(seed);
        
        // Initialize base rates (realistic starting values)
        mockBaseRates["USD_SGD"] = 1.35f;
        mockBaseRates["USD_INR"] = 83.0f;
        mockBaseRates["EUR_USD"] = 1.09f;
        
        // Set current rates to base rates
        foreach (var pair in mockBaseRates.Keys)
        {
            mockCurrentRates[pair] = mockBaseRates[pair];
            currentRates[pair] = mockBaseRates[pair];
            rateHistory[pair] = new List<FXRateData>();
            AddToHistory(pair, mockBaseRates[pair]);
        }
        
        lastUpdateTime = Time.time;
    }

    /// <summary>
    /// Coroutine that updates mock rates periodically with realistic drift and occasional spikes.
    /// </summary>
    private IEnumerator MockRateUpdateLoop()
    {
        while (!isBackendMode)
        {
            yield return new WaitForSeconds(UPDATE_INTERVAL);
            UpdateMockRates();
        }
    }

    /// <summary>
    /// Updates mock rates with deterministic drift and volatility.
    /// </summary>
    private void UpdateMockRates()
    {
        foreach (var pair in mockCurrentRates.Keys)
        {
            float currentRate = mockCurrentRates[pair];
            
            // Generate drift: small random walk (0.01% to 0.05% per update)
            float drift = (float)(mockRNG.NextDouble() - 0.5) * 0.0005f;
            
            // Occasional spikes (5% chance of larger move)
            if (mockRNG.NextDouble() < 0.05f)
            {
                drift *= 5f; // Larger move
            }
            
            float newRate = currentRate * (1f + drift);
            
            // Clamp to reasonable bounds (±10% from base)
            float baseRate = mockBaseRates[pair];
            newRate = Mathf.Clamp(newRate, baseRate * 0.9f, baseRate * 1.1f);
            
            mockCurrentRates[pair] = newRate;
            currentRates[pair] = newRate;
            
            AddToHistory(pair, newRate);
            OnRateChanged?.Invoke(pair, newRate);
        }
        
        OnRatesUpdated?.Invoke(new Dictionary<string, float>(currentRates));
    }

    /// <summary>
    /// Fetches rates from backend API.
    /// </summary>
    private IEnumerator FetchRatesFromBackend()
    {
        string url = $"{apiBaseUrl}/api/fx_rates";
        
        using (UnityWebRequest request = UnityWebRequest.Get(url))
        {
            if (!string.IsNullOrEmpty(authToken))
            {
                request.SetRequestHeader("Authorization", $"Bearer {authToken}");
            }
            
            yield return request.SendWebRequest();
            
            if (request.result == UnityWebRequest.Result.Success)
            {
                try
                {
                    string jsonResponse = request.downloadHandler.text;
                    var response = JsonUtility.FromJson<FXRateResponse>(jsonResponse);
                    
                    // Parse rates dictionary
                    currentRates.Clear();
                    var ratesDict = response.rates.ToDictionary();
                    foreach (var kvp in ratesDict)
                    {
                        currentRates[kvp.Key] = kvp.Value;
                        AddToHistory(kvp.Key, kvp.Value);
                        OnRateChanged?.Invoke(kvp.Key, kvp.Value);
                    }
                    
                    OnRatesUpdated?.Invoke(new Dictionary<string, float>(currentRates));
                }
                catch (Exception e)
                {
                    Debug.LogError($"Failed to parse FX rates response: {e.Message}");
                }
            }
            else
            {
                Debug.LogWarning($"Failed to fetch FX rates: {request.error}. Falling back to mock mode.");
                // Fallback to mock mode on error
                if (isBackendMode)
                {
                    isBackendMode = false;
                    InitializeMockMode(ConfigManager.Instance.Config.mockSeed);
                    StartCoroutine(MockRateUpdateLoop());
                }
            }
        }
    }

    /// <summary>
    /// Gets current rate for a currency pair.
    /// </summary>
    public float GetCurrentRate(string pair)
    {
        if (currentRates.ContainsKey(pair))
        {
            return currentRates[pair];
        }
        return 0f;
    }

    /// <summary>
    /// Gets rate history for a currency pair (last N points).
    /// </summary>
    public List<FXRateData> GetRateHistory(string pair, int maxPoints = 100)
    {
        if (rateHistory.ContainsKey(pair))
        {
            var history = rateHistory[pair];
            int startIndex = Mathf.Max(0, history.Count - maxPoints);
            return history.GetRange(startIndex, history.Count - startIndex);
        }
        return new List<FXRateData>();
    }

    /// <summary>
    /// Adds a rate point to history, maintaining max history size.
    /// </summary>
    private void AddToHistory(string pair, float rate)
    {
        if (!rateHistory.ContainsKey(pair))
        {
            rateHistory[pair] = new List<FXRateData>();
        }
        
        var data = new FXRateData
        {
            pair = pair,
            rate = rate,
            timestamp = DateTime.UtcNow.ToString("yyyy-MM-ddTHH:mm:ssZ")
        };
        
        rateHistory[pair].Add(data);
        
        // Keep only last 1000 points
        if (rateHistory[pair].Count > 1000)
        {
            rateHistory[pair].RemoveAt(0);
        }
    }

    /// <summary>
    /// Gets historical rate at a specific time (for prediction resolution).
    /// </summary>
    public float GetRateAtTime(string pair, DateTime targetTime)
    {
        if (!rateHistory.ContainsKey(pair))
        {
            return GetCurrentRate(pair);
        }
        
        var history = rateHistory[pair];
        if (history.Count == 0)
        {
            return GetCurrentRate(pair);
        }
        
        // Find closest rate to target time
        FXRateData closest = history[0];
        long minDiff = long.MaxValue;
        
        foreach (var data in history)
        {
            if (DateTime.TryParse(data.timestamp, out DateTime dataTime))
            {
                long diff = Math.Abs((targetTime.Ticks - dataTime.Ticks));
                if (diff < minDiff)
                {
                    minDiff = diff;
                    closest = data;
                }
            }
        }
        
        return closest.rate;
    }

    /// <summary>
    /// Forces a refresh of rates (useful for testing or manual refresh).
    /// </summary>
    public void RefreshRates()
    {
        if (isBackendMode)
        {
            StartCoroutine(FetchRatesFromBackend());
        }
    }
}

