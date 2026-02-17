using UnityEngine;

public class PeaFollower : MonoBehaviour
{
    [Header("Visual")]
    [SerializeField] private SpriteRenderer spriteRenderer;

    private Rigidbody2D rb;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        if (rb == null)
            rb = gameObject.AddComponent<Rigidbody2D>();

        rb.bodyType = RigidbodyType2D.Kinematic;
        rb.gravityScale = 0f;
        rb.constraints = RigidbodyConstraints2D.FreezeRotation;
        rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;

        // Trigger so peas don't push the player or each other
        Collider2D col = GetComponent<Collider2D>();
        if (col != null)
            col.isTrigger = true;

        int peaLayer = LayerMask.NameToLayer("Pea");
        if (peaLayer != -1)
            gameObject.layer = peaLayer;
        else
            Debug.LogWarning("'Pea' layer not found. Create it in Project Settings > Tags and Layers.");
    }

    // Called every FixedUpdate by PeaManager with the exact world position this pea should be at
    public void SetPosition(Vector2 position)
    {
        rb.MovePosition(position);
    }
}