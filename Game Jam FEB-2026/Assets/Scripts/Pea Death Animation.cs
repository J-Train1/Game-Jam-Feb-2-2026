using UnityEngine;
using System.Collections;

public class PeaDeathAnimation : MonoBehaviour
{
    [Header("Death Animation Settings")]
    [SerializeField] private float bounceForce = 8f;
    [SerializeField] private float spinSpeed = 720f;
    [SerializeField] private float fadeStartDelay = 0.5f;
    [SerializeField] private float fadeDuration = 1f;

    private Rigidbody2D rb;
    private SpriteRenderer spriteRenderer;
    private bool isAnimating = false;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    public void PlayDeathAnimation()
    {
        if (isAnimating) return;
        isAnimating = true;

        // Make physics work for the death animation
        rb.bodyType = RigidbodyType2D.Dynamic;
        rb.gravityScale = 3f;
        rb.constraints = RigidbodyConstraints2D.None; // Allow rotation

        // Make collider a trigger so it doesn't collide with anything
        Collider2D col = GetComponent<Collider2D>();
        if (col != null)
            col.isTrigger = true;

        // Bounce upward with a bit of random horizontal velocity
        float randomX = Random.Range(-2f, 2f);
        rb.linearVelocity = new Vector2(randomX, bounceForce);

        // Add spin
        rb.angularVelocity = Random.Range(-spinSpeed, spinSpeed);

        StartCoroutine(DeathSequence());
    }

    IEnumerator DeathSequence()
    {
        // Wait before starting fade
        yield return new WaitForSeconds(fadeStartDelay);

        // Fade out while falling
        if (spriteRenderer != null)
        {
            Color startColor = spriteRenderer.color;
            float elapsed = 0f;

            while (elapsed < fadeDuration)
            {
                elapsed += Time.deltaTime;
                float alpha = Mathf.Lerp(1f, 0f, elapsed / fadeDuration);
                spriteRenderer.color = new Color(startColor.r, startColor.g, startColor.b, alpha);
                yield return null;
            }
        }

        // Destroy after fade completes
        Destroy(gameObject);
    }
}