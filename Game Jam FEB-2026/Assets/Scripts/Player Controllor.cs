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
    [SerializeField] private float groundCheckRadius = 0.2f;
    [SerializeField] private LayerMask groundLayer;

    [Header("Position History")]
    [SerializeField] private int historySize = 120;

    private Rigidbody2D rb;
    private float horizontalInput;
    private bool jumpPressed;
    private bool isGrounded;
    private float currentVelocityX;
    private float halfHeight = 0.5f;

    private Queue<Vector2> positionHistory = new Queue<Vector2>();

    private PlayerInput playerInput;
    private InputAction moveAction;
    private InputAction jumpAction;

    private PeaManager peaManager;

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
        isGrounded = Physics2D.OverlapCircle(groundCheck.position, groundCheckRadius, groundLayer);

        bool stacked = peaManager != null && peaManager.IsStacked() && peaManager.GetPeaCount() > 0;

        // Horizontal movement
        float targetVelocityX = horizontalInput * moveSpeed;
        if (horizontalInput != 0)
            currentVelocityX = Mathf.MoveTowards(currentVelocityX, targetVelocityX, acceleration * Time.fixedDeltaTime);
        else
            currentVelocityX = Mathf.MoveTowards(currentVelocityX, 0, deceleration * Time.fixedDeltaTime);

        // If stacked, check if the entire stack can move before applying velocity
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

        // Stack floor clamping — only clamp Y, never touch X, only when falling or still
        if (stacked && rb.linearVelocity.y <= 0)
        {
            RaycastHit2D hit = Physics2D.Raycast(transform.position, Vector2.down, 20f, groundLayer);
            if (hit.collider != null)
            {
                float groundY = hit.point.y;
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
        if (groundCheck != null)
        {
            Gizmos.color = isGrounded ? Color.green : Color.red;
            Gizmos.DrawWireSphere(groundCheck.position, groundCheckRadius);
        }
    }
}