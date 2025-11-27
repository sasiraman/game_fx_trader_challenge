using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;

/// <summary>
/// Controller for the landing/login screen.
/// Handles guest login and backend authentication.
/// </summary>
public class LandingScreenController : MonoBehaviour
{
    [Header("UI References")]
    public TMP_InputField usernameInput;
    public TMP_InputField passwordInput;
    public Button guestLoginButton;
    public Button backendLoginButton;
    public GameObject tutorialPanel;
    public Button tutorialCloseButton;
    public Button tutorialButton;

    private void Start()
    {
        // Setup button listeners
        if (guestLoginButton != null)
        {
            guestLoginButton.onClick.AddListener(OnGuestLogin);
        }
        
        if (backendLoginButton != null)
        {
            backendLoginButton.onClick.AddListener(OnBackendLogin);
        }
        
        if (tutorialButton != null)
        {
            tutorialButton.onClick.AddListener(ShowTutorial);
        }
        
        if (tutorialCloseButton != null)
        {
            tutorialCloseButton.onClick.AddListener(HideTutorial);
        }
        
        // Hide tutorial initially
        if (tutorialPanel != null)
        {
            tutorialPanel.SetActive(false);
        }
    }

    /// <summary>
    /// Handles guest login (no backend authentication).
    /// </summary>
    private void OnGuestLogin()
    {
        string username = "guest_" + System.Guid.NewGuid().ToString().Substring(0, 8);
        
        if (GameManager.Instance != null)
        {
            GameManager.Instance.Stats.username = username;
        }
        
        // Load main menu
        SceneManager.LoadScene("MainMenu");
    }

    /// <summary>
    /// Handles backend login with username/password.
    /// </summary>
    private void OnBackendLogin()
    {
        if (ConfigManager.Instance == null || !ConfigManager.Instance.Config.useBackend)
        {
            Debug.LogWarning("Backend mode not enabled. Use guest login or enable backend in config.");
            return;
        }
        
        string username = usernameInput != null ? usernameInput.text : "";
        string password = passwordInput != null ? passwordInput.text : "";
        
        if (string.IsNullOrEmpty(username))
        {
            Debug.LogWarning("Username required for backend login.");
            return;
        }
        
        // Start login coroutine
        StartCoroutine(LoginCoroutine(username, password));
    }

    /// <summary>
    /// Coroutine for backend authentication.
    /// </summary>
    private System.Collections.IEnumerator LoginCoroutine(string username, string password)
    {
        if (APIClient.Instance == null)
        {
            Debug.LogError("APIClient not found!");
            yield break;
        }
        
        bool success = false;
        string token = null;
        
        yield return StartCoroutine(APIClient.Instance.Login(username, password, (s, t) =>
        {
            success = s;
            token = t;
        }));
        
        if (success)
        {
            if (GameManager.Instance != null)
            {
                GameManager.Instance.Stats.username = username;
            }
            
            SceneManager.LoadScene("MainMenu");
        }
        else
        {
            Debug.LogWarning("Login failed. Please check credentials or use guest login.");
            // Show error message to user (implement UI feedback)
        }
    }

    /// <summary>
    /// Shows tutorial overlay.
    /// </summary>
    private void ShowTutorial()
    {
        if (tutorialPanel != null)
        {
            tutorialPanel.SetActive(true);
        }
    }

    /// <summary>
    /// Hides tutorial overlay.
    /// </summary>
    private void HideTutorial()
    {
        if (tutorialPanel != null)
        {
            tutorialPanel.SetActive(false);
        }
    }
}

