using UnityEngine;
using System.Collections;

public class BossEnemy : MonoBehaviour
{
    [Header("Boss Settings")]
    [SerializeField] private int hitsRequired = 1; // How many peas needed to defeat

    [Header("Visual Effects")]
    [SerializeField] private GameObject explosionImage; // Explosion sprite/particle
    [SerializeField] private float explosionDuration = 1f; // How long explosion shows before win screen

    [Header("Win Screen")]
    [SerializeField] private GameObject winScreenUI; // The "YOU WIN" canvas panel

    [Header("Audio (Optional)")]
    [SerializeField] private AudioClip explosionSound;
    [SerializeField] private AudioClip winSound;

    private int currentHits = 0;
    private bool isDefeated = false;

    void Start()
    {
        // Make sure explosion and win screen are hidden initially
        if (explosionImage != null)
            explosionImage.SetActive(false);

        if (winScreenUI != null)
            winScreenUI.SetActive(false);
    }

    void OnCollisionEnter2D(Collision2D collision)
    {
        Debug.Log($"Boss OnCollisionEnter2D with: {collision.gameObject.name}");

        if (isDefeated)
        {
            Debug.Log("Boss already defeated, ignoring collision");
            return;
        }

        // Check if hit by thrown pea
        ThrownPea thrownPea = collision.gameObject.GetComponent<ThrownPea>();

        Debug.Log($"ThrownPea component: {(thrownPea != null ? "FOUND" : "NOT FOUND")}");

        if (thrownPea != null)
        {
            Debug.Log("Boss hit by thrown pea!");
            HitByPea();
        }
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        Debug.Log($"Boss OnTriggerEnter2D with: {other.gameObject.name}");

        if (isDefeated)
        {
            Debug.Log("Boss already defeated, ignoring trigger");
            return;
        }

        // Also check trigger collisions
        ThrownPea thrownPea = other.GetComponent<ThrownPea>();

        Debug.Log($"ThrownPea component: {(thrownPea != null ? "FOUND" : "NOT FOUND")}");

        if (thrownPea != null)
        {
            Debug.Log("Boss hit by thrown pea (trigger)!");
            HitByPea();
        }
    }

    void HitByPea()
    {
        currentHits++;

        Debug.Log($"Boss hit! {currentHits}/{hitsRequired}");

        // Check if defeated
        if (currentHits >= hitsRequired)
        {
            DefeatBoss();
        }
    }

    void DefeatBoss()
    {
        isDefeated = true;

        Debug.Log("Boss defeated! Player wins!");

        // Play explosion sound
        if (explosionSound != null)
        {
            AudioSource.PlayClipAtPoint(explosionSound, transform.position);
        }

        // Show explosion
        if (explosionImage != null)
        {
            explosionImage.SetActive(true);
        }

        // Hide boss sprite (optional)
        SpriteRenderer sr = GetComponent<SpriteRenderer>();
        if (sr != null)
        {
            sr.enabled = false;
        }

        // Start win sequence
        StartCoroutine(ShowWinScreen());
    }

    IEnumerator ShowWinScreen()
    {
        // Wait for explosion to be visible
        yield return new WaitForSeconds(explosionDuration);

        // Play win sound
        if (winSound != null)
        {
            AudioSource.PlayClipAtPoint(winSound, Camera.main.transform.position);
        }

        // Show win screen
        if (winScreenUI != null)
        {
            winScreenUI.SetActive(true);
        }

        // Pause game (optional - prevents player from moving during win screen)
        Time.timeScale = 0f;

        Debug.Log("Win screen displayed!");
    }
}