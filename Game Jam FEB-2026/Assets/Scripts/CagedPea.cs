using UnityEngine;

public class CagedPea : MonoBehaviour
{
    [Header("Release Settings")]
    [SerializeField] private float conversionHeightAboveGround = 1f;
    [SerializeField] private GameObject collectiblePeaPrefab;
    [SerializeField] private LayerMask groundLayer;

    [Header("Visual Effect (Optional)")]
    [SerializeField] private GameObject releaseParticles;

    private Rigidbody2D rb;
    private bool isReleased = false;
    private bool hasConverted = false;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        if (rb == null)
        {
            rb = gameObject.AddComponent<Rigidbody2D>();
        }

        // Start as kinematic (not falling)
        rb.bodyType = RigidbodyType2D.Kinematic;
        rb.gravityScale = 3f;

        // Make sure it has a collider
        if (GetComponent<Collider2D>() == null)
        {
            CircleCollider2D col = gameObject.AddComponent<CircleCollider2D>();
            col.radius = 0.5f;
        }
    }

    public void Release()
    {
        if (isReleased) return;

        isReleased = true;

        Debug.Log("Caged pea released - starting fall");

        // Spawn particle effect
        if (releaseParticles != null)
        {
            Instantiate(releaseParticles, transform.position, Quaternion.identity);
        }

        // Start falling
        rb.bodyType = RigidbodyType2D.Dynamic;
    }

    void FixedUpdate()
    {
        if (!isReleased || hasConverted) return;

        // Check height above ground
        RaycastHit2D groundHit = Physics2D.Raycast(transform.position, Vector2.down, 100f, groundLayer);

        if (groundHit.collider != null)
        {
            float heightAboveGround = transform.position.y - groundHit.point.y;

            // Convert to collectible when reaching threshold height
            if (heightAboveGround <= conversionHeightAboveGround)
            {
                ConvertToCollectible();
            }
        }
    }

    void ConvertToCollectible()
    {
        hasConverted = true;

        Debug.Log("Caged pea converting to collectible");

        // Spawn collectible at current position
        if (collectiblePeaPrefab != null)
        {
            GameObject collectible = Instantiate(collectiblePeaPrefab, transform.position, Quaternion.identity);
            Destroy(gameObject);
        }
        else
        {
            // Convert in-place
            Destroy(this);

            rb.bodyType = RigidbodyType2D.Kinematic;
            rb.linearVelocity = Vector2.zero;

            Collider2D col = GetComponent<Collider2D>();
            if (col != null)
                col.isTrigger = true;

            gameObject.AddComponent<CollectiblePea>();
        }
    }
}