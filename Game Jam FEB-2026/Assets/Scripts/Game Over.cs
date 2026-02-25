using UnityEngine;
using UnityEngine.SceneManagement;

public class GameOverScreen : MonoBehaviour
{
    [Header("Scene Names")]
    [SerializeField] private string mainMenuSceneName = "Main Menu";
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
        Debug.Log($"GoToMainMenu called! Attempting to load scene: '{mainMenuSceneName}'");

        // Unpause game before loading
        Time.timeScale = 1f;

        // Check if scene exists in build settings
        int sceneCount = SceneManager.sceneCountInBuildSettings;
        bool sceneFound = false;

        for (int i = 0; i < sceneCount; i++)
        {
            string scenePath = SceneUtility.GetScenePathByBuildIndex(i);
            string sceneName = System.IO.Path.GetFileNameWithoutExtension(scenePath);
            Debug.Log($"Build setting scene {i}: '{sceneName}'");

            if (sceneName == mainMenuSceneName)
            {
                sceneFound = true;
                Debug.Log($"Found matching scene at index {i}!");
            }
        }

        if (!sceneFound)
        {
            Debug.LogError($"Scene '{mainMenuSceneName}' NOT FOUND in Build Settings! Add it via File > Build Settings");
            return;
        }

        // Load main menu
        Debug.Log($"Loading scene: '{mainMenuSceneName}'");
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