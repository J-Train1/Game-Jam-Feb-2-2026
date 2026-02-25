using UnityEngine;
using UnityEngine.SceneManagement;

public class GameOverScreen : MonoBehaviour
{
    [Header("Scene Names")]
    [SerializeField] private string mainMenuSceneName = "MainMenu";
    [SerializeField] private string currentLevelName = "Map"; // Name of current level to restart

    [Header("Game Over UI")]
    [SerializeField] private GameObject gameOverPanel; // The "YOU DIED" panel

    [Header("Delay Settings")]
    [SerializeField] private float delayBeforeShow = 0.5f; // Wait before showing game over screen

    void Start()
    {
        // Automatically detect current scene name if not set
        if (string.IsNullOrEmpty(currentLevelName) || currentLevelName == "Map")
        {
            currentLevelName = SceneManager.GetActiveScene().name;
        }

        // Make sure game over panel is hidden initially
        if (gameOverPanel != null)
        {
            gameOverPanel.SetActive(false);
        }
    }

    // Called by HealthSystem when player dies
    public void ShowGameOver()
    {
        if (gameOverPanel != null)
        {
            Invoke(nameof(DisplayGameOverPanel), delayBeforeShow);
        }
    }

    void DisplayGameOverPanel()
    {
        if (gameOverPanel != null)
        {
            gameOverPanel.SetActive(true);
            Debug.Log("Game Over screen displayed");
        }

        // Pause game (optional) - COMMENTED OUT FOR TESTING
        // Time.timeScale = 0f;
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

    // Called by Quit button (optional)
    public void QuitGame()
    {
        Debug.Log("Quitting game");

        // Unpause before quitting
        Time.timeScale = 1f;

        Application.Quit();
    }
}