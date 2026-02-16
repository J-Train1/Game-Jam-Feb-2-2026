using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class CollectiblePea : MonoBehaviour
{
    [Header("Collection Settings")]
    [SerializeField] private bool useTag = true;
    [SerializeField] private string playerTag = "Player";

    [Header("Visual Feedback")]
    [SerializeField] private float bobSpeed = 2f;
    [SerializeField] private float bobHeight = 0.2f;
    [SerializeField] private bool enableBobbing = true;

    private Vector3 startPosition;
    private Collider2D col;

    void Awake()
    {
        col = GetComponent<Collider2D>();
        col.isTrigger = true; // Make sure it's a trigger

        startPosition = transform.position;
    }

    void Update()
    {
        // Optional: Bobbing animation for collectibles
        if (enableBobbing)
        {
            float newY = startPosition.y + Mathf.Sin(Time.time * bobSpeed) * bobHeight;
            transform.position = new Vector3(transform.position.x, newY, transform.position.z);
        }
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        // Check if player collided
        bool isPlayer = false;

        if (useTag)
        {
            isPlayer = other.CompareTag(playerTag);
        }
        else
        {
            isPlayer = other.GetComponent<PlayerController>() != null;
        }

        if (isPlayer)
        {
            // Find the PeaManager and tell it to collect this pea
            PeaManager peaManager = other.GetComponent<PeaManager>();

            if (peaManager != null)
            {
                peaManager.CollectPea(gameObject);
                // Note: PeaManager will destroy this object
            }
            else
            {
                Debug.LogWarning("Player doesn't have a PeaManager component!");
            }
        }
    }

    // Visual debug
    void OnDrawGizmos()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, 0.3f);
    }
}