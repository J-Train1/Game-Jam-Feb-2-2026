using UnityEngine;

public class ThrownPea : MonoBehaviour
{
    [Header("Physics")]
    [SerializeField] private float stoppedVelocityThreshold = 1f;
    [SerializeField] private float checkStoppedDelay = 0.3f;
    [SerializeField] private float timeBeforeConversion = 1.5f;

    [Header("Conversion")]
    [SerializeField] private GameObject collectiblePeaPrefab;
    [SerializeField] private float collectibleSpawnHeightOffset = 0.5f;

    private Rigidbody2D rb;
    private float timeThrown;
    private float timeStationary = 0f;
    private float timeOnGround = 0f;
    private bool hasLanded = false;
    private bool isGrounded = false;
    private Vector2 lastPosition;

    public void Initialize(Vector2 velocity, GameObject collectiblePrefab)
    {
        collectiblePeaPrefab = collectiblePrefab;

        rb = GetComponent<Rigidbody2D>();
        if (rb == null)
            rb = gameObject.AddComponent<Rigidbody2D>();

        rb.bodyType = RigidbodyType2D.Dynamic;
        rb.gravityScale = 3f;
        rb.linearVelocity = velocity;
        rb.constraints = RigidbodyConstraints2D.FreezeRotation;
        rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;

        PhysicsMaterial2D mat = new PhysicsMaterial2D("ThrownPea");
        mat.friction = 0.8f;
        mat.bounciness = 0.2f;
        rb.sharedMaterial = mat;

        Collider2D col = GetComponent<Collider2D>();
        if (col != null)
        {
            col.isTrigger = false;
            col.sharedMaterial = mat;
        }

        PeaFollower follower = GetComponent<PeaFollower>();
        if (follower != null)
            Destroy(follower);

        timeThrown = Time.time;
        lastPosition = transform.position;

        Debug.Log($"ThrownPea initialized with velocity: {velocity}");
    }

    void FixedUpdate()
    {
        if (hasLanded)
            return;

        if (Time.time - timeThrown > timeBeforeConversion)
        {
            Debug.Log("ThrownPea: Max time reached, forcing conversion");
            ConvertToCollectible();
            return;
        }

        if (Time.time - timeThrown < checkStoppedDelay)
            return;

        if (isGrounded)
        {
            timeOnGround += Time.fixedDeltaTime;

            float speed = rb.linearVelocity.magnitude;
            float distanceMoved = Vector2.Distance(transform.position, lastPosition);

            if (speed < stoppedVelocityThreshold && distanceMoved < 0.01f)
            {
                timeStationary += Time.fixedDeltaTime;

                if (timeStationary > 0.3f)
                {
                    Debug.Log($"ThrownPea stopped: speed={speed:F2}, moved={distanceMoved:F3}");
                    ConvertToCollectible();
                }
            }
            else
            {
                timeStationary = 0f;
            }
        }

        lastPosition = transform.position;
    }

    void OnCollisionEnter2D(Collision2D collision)
    {
        // Check if we hit a hazard
        if (collision.gameObject.CompareTag("Hazard"))
        {
            Debug.Log("ThrownPea hit hazard - triggering death animation");
            TriggerDeathAnimation();
            return;
        }

        // Check if we hit ground
        if (collision.gameObject.layer == LayerMask.NameToLayer("Ground"))
        {
            isGrounded = true;
        }

        Debug.Log($"ThrownPea hit: {collision.gameObject.name}, velocity: {rb.linearVelocity.magnitude:F2}");
    }

    void TriggerDeathAnimation()
    {
        hasLanded = true; // Stop other conversion checks

        // Check if this pea has the death animation component
        PeaDeathAnimation deathAnim = GetComponent<PeaDeathAnimation>();

        if (deathAnim == null)
        {
            // Add the death animation component
            deathAnim = gameObject.AddComponent<PeaDeathAnimation>();
        }

        // Make sure it's set to dynamic with physics for the animation
        rb.bodyType = RigidbodyType2D.Dynamic;
        rb.gravityScale = 3f;

        // Make collider trigger so it doesn't collide during death
        Collider2D col = GetComponent<Collider2D>();
        if (col != null)
            col.isTrigger = true;

        // Remove this script and let death animation take over
        Destroy(this);

        // Play the death animation
        deathAnim.PlayDeathAnimation();
    }

    void OnCollisionStay2D(Collision2D collision)
    {
        if (collision.gameObject.layer == LayerMask.NameToLayer("Ground"))
        {
            isGrounded = true;
        }
    }

    void OnCollisionExit2D(Collision2D collision)
    {
        if (collision.gameObject.layer == LayerMask.NameToLayer("Ground"))
        {
            isGrounded = false;
            timeOnGround = 0f;
            timeStationary = 0f;
        }
    }

    void ConvertToCollectible()
    {
        hasLanded = true;

        Debug.Log($"ThrownPea converting to collectible. Prefab assigned: {collectiblePeaPrefab != null}");

        Vector3 spawnPosition = transform.position + Vector3.up * collectibleSpawnHeightOffset;

        if (collectiblePeaPrefab != null)
        {
            Debug.Log("Spawning collectible prefab");
            GameObject collectible = Instantiate(collectiblePeaPrefab, spawnPosition, Quaternion.identity);
            Destroy(gameObject);
        }
        else
        {
            Debug.Log("Converting in-place (no prefab)");

            transform.position = spawnPosition;

            Destroy(this);

            rb.bodyType = RigidbodyType2D.Kinematic;
            rb.linearVelocity = Vector2.zero;
            rb.angularVelocity = 0f;
            rb.gravityScale = 0f;
            rb.sharedMaterial = null;

            Collider2D col = GetComponent<Collider2D>();
            if (col != null)
            {
                col.isTrigger = true;
                col.sharedMaterial = null;
            }

            CollectiblePea collectible = gameObject.AddComponent<CollectiblePea>();

            Debug.Log($"CollectiblePea component added: {collectible != null}");
        }
    }
}