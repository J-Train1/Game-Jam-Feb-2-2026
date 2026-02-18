using UnityEngine;

public class PeaFollower : MonoBehaviour
{
    [Header("Ground Check")]
    [SerializeField] private float groundCheckRadius = 0.2f;
    [SerializeField] private LayerMask groundLayer;

    [Header("Visual")]
    [SerializeField] private SpriteRenderer spriteRenderer;

    private Rigidbody2D rb;
    private float peaRadius = 0.5f; // half of the 1x1 size so pea sits on top of ground

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        if (rb == null)
            rb = gameObject.AddComponent<Rigidbody2D>();

        if (spriteRenderer == null)
            spriteRenderer = GetComponent<SpriteRenderer>();

        rb.bodyType = RigidbodyType2D.Kinematic;
        rb.gravityScale = 0f;
        rb.constraints = RigidbodyConstraints2D.FreezeRotation;
        rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;

        Collider2D col = GetComponent<Collider2D>();
        if (col != null)
            col.isTrigger = true;

        int peaLayer = LayerMask.NameToLayer("Pea");
        if (peaLayer != -1)
            gameObject.layer = peaLayer;
        else
            Debug.LogWarning("'Pea' layer not found. Create it in Project Settings > Tags and Layers.");
    }

    public void SetPosition(Vector2 position, bool checkCollisions = true)
    {
        if (checkCollisions)
        {
            Vector2 currentPos = rb.position;

            // Check for walls horizontally before moving
            Vector2 direction = position - currentPos;
            float distance = direction.magnitude;

            if (distance > 0.01f)
            {
                RaycastHit2D wallHit = Physics2D.Raycast(currentPos, direction.normalized, distance + peaRadius, groundLayer);

                if (wallHit.collider != null)
                {
                    // Stop at the wall, don't go through it
                    position = currentPos + direction.normalized * Mathf.Max(0, wallHit.distance - peaRadius);
                }
            }

            // Cast downward from the target position to find the ground beneath this pea
            RaycastHit2D groundHit = Physics2D.Raycast(position, Vector2.down, 10f, groundLayer);

            if (groundHit.collider != null)
            {
                // Clamp Y so the pea never goes below the ground surface
                float groundY = groundHit.point.y + peaRadius;
                position.y = Mathf.Max(position.y, groundY);
            }
        }

        rb.MovePosition(position);
    }

    public SpriteRenderer GetSpriteRenderer()
    {
        return spriteRenderer;
    }
}