using UnityEngine;
using UnityEngine.InputSystem;

public class ThrowingSystem : MonoBehaviour
{
    [Header("Throwing Settings")]
    [SerializeField] private float maxThrowForce = 20f;
    [SerializeField] private float dragScale = 1f; // How far you need to drag for max force
    [SerializeField] private float thrownPeaGravity = 3f; // Must match ThrownPea's gravityScale
    [SerializeField] private GameObject thrownPeaPrefab;

    [Header("Trajectory Line")]
    [SerializeField] private LineRenderer trajectoryLine;
    [SerializeField] private int trajectoryPointCount = 20;
    [SerializeField] private float trajectoryTimeStep = 0.1f;
    [SerializeField] private Color trajectoryColor = Color.white;

    [Header("References")]
    [SerializeField] private PeaManager peaManager;
    [SerializeField] private Camera mainCamera;

    private Vector2 dragStartPos;
    private Vector2 currentDragPos;
    private bool isDragging = false;

    void Awake()
    {
        if (mainCamera == null)
            mainCamera = Camera.main;

        if (peaManager == null)
            peaManager = GetComponent<PeaManager>();

        // Setup trajectory line
        if (trajectoryLine == null)
        {
            GameObject lineObj = new GameObject("TrajectoryLine");
            lineObj.transform.SetParent(transform);
            trajectoryLine = lineObj.AddComponent<LineRenderer>();
        }

        trajectoryLine.positionCount = trajectoryPointCount;
        trajectoryLine.startWidth = 0.05f;
        trajectoryLine.endWidth = 0.05f;
        trajectoryLine.material = new Material(Shader.Find("Sprites/Default"));
        trajectoryLine.startColor = trajectoryColor;
        trajectoryLine.endColor = trajectoryColor;
        trajectoryLine.enabled = false;

        // Make dotted line effect
        trajectoryLine.textureMode = LineTextureMode.Tile;
    }

    void Update()
    {
        HandleThrowingInput();
    }

    void HandleThrowingInput()
    {
        // Can only throw if we have peas
        if (peaManager.GetPeaCount() == 0)
        {
            if (isDragging)
            {
                isDragging = false;
                trajectoryLine.enabled = false;
            }
            return;
        }

        // Mouse input
        Mouse mouse = Mouse.current;
        if (mouse == null) return;

        // Start drag
        if (mouse.leftButton.wasPressedThisFrame)
        {
            dragStartPos = mainCamera.ScreenToWorldPoint(mouse.position.ReadValue());
            isDragging = true;
        }

        // Update drag
        if (isDragging && mouse.leftButton.isPressed)
        {
            currentDragPos = mainCamera.ScreenToWorldPoint(mouse.position.ReadValue());
            UpdateTrajectoryLine();
        }

        // Release - throw!
        if (isDragging && mouse.leftButton.wasReleasedThisFrame)
        {
            ThrowPea();
            isDragging = false;
            trajectoryLine.enabled = false;
        }
    }

    void UpdateTrajectoryLine()
    {
        trajectoryLine.enabled = true;

        Vector2 throwVector = dragStartPos - currentDragPos;
        Vector2 throwVelocity = CalculateThrowVelocity(throwVector);

        // Simulate trajectory with actual Unity physics
        Vector2 startPos = transform.position;
        Vector2 velocity = throwVelocity;
        Vector2 gravity = Physics2D.gravity * thrownPeaGravity; // Use same gravity as ThrownPea

        for (int i = 0; i < trajectoryPointCount; i++)
        {
            float time = i * trajectoryTimeStep;

            // Physics formula: position = startPos + velocity * time + 0.5 * gravity * time^2
            Vector2 point = startPos + velocity * time + 0.5f * gravity * time * time;
            trajectoryLine.SetPosition(i, point);

            // Check if we hit ground - stop trajectory there
            if (i > 0)
            {
                Vector2 prevPoint = trajectoryLine.GetPosition(i - 1);

                // Cast a line between previous point and current point
                RaycastHit2D hit = Physics2D.Linecast(prevPoint, point, LayerMask.GetMask("Ground"));

                if (hit.collider != null)
                {
                    // Found collision point - place dot there and hide rest
                    trajectoryLine.SetPosition(i, hit.point);

                    // Hide remaining points by placing them at hit point
                    for (int j = i + 1; j < trajectoryPointCount; j++)
                    {
                        trajectoryLine.SetPosition(j, hit.point);
                    }
                    break;
                }
            }
        }
    }

    Vector2 CalculateThrowVelocity(Vector2 dragVector)
    {
        // Clamp drag distance to max throw force
        float dragDistance = dragVector.magnitude;
        float clampedDistance = Mathf.Min(dragDistance, dragScale);

        // Calculate force (0 to maxThrowForce)
        float forceMagnitude = (clampedDistance / dragScale) * maxThrowForce;

        // Apply force in drag direction
        Vector2 velocity = dragVector.normalized * forceMagnitude;
        return velocity;
    }

    // REMOVED - using inline physics calculation in UpdateTrajectoryLine instead

    void ThrowPea()
    {
        if (peaManager.GetPeaCount() == 0) return;

        // Calculate throw velocity
        Vector2 throwVector = dragStartPos - currentDragPos;
        Vector2 throwVelocity = CalculateThrowVelocity(throwVector);

        // Only throw if there's some force
        if (throwVelocity.magnitude < 0.5f) return;

        // Remove last pea from chain
        PeaFollower lastPea = peaManager.RemoveLastPea();

        if (lastPea == null)
        {
            Debug.LogWarning("Failed to remove pea for throwing!");
            return;
        }

        // Convert to thrown pea
        ThrownPea thrownPea = lastPea.gameObject.AddComponent<ThrownPea>();
        thrownPea.Initialize(throwVelocity, thrownPeaPrefab);

        Debug.Log($"Threw pea with velocity: {throwVelocity}, remaining peas: {peaManager.GetPeaCount()}");
    }
}