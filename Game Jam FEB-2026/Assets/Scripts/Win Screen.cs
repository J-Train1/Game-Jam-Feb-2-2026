using UnityEngine;
using UnityEngine.SceneManagement;

public class WinScreen : MonoBehaviour
{
    [Header("Scene Names")]
    [SerializeField] private string mainMenuSceneName = "MainMenu";
    [SerializeField] private string currentLevelName = "Map"; // Name of current level to restart

    void Start()
    {
        // Automatically detect current scene name if not set
        if (string.IsNullOrEmpty(currentLevelName) || currentLevelName == "Map")
        {
            currentLevelName = SceneManager.GetActiveScene().name;
        }
    }

    // Called by Restart button
    public void RestartLevel()
    {
        Debug.Log($"Restarting level: {currentLevelName}");

        // Unpause game before loading
        Time.timeScale = 1f;

        // Reload current scene
        SceneManager.LoadScene(currentLevelName);
    }

    // Called by Main Menu button
    public void GoToMainMenu()
    {
        Debug.Log("Returning to main menu");

        // Unpause game before loading
        Time.timeScale = 1f;

        // Load main menu
        SceneManager.LoadScene(mainMenuSceneName);
    }

    // Called by Next Level button (optional)
    public void LoadNextLevel()
    {
        Debug.Log("Loading next level");

        // Unpause game
        Time.timeScale = 1f;

        // Load next scene in build order
        int currentSceneIndex = SceneManager.GetActiveScene().buildIndex;
        int nextSceneIndex = currentSceneIndex + 1;

        // Check if there is a next scene
        if (nextSceneIndex < SceneManager.sceneCountInBuildSettings)
        {
            SceneManager.LoadScene(nextSceneIndex);
        }
        else
        {
            // No more levels - go to main menu
            Debug.Log("No more levels, returning to main menu");
            GoToMainMenu();
        }
    }

    // Called by Quit button (optional)
    public void QuitGame()
    {
        Debug.Log("Quitting game");

        // Unpause before quitting
        Time.timeScale = 1f;

        Application.Quit();
    }
}