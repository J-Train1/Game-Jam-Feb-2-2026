using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections.Generic;

[RequireComponent(typeof(Rigidbody2D))]
public class PlayerController : MonoBehaviour
{
    [Header("Movement Settings")]
    [SerializeField] private float moveSpeed = 5f;
    [SerializeField] private float jumpForce = 12f;
    [SerializeField] private float acceleration = 50f;
    [SerializeField] private float deceleration = 50f;

    [Header("Ground Check")]
    [SerializeField] private Transform groundCheck;
    [SerializeField] private float groundCheckRadius = 0.3f;
    [SerializeField] private LayerMask groundLayer;

    [Header("Position History")]
    [SerializeField] private int historySize = 120;

    private Rigidbody2D rb;
    private float horizontalInput;
    private bool jumpPressed;
    private bool isGrounded;
    private float currentVelocityX;
    private float halfHeight = 0.5f;

    private float coyoteTimeCounter;
    private const float coyoteTime = 0.1f; // Small buffer for ground detection

    private Queue<Vector2> positionHistory = new Queue<Vector2>();

    private PlayerInput playerInput;
    private InputAction moveAction;
    private InputAction jumpAction;

    private PeaManager peaManager;
    private float stackRiseTarget = 0f; // Target Y position during stacking
    private bool isRisingForStack = false;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();

        for (int i = 0; i < historySize; i++)
            positionHistory.Enqueue(transform.position);

        playerInput = GetComponent<PlayerInput>();
        if (playerInput != null)
        {
            moveAction = playerInput.actions["Move"];
            jumpAction = playerInput.actions["Jump"];
        }

        peaManager = GetComponent<PeaManager>();
    }

    void OnEnable()
    {
        if (moveAction != null) moveAction.Enable();
        if (jumpAction != null) jumpAction.Enable();
    }

    void OnDisable()
    {
        if (moveAction != null) moveAction.Disable();
        if (jumpAction != null) jumpAction.Disable();
    }

    void Update()
    {
        if (moveAction != null)
            horizontalInput = moveAction.ReadValue<Vector2>().x;

        if (jumpAction != null && jumpAction.WasPressedThisFrame())
            jumpPressed = true;

        RecordPosition();
    }

    void FixedUpdate()
    {
        // Raycast from player center downward (avoids corner detection issues)
        // Distance = half the player height + ground check distance
        float rayDistance = halfHeight + groundCheckRadius;
        RaycastHit2D hit = Physics2D.Raycast(transform.position, Vector2.down, rayDistance, groundLayer);
        bool groundDetected = hit.collider != null;

        // Coyote time: if we detect ground, reset the timer. Otherwise count down.
        if (groundDetected)
        {
            coyoteTimeCounter = coyoteTime;
            isGrounded = true;
        }
        else
        {
            coyoteTimeCounter -= Time.fixedDeltaTime;
            isGrounded = coyoteTimeCounter > 0f;
        }

        bool stacked = peaManager != null && peaManager.IsStackingComplete() && peaManager.GetPeaCount() > 0;

        // Horizontal movement
        float targetVelocityX = horizontalInput * moveSpeed;
        if (horizontalInput != 0)
            currentVelocityX = Mathf.MoveTowards(currentVelocityX, targetVelocityX, acceleration * Time.fixedDeltaTime);
        else
            currentVelocityX = Mathf.MoveTowards(currentVelocityX, 0, deceleration * Time.fixedDeltaTime);

        // If stacked (animation complete), check if the entire stack can move before applying velocity
        if (stacked && Mathf.Abs(currentVelocityX) > 0.01f)
        {
            Vector2 desiredPos = rb.position + new Vector2(currentVelocityX * Time.fixedDeltaTime, 0);
            if (!peaManager.CanStackMove(desiredPos))
            {
                // Stack is blocked by a wall — stop horizontal movement
                currentVelocityX = 0;
            }
        }

        rb.linearVelocity = new Vector2(currentVelocityX, rb.linearVelocity.y);

        // During stacking animation, progressively rise as peas stack
        bool isStacking = peaManager != null && peaManager.IsStacked() && !peaManager.IsStackingComplete();
        if (isStacking && peaManager.GetPeaCount() > 0)
        {
            RaycastHit2D stackHit = Physics2D.Raycast(transform.position, Vector2.down, 20f, groundLayer);
            if (stackHit.collider != null)
            {
                float groundY = stackHit.point.y;
                // Rise based on how many peas have ACTUALLY stacked so far
                int stackedCount = peaManager.GetStackedPeaCount();
                float currentStackHeight = stackedCount * peaManager.GetStackSpacing();
                float targetY = groundY + currentStackHeight + halfHeight;

                // Smoothly lerp toward target height as animation progresses
                float currentY = rb.position.y;
                float newY = Mathf.MoveTowards(currentY, targetY, 15f * Time.fixedDeltaTime);

                rb.position = new Vector2(rb.position.x, newY);
                rb.linearVelocity = new Vector2(rb.linearVelocity.x, 0);
            }
        }
        // Stack floor clamping — only when animation is COMPLETE
        else if (stacked && rb.linearVelocity.y <= 0)
        {
            RaycastHit2D stackHit = Physics2D.Raycast(transform.position, Vector2.down, 20f, groundLayer);
            if (stackHit.collider != null)
            {
                float groundY = stackHit.point.y;
                float stackHeight = peaManager.GetPeaCount() * peaManager.GetStackSpacing();
                float minY = groundY + stackHeight + halfHeight;

                if (rb.position.y < minY)
                {
                    // Directly set Y only — X already correct from velocity above
                    rb.position = new Vector2(rb.position.x, minY);
                    rb.linearVelocity = new Vector2(rb.linearVelocity.x, 0);
                    isGrounded = true;
                }
            }
        }

        // Jump
        if (jumpPressed)
        {
            if (isGrounded)
                rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpForce);
            else
                Debug.Log($"Jump blocked: isGrounded={isGrounded}, position={transform.position}, stacked={stacked}");
        }
        jumpPressed = false;
    }

    private void RecordPosition()
    {
        positionHistory.Enqueue(transform.position);
        if (positionHistory.Count > historySize)
            positionHistory.Dequeue();
    }

    public Vector2 GetHistoricalPosition(int framesAgo)
    {
        framesAgo = Mathf.Clamp(framesAgo, 0, positionHistory.Count - 1);
        Vector2[] historyArray = positionHistory.ToArray();
        return historyArray[historyArray.Length - 1 - framesAgo];
    }

    public int GetHistorySize() => historySize;

    void OnDrawGizmosSelected()
    {
        Gizmos.color = isGrounded ? Color.green : Color.red;
        // Draw the downward raycast from player center
        float rayDistance = halfHeight + groundCheckRadius;
        Vector3 start = transform.position;
        Vector3 end = start + Vector3.down * rayDistance;
        Gizmos.DrawLine(start, end);
        Gizmos.DrawWireSphere(end, 0.05f);
    }
}