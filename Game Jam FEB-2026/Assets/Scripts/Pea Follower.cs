using System.Collections.Generic;
using UnityEngine;

public class PeaFollower : MonoBehaviour
{
    [Header("Follow Settings")]
    [SerializeField] private float followSpeed = 8f;
    [SerializeField] private int followDelay = 20; // frames behind the target (increased from 15)
    [SerializeField] private float minDistanceToMove = 0.01f;
    [SerializeField] private float minDistanceFromTarget = 0.5f; // Minimum distance to maintain from target

    [Header("Visual")]
    [SerializeField] private SpriteRenderer spriteRenderer;

    private PlayerController playerController;
    private PeaFollower previousPea; // if following another pea instead of player
    private int indexInChain = 0;
    private Vector2 targetPosition;

    void Awake()
    {
        // Set up collision layer for peas - they should not collide with player or other peas
        SetupCollisionLayer();
    }

    private void SetupCollisionLayer()
    {
        // Try to set the pea to a "Pea" layer if it exists
        // If the layer doesn't exist, you'll need to create it in Unity:
        // Edit > Project Settings > Tags and Layers > Add "Pea" layer

        int peaLayer = LayerMask.NameToLayer("Pea");
        if (peaLayer != -1)
        {
            gameObject.layer = peaLayer;
        }
        else
        {
            // Fallback: Try to use "Ignore Raycast" layer or keep default
            Debug.LogWarning("'Pea' layer not found. Please create a 'Pea' layer in Project Settings > Tags and Layers, then set up collision matrix to ignore Player and Pea layers.");
        }

        // If the pea has a Rigidbody2D, make it kinematic so it doesn't respond to physics
        Rigidbody2D rb = GetComponent<Rigidbody2D>();
        if (rb != null)
        {
            rb.bodyType = RigidbodyType2D.Kinematic;
            rb.constraints = RigidbodyConstraints2D.FreezeRotation;
        }

        // If the pea has a collider, make it a trigger
        Collider2D col = GetComponent<Collider2D>();
        if (col != null)
        {
            col.isTrigger = true;
        }
    }

    public void Initialize(PlayerController player, PeaFollower previous, int chainIndex)
    {
        playerController = player;
        previousPea = previous;
        indexInChain = chainIndex;

        // Calculate follow delay based on chain position
        // Each pea follows a bit behind the one in front
        followDelay = 20 + (chainIndex * 8); // Increased spacing between peas
    }

    void Update()
    {
        UpdateFollowPosition();
    }

    private void UpdateFollowPosition()
    {
        // Get target position from history
        if (previousPea != null)
        {
            // Follow the pea in front of us
            targetPosition = previousPea.GetHistoricalPosition(20); // Increased delay between peas
        }
        else if (playerController != null)
        {
            // First pea in chain - follow player directly
            targetPosition = playerController.GetHistoricalPosition(followDelay);
        }
        else
        {
            return; // No target to follow
        }

        // Check distance to target
        float distance = Vector2.Distance(transform.position, targetPosition);

        // Only move if we're far enough away AND above minimum distance
        if (distance > minDistanceToMove && distance > minDistanceFromTarget)
        {
            // Calculate how much to move
            float moveAmount = followSpeed * Time.deltaTime;

            // Don't move closer than minDistanceFromTarget
            float maxMove = distance - minDistanceFromTarget;
            moveAmount = Mathf.Min(moveAmount, maxMove);

            if (moveAmount > 0)
            {
                // Smooth movement towards target
                transform.position = Vector2.MoveTowards(
                    transform.position,
                    targetPosition,
                    moveAmount
                );
            }
        }
    }

    // For other peas to follow this pea
    private Queue<Vector2> myPositionHistory = new Queue<Vector2>();
    private int myHistorySize = 100; // Increased history size for better tracking

    void LateUpdate()
    {
        // Record our own position history for peas following us
        RecordPosition();
    }

    private void RecordPosition()
    {
        myPositionHistory.Enqueue(transform.position);

        if (myPositionHistory.Count > myHistorySize)
        {
            myPositionHistory.Dequeue();
        }
    }

    public Vector2 GetHistoricalPosition(int framesAgo)
    {
        framesAgo = Mathf.Clamp(framesAgo, 0, myPositionHistory.Count - 1);

        Vector2[] historyArray = myPositionHistory.ToArray();
        int index = historyArray.Length - 1 - framesAgo;

        return index >= 0 ? historyArray[index] : transform.position;
    }

    public void SetFollowSpeed(float speed)
    {
        followSpeed = speed;
    }

    public void SetFollowDelay(int delay)
    {
        followDelay = delay;
    }

    // Visual feedback
    void OnDrawGizmos()
    {
        if (Application.isPlaying)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawLine(transform.position, targetPosition);
            Gizmos.DrawWireSphere(targetPosition, 0.1f);

            // Draw minimum distance circle
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(targetPosition, minDistanceFromTarget);
        }
    }
}