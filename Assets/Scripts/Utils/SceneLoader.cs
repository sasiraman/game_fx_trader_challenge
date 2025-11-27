using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Utility class for scene loading and management.
/// </summary>
public class SceneLoader : MonoBehaviour
{
    /// <summary>
    /// Loads a scene by name.
    /// </summary>
    public static void LoadScene(string sceneName)
    {
        SceneManager.LoadScene(sceneName);
    }

    /// <summary>
    /// Loads the landing scene.
    /// </summary>
    public static void LoadLanding()
    {
        LoadScene("Landing");
    }

    /// <summary>
    /// Loads the main menu scene.
    /// </summary>
    public static void LoadMainMenu()
    {
        LoadScene("MainMenu");
    }

    /// <summary>
    /// Loads the gameplay scene.
    /// </summary>
    public static void LoadGamePlay()
    {
        LoadScene("GamePlay");
    }

    /// <summary>
    /// Loads the results scene.
    /// </summary>
    public static void LoadResults()
    {
        LoadScene("Results");
    }

    /// <summary>
    /// Loads the leaderboard scene.
    /// </summary>
    public static void LoadLeaderboard()
    {
        LoadScene("Leaderboard");
    }
}

