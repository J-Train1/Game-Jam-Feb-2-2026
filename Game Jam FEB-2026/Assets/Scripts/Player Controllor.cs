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
    [SerializeField] private int historySize = 120; // 2 seconds at 60fps

    private Rigidbody2D rb;
    private float horizontalInput;
    private bool jumpPressed;
    private bool isGrounded;
    private float currentVelocityX;

    // Position history for followers
    private Queue<Vector2> positionHistory = new Queue<Vector2>();

    // Input Actions
    private PlayerInput playerInput;
    private InputAction moveAction;
    private InputAction jumpAction;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();

        // Initialize position history
        for (int i = 0; i < historySize; i++)
        {
            positionHistory.Enqueue(transform.position);
        }

        // Get PlayerInput component (add it in Inspector if not present)
        playerInput = GetComponent<PlayerInput>();

        if (playerInput != null)
        {
            // Get actions from the Input Action Asset
            moveAction = playerInput.actions["Move"];
            jumpAction = playerInput.actions["Jump"];
        }
        else
        {
            Debug.LogWarning("PlayerInput component not found. Add it to use Input System.");
        }
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
        // Get input from new Input System
        if (moveAction != null)
        {
            horizontalInput = moveAction.ReadValue<Vector2>().x;
        }

        if (jumpAction != null && jumpAction.WasPressedThisFrame())
        {
            jumpPressed = true;
        }

        // Record position history every frame
        RecordPosition();

        // Debug logging
        if (horizontalInput != 0)
        {
            Debug.Log($"Horizontal Input: {horizontalInput}");
        }
        if (jumpPressed)
        {
            Debug.Log($"Jump Pressed! Grounded: {isGrounded}");
        }
    }

    void FixedUpdate()
    {
        // Ground check
        isGrounded = Physics2D.OverlapCircle(groundCheck.position, groundCheckRadius, groundLayer);

        // Handle horizontal movement with acceleration/deceleration
        float targetVelocityX = horizontalInput * moveSpeed;

        if (horizontalInput != 0)
        {
            // Accelerate
            currentVelocityX = Mathf.MoveTowards(currentVelocityX, targetVelocityX, acceleration * Time.fixedDeltaTime);
        }
        else
        {
            // Decelerate
            currentVelocityX = Mathf.MoveTowards(currentVelocityX, 0, deceleration * Time.fixedDeltaTime);
        }

        rb.linearVelocity = new Vector2(currentVelocityX, rb.linearVelocity.y);

        // Handle jump
        if (jumpPressed && isGrounded)
        {
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpForce);
            Debug.Log("JUMP!");
        }

        jumpPressed = false;

        // Debug every 60 frames
        if (Time.frameCount % 60 == 0)
        {
            Debug.Log($"Grounded: {isGrounded}, VelocityX: {currentVelocityX}, VelocityY: {rb.linearVelocity.y}");
        }
    }

    private void RecordPosition()
    {
        // Add current position to history
        positionHistory.Enqueue(transform.position);

        // Remove oldest position if we exceed history size
        if (positionHistory.Count > historySize)
        {
            positionHistory.Dequeue();
        }
    }

    public Vector2 GetHistoricalPosition(int framesAgo)
    {
        // Clamp to valid range
        framesAgo = Mathf.Clamp(framesAgo, 0, positionHistory.Count - 1);

        // Convert queue to array to access by index
        Vector2[] historyArray = positionHistory.ToArray();

        // Get position from history (newer positions are at the end)
        int index = historyArray.Length - 1 - framesAgo;
        return historyArray[index];
    }

    public int GetHistorySize()
    {
        return historySize;
    }

    // Visual debug for ground check
    void OnDrawGizmosSelected()
    {
        if (groundCheck != null)
        {
            Gizmos.color = isGrounded ? Color.green : Color.red;
            Gizmos.DrawWireSphere(groundCheck.position, groundCheckRadius);
        }
    }
}