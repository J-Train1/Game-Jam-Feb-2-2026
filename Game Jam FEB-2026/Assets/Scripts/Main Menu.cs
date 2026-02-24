using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour
{
    [Header("Menu Panels")]
    [SerializeField] private GameObject mainMenuPanel;
    [SerializeField] private GameObject howToPlayPanel;

    [Header("Scene Settings")]
    [SerializeField] private string gameSceneName = "Map";

    void Start()
    {
        // Make sure main menu is visible and how-to-play is hidden
        if (mainMenuPanel != null)
            mainMenuPanel.SetActive(true);

        if (howToPlayPanel != null)
            howToPlayPanel.SetActive(false);
    }

    // Called by Play button
    public void PlayGame()
    {
        Debug.Log($"Loading scene: {gameSceneName}");
        SceneManager.LoadScene(gameSceneName);
    }

    // Called by How To Play button
    public void ShowHowToPlay()
    {
        if (howToPlayPanel != null)
        {
            howToPlayPanel.SetActive(true);
            Debug.Log("How To Play panel opened");
        }
    }

    // Called by X button on How To Play panel
    public void CloseHowToPlay()
    {
        if (howToPlayPanel != null)
        {
            howToPlayPanel.SetActive(false);
            Debug.Log("How To Play panel closed");
        }
    }

    // Optional: Quit game (works in builds, not editor)
    public void QuitGame()
    {
        Debug.Log("Quitting game");
        Application.Quit();
    }
}