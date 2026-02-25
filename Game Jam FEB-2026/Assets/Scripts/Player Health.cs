using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class HealthSystem : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private PeaManager peaManager;
    [SerializeField] private GameOverScreen gameOverScreen; // Reference to game over controller
    [SerializeField] private SpriteRenderer playerSpriteRenderer;

    [Header("Damage Settings")]
    [SerializeField] private float invincibilityTime = 1f;
    [SerializeField] private string hazardTag = "Hazard";

    [Header("Visual Feedback")]
    [SerializeField] private Color damageColor = Color.red;
    [SerializeField] private float flashDuration = 1f;

    private float invincibilityTimer = 0f;
    private bool isDead = false;

    void Awake()
    {
        if (peaManager == null)
            peaManager = GetComponent<PeaManager>();

        if (playerSpriteRenderer == null)
            playerSpriteRenderer = GetComponent<SpriteRenderer>();
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
            // Get the last pea and remove it from the chain
            PeaFollower lastPea = peaChain[peaChain.Count - 1];
            peaChain.RemoveAt(peaChain.Count - 1);

            // Flash all remaining peas red (not including the dying one)
            StartCoroutine(DamageFlash());

            // Play death animation - pea will stay red during animation
            PeaDeathAnimation deathAnim = lastPea.GetComponent<PeaDeathAnimation>();
            if (deathAnim != null)
            {
                // Make sure the dying pea is red
                SpriteRenderer dyingRenderer = lastPea.GetSpriteRenderer();
                if (dyingRenderer != null)
                    dyingRenderer.color = damageColor;

                deathAnim.PlayDeathAnimation();
            }
            else
            {
                // Fallback if no death animation component
                Destroy(lastPea.gameObject);
            }

            invincibilityTimer = invincibilityTime;

            Debug.Log($"Pea lost! Remaining: {peaChain.Count}");
        }
        else
        {
            // No peas left — player dies
            Die();
        }
    }

    IEnumerator DamageFlash()
    {
        // Store original colors
        Color playerOriginalColor = playerSpriteRenderer != null ? playerSpriteRenderer.color : Color.white;

        var peaChain = peaManager.GetPeaChain();
        Color[] peaOriginalColors = new Color[peaChain.Count];
        SpriteRenderer[] peaRenderers = new SpriteRenderer[peaChain.Count];

        // Get all pea renderers and store original colors
        for (int i = 0; i < peaChain.Count; i++)
        {
            peaRenderers[i] = peaChain[i].GetSpriteRenderer();
            if (peaRenderers[i] != null)
                peaOriginalColors[i] = peaRenderers[i].color;
        }

        // Flash to red
        if (playerSpriteRenderer != null)
            playerSpriteRenderer.color = damageColor;

        foreach (var renderer in peaRenderers)
        {
            if (renderer != null)
                renderer.color = damageColor;
        }

        // Wait for flash duration
        yield return new WaitForSeconds(flashDuration);

        // Restore original colors
        if (playerSpriteRenderer != null)
            playerSpriteRenderer.color = playerOriginalColor;

        for (int i = 0; i < peaRenderers.Length; i++)
        {
            if (peaRenderers[i] != null)
                peaRenderers[i].color = peaOriginalColors[i];
        }
    }

    void Die()
    {
        isDead = true;
        Debug.Log("Player died!");

        // Call game over screen
        if (gameOverScreen != null)
        {
            gameOverScreen.ShowGameOver();
        }
        else
        {
            // Fallback if no game over screen assigned
            Debug.LogWarning("No GameOverScreen assigned to HealthSystem!");
            Time.timeScale = 0f;
        }
    }

    // DEPRECATED - keeping for backward compatibility, but GameOverScreen handles this now
    public void Restart()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    // DEPRECATED - keeping for backward compatibility, but GameOverScreen handles this now
    public void MainMenu()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene("MainMenu");
    }

    public int GetCurrentHealth()
    {
        return peaManager.GetPeaCount() + 1; // +1 for the player itself
    }
}