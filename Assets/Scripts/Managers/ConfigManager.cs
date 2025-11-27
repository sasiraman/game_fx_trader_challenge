using System;
using System.IO;
using UnityEngine;

/// <summary>
/// Manages game configuration loaded from StreamingAssets/config.json or index.html config panel.
/// Supports both mock and backend API modes.
/// </summary>
public class ConfigManager : MonoBehaviour
{
    public static ConfigManager Instance { get; private set; }

    [System.Serializable]
    public class GameConfig
    {
        public bool useBackend = false;
        public string apiBaseUrl = "http://localhost:3000";
        public string jwtToken = "";
        public int mockSeed = 12345;
        public float initialCredits = 10000f;
        public string[] availableCorridors = new string[] { "USD_SGD", "USD_INR", "EUR_USD" };
    }

    public GameConfig Config { get; private set; }

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            LoadConfig();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    /// <summary>
    /// Loads configuration from StreamingAssets/config.json or creates default config.
    /// </summary>
    private void LoadConfig()
    {
        string configPath = Path.Combine(Application.streamingAssetsPath, "config.json");
        
        if (File.Exists(configPath))
        {
            try
            {
                string jsonContent = File.ReadAllText(configPath);
                Config = JsonUtility.FromJson<GameConfig>(jsonContent);
                Debug.Log($"Config loaded from {configPath}");
            }
            catch (Exception e)
            {
                Debug.LogWarning($"Failed to load config: {e.Message}. Using defaults.");
                Config = new GameConfig();
            }
        }
        else
        {
            Debug.Log("Config file not found. Using defaults.");
            Config = new GameConfig();
            SaveConfig(); // Create default config file
        }

        // Check for config from index.html (set via JavaScript)
        CheckWebGLConfig();
    }

    /// <summary>
    /// Checks for configuration passed from index.html JavaScript.
    /// Called from WebGL build when config panel updates values.
    /// </summary>
    private void CheckWebGLConfig()
    {
        // In WebGL, JavaScript can set these via Application.ExternalCall
        // For now, we'll use PlayerPrefs as a bridge
        if (Application.platform == RuntimePlatform.WebGLPlayer)
        {
            if (PlayerPrefs.HasKey("Config_UseBackend"))
            {
                Config.useBackend = PlayerPrefs.GetInt("Config_UseBackend") == 1;
            }
            if (PlayerPrefs.HasKey("Config_ApiBaseUrl"))
            {
                Config.apiBaseUrl = PlayerPrefs.GetString("Config_ApiBaseUrl");
            }
            if (PlayerPrefs.HasKey("Config_MockSeed"))
            {
                Config.mockSeed = PlayerPrefs.GetInt("Config_MockSeed");
            }
        }
    }

    /// <summary>
    /// Saves current configuration to StreamingAssets/config.json.
    /// </summary>
    public void SaveConfig()
    {
        string configPath = Path.Combine(Application.streamingAssetsPath, "config.json");
        string directory = Path.GetDirectoryName(configPath);
        
        if (!Directory.Exists(directory))
        {
            Directory.CreateDirectory(directory);
        }

        try
        {
            string jsonContent = JsonUtility.ToJson(Config, true);
            File.WriteAllText(configPath, jsonContent);
            Debug.Log($"Config saved to {configPath}");
        }
        catch (Exception e)
        {
            Debug.LogError($"Failed to save config: {e.Message}");
        }
    }

    /// <summary>
    /// Updates configuration from external source (e.g., index.html config panel).
    /// </summary>
    public void UpdateConfig(bool useBackend, string apiBaseUrl, int mockSeed)
    {
        Config.useBackend = useBackend;
        Config.apiBaseUrl = apiBaseUrl;
        Config.mockSeed = mockSeed;
        SaveConfig();
    }

    /// <summary>
    /// Sets JWT token for backend authentication.
    /// </summary>
    public void SetAuthToken(string token)
    {
        Config.jwtToken = token;
    }
}

