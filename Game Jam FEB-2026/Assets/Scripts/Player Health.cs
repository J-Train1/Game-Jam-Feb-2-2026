using UnityEngine;
using UnityEngine.SceneManagement;

public class HealthSystem : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private PeaManager peaManager;
    [SerializeField] private GameObject gameOverCanvas;

    [Header("Damage Settings")]
    [SerializeField] private float invincibilityTime = 1f;
    [SerializeField] private string hazardTag = "Hazard";

    private float invincibilityTimer = 0f;
    private bool isDead = false;

    void Awake()
    {
        if (peaManager == null)
            peaManager = GetComponent<PeaManager>();

        if (gameOverCanvas != null)
            gameOverCanvas.SetActive(false);
    }

    void Update()
    {
        if (invincibilityTimer > 0)
            invincibilityTimer -= Time.deltaTime;
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (isDead || invincibilityTimer > 0)
            return;

        if (other.CompareTag(hazardTag))
        {
            TakeDamage();
        }
    }

    public void TakeDamage()
    {
        if (isDead || invincibilityTimer > 0)
            return;

        var peaChain = peaManager.GetPeaChain();

        if (peaChain.Count > 0)
        {
            // Kill the last pea in the chain
            PeaFollower lastPea = peaChain[peaChain.Count - 1];
            peaChain.RemoveAt(peaChain.Count - 1);
            Destroy(lastPea.gameObject);

            invincibilityTimer = invincibilityTime;
            Debug.Log($"Pea lost! Remaining: {peaChain.Count}");
        }
        else
        {
            // No peas left — player dies
            Die();
        }
    }

    void Die()
    {
        isDead = true;
        Debug.Log("Player died!");

        if (gameOverCanvas != null)
            gameOverCanvas.SetActive(true);

        // Freeze the game
        Time.timeScale = 0f;
    }

    public void Restart()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    public void MainMenu()
    {
        Time.timeScale = 1f;
        // Replace "MainMenu" with your actual main menu scene name
        SceneManager.LoadScene("MainMenu");
    }

    public int GetCurrentHealth()
    {
        return peaManager.GetPeaCount() + 1; // +1 for the player itself
    }
}