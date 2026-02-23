using UnityEngine;

public class MovingPlatform : MonoBehaviour
{
    [Header("Movement Settings")]
    [SerializeField] private Vector2 moveDirection = Vector2.right; // Direction to move
    [SerializeField] private float moveDistance = 5f; // How far to move
    [SerializeField] private float moveSpeed = 2f; // Speed of movement
    [SerializeField] private AnimationCurve movementCurve = AnimationCurve.EaseInOut(0, 0, 1, 1); // Smooth movement

    [Header("Movement Type")]
    [SerializeField] private bool useWaypoints = false;
    [SerializeField] private Transform[] waypoints; // Optional: define exact positions

    private Vector3 startPosition;
    private float timeOffset;
    private int currentWaypointIndex = 0;

    void Start()
    {
        startPosition = transform.position;
        timeOffset = Random.Range(0f, 100f); // Random offset so platforms aren't synced

        // Normalize direction
        moveDirection = moveDirection.normalized;
    }

    void Update()
    {
        if (useWaypoints && waypoints != null && waypoints.Length > 0)
        {
            MoveAlongWaypoints();
        }
        else
        {
            MoveBackAndForth();
        }
    }

    void MoveBackAndForth()
    {
        // Calculate position using sine wave for smooth back-and-forth
        float time = (Time.time + timeOffset) * moveSpeed;
        float curveValue = movementCurve.Evaluate(Mathf.PingPong(time, 1f));
        float offset = (curveValue - 0.5f) * 2f * moveDistance; // -distance to +distance

        Vector3 newPosition = startPosition + (Vector3)(moveDirection * offset);
        transform.position = newPosition;
    }

    void MoveAlongWaypoints()
    {
        if (waypoints.Length < 2) return;

        // Move toward current waypoint
        Transform targetWaypoint = waypoints[currentWaypointIndex];

        float step = moveSpeed * Time.deltaTime;
        transform.position = Vector3.MoveTowards(transform.position, targetWaypoint.position, step);

        // Check if reached waypoint
        if (Vector3.Distance(transform.position, targetWaypoint.position) < 0.01f)
        {
            // Move to next waypoint
            currentWaypointIndex = (currentWaypointIndex + 1) % waypoints.Length;
        }
    }

    void OnCollisionEnter2D(Collision2D collision)
    {
        // Make player/peas children of platform so they move with it
        if (collision.gameObject.CompareTag("Player") || collision.gameObject.GetComponent<PeaFollower>() != null)
        {
            collision.transform.SetParent(transform);
        }
    }

    void OnCollisionExit2D(Collision2D collision)
    {
        // Unparent when leaving platform
        if (collision.gameObject.CompareTag("Player") || collision.gameObject.GetComponent<PeaFollower>() != null)
        {
            collision.transform.SetParent(null);
        }
    }

    void OnDrawGizmos()
    {
        // Visualize movement path in editor
        if (useWaypoints && waypoints != null && waypoints.Length > 1)
        {
            Gizmos.color = Color.yellow;
            for (int i = 0; i < waypoints.Length; i++)
            {
                if (waypoints[i] != null)
                {
                    int nextIndex = (i + 1) % waypoints.Length;
                    if (waypoints[nextIndex] != null)
                    {
                        Gizmos.DrawLine(waypoints[i].position, waypoints[nextIndex].position);
                    }
                }
            }
        }
        else
        {
            // Show movement range
            Vector3 start = Application.isPlaying ? startPosition : transform.position;
            Gizmos.color = Color.cyan;
            Gizmos.DrawLine(start - (Vector3)(moveDirection * moveDistance),
                           start + (Vector3)(moveDirection * moveDistance));
        }
    }
}