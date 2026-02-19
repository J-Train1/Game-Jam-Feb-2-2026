using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections.Generic;

public class PeaManager : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private PlayerController playerController;
    [SerializeField] private GameObject peaFollowerPrefab;

    [Header("Chain Settings")]
    [SerializeField] private int frameDelay = 8;

    [Header("Stack Settings")]
    [SerializeField] private float stackSpacing = 1.0f;
    [SerializeField] private float stackAnimationSpeed = 0.08f; // Time between each pea stacking
    [SerializeField] private float stackLerpSpeed = 100f; // How fast peas move into position (very high = instant)

    private List<Vector2> positionHistory = new List<Vector2>();
    private List<PeaFollower> peaChain = new List<PeaFollower>();
    private List<Vector2> currentPeaPositions = new List<Vector2>();

    private bool isStacked = false;
    private bool isStacking = false; // Currently animating into stack
    private int stackedPeaCount = 0; // How many peas are currently in stack formation
    private float stackAnimationTimer = 0f;

    void Awake()
    {
        if (playerController == null)
            playerController = GetComponent<PlayerController>();
    }

    void Update()
    {
        var keyboard = Keyboard.current;
        if (keyboard == null) return;

        // Input detection in Update - just set flags
        if (keyboard.upArrowKey.wasPressedThisFrame && peaChain.Count > 0 && !isStacked && !isStacking)
        {
            // Start stacking animation
            Debug.Log("Starting stack animation!");
            isStacking = true;
            isStacked = false; // Not fully stacked yet
            stackedPeaCount = 1; // Start with just 1 pea stacked
            stackAnimationTimer = 0f;

            // Only skip animation if we have no peas (shouldn't happen but safety check)
            if (peaChain.Count == 0)
            {
                isStacking = false;
                isStacked = true;
                stackedPeaCount = 0;
                Debug.Log("No peas to stack!");
            }
        }

        if (keyboard.downArrowKey.wasPressedThisFrame && (isStacked || isStacking))
        {
            Debug.Log("Unstacking!");
            isStacked = false;
            isStacking = false;
            stackedPeaCount = 0;
        }
    }

    void FixedUpdate()
    {
        positionHistory.Insert(0, playerController.transform.position);
        int maxHistory = (peaChain.Count + 1) * frameDelay + 1;
        if (positionHistory.Count > maxHistory)
            positionHistory.RemoveRange(maxHistory, positionHistory.Count - maxHistory);

        // Stacking animation in FixedUpdate - syncs with PlayerController
        if (isStacking)
        {
            stackAnimationTimer += Time.fixedDeltaTime;

            if (stackAnimationTimer >= stackAnimationSpeed)
            {
                stackAnimationTimer = 0f;
                stackedPeaCount++;
                Debug.Log($"Stacking animation: {stackedPeaCount} / {peaChain.Count} peas stacked");

                // Complete animation AFTER last pea has had time to stack
                if (stackedPeaCount > peaChain.Count)
                {
                    isStacking = false;
                    isStacked = true;
                    stackedPeaCount = peaChain.Count; // Cap it
                    Debug.Log("Stacking complete!");

                    // IMMEDIATELY update all pea positions to player-relative (smooth transition)
                    Vector2 playerPos = playerController.transform.position;
                    for (int i = 0; i < peaChain.Count; i++)
                    {
                        Vector2 newPos = playerPos + Vector2.down * stackSpacing * (i + 1);
                        currentPeaPositions[i] = newPos;
                        peaChain[i].SetPosition(newPos, false);
                    }
                }
            }
        }

        UpdatePeaPositions();
    }

    private void UpdatePeaPositions()
    {
        Vector2 playerPos = playerController.transform.position;

        if (isStacked || isStacking)
        {
            // Calculate desired positions for all peas
            List<Vector2> desiredPositions = new List<Vector2>();

            for (int i = 0; i < peaChain.Count; i++)
            {
                bool thispeaHasStacked = (i < stackedPeaCount) || isStacked;

                if (thispeaHasStacked)
                {
                    Vector2 targetPos;

                    if (isStacking)
                    {
                        // DURING ANIMATION: Stack UP from ground (fixed target, player rises independently)
                        RaycastHit2D groundHit = Physics2D.Raycast(playerPos, Vector2.down, 20f, LayerMask.GetMask("Ground"));
                        Vector2 stackBasePos = groundHit.collider != null ? groundHit.point : playerPos;
                        targetPos = stackBasePos + Vector2.up * stackSpacing * (i + 1);
                    }
                    else
                    {
                        // AFTER ANIMATION: Stack DOWN from player (moves as one unit)
                        targetPos = playerPos + Vector2.down * stackSpacing * (i + 1);
                    }

                    desiredPositions.Add(targetPos);
                }
                else
                {
                    // This pea hasn't stacked yet - follow normally
                    int historyIndex = Mathf.Clamp((i + 1) * frameDelay, 0, positionHistory.Count - 1);
                    desiredPositions.Add(positionHistory[historyIndex]);
                }
            }

            // Check if ANY stacked pea would collide with a wall
            bool stackBlocked = false;
            LayerMask groundLayer = LayerMask.GetMask("Ground");
            float peaRadius = 0.5f;

            int peaCountToCheck = isStacked ? peaChain.Count : stackedPeaCount;
            for (int i = 0; i < peaCountToCheck; i++)
            {
                Vector2 currentPos = currentPeaPositions[i];
                Vector2 desiredPos = desiredPositions[i];
                Vector2 movement = desiredPos - currentPos;
                float distance = movement.magnitude;

                if (distance > 0.01f)
                {
                    RaycastHit2D hit = Physics2D.Raycast(currentPos, movement.normalized, distance + peaRadius, groundLayer);
                    if (hit.collider != null)
                    {
                        stackBlocked = true;
                        break;
                    }
                }
            }

            // Update positions
            if (!stackBlocked)
            {
                for (int i = 0; i < peaChain.Count; i++)
                {
                    bool thispeaHasStacked = (i < stackedPeaCount) || isStacked;

                    // All peas snap instantly to their target positions
                    currentPeaPositions[i] = desiredPositions[i];
                    peaChain[i].SetPosition(currentPeaPositions[i], !thispeaHasStacked);
                }
            }
            else
            {
                // Stack blocked - unstacked peas still follow normally
                for (int i = 0; i < peaChain.Count; i++)
                {
                    bool thispeaHasStacked = (i < stackedPeaCount) || isStacked;
                    if (!thispeaHasStacked)
                    {
                        currentPeaPositions[i] = desiredPositions[i];
                        peaChain[i].SetPosition(currentPeaPositions[i], true);
                    }
                }
            }
        }
        else
        {
            // Follow mode: each pea moves independently
            for (int i = 0; i < peaChain.Count; i++)
            {
                int historyIndex = Mathf.Clamp((i + 1) * frameDelay, 0, positionHistory.Count - 1);
                Vector2 targetPos = positionHistory[historyIndex];
                currentPeaPositions[i] = targetPos;
                peaChain[i].SetPosition(currentPeaPositions[i], true);
            }
        }
    }

    // ── Pea collection ───────────────────────────────────────────────────────

    public void CollectPea(GameObject collectiblePea)
    {
        GameObject newPeaObj = Instantiate(peaFollowerPrefab, collectiblePea.transform.position, Quaternion.identity);
        PeaFollower newPea = newPeaObj.GetComponent<PeaFollower>();

        if (newPea != null)
        {
            peaChain.Add(newPea);
            currentPeaPositions.Add((Vector2)collectiblePea.transform.position);
            Debug.Log($"Pea collected! Chain size: {peaChain.Count}");
        }

        Destroy(collectiblePea);
    }

    public float GetStackSpacing() => stackSpacing;
    public bool IsStacked() => isStacked;
    public bool IsStacking() => isStacking; // Animation in progress
    public bool IsStackingComplete() => isStacked && !isStacking; // Only true when fully stacked
    public int GetPeaCount() => peaChain.Count;
    public int GetStackedPeaCount() => stackedPeaCount; // How many peas have stacked so far
    public List<PeaFollower> GetPeaChain() => peaChain;

    // Check if the entire stack (including player) can move in a direction
    public bool CanStackMove(Vector2 playerDesiredPos)
    {
        if (!isStacked || peaChain.Count == 0)
            return true; // Not stacked, move freely

        LayerMask groundLayer = LayerMask.GetMask("Ground");
        float peaRadius = 0.5f;

        // Check player movement
        Vector2 playerCurrentPos = playerController.transform.position;
        Vector2 playerMovement = playerDesiredPos - playerCurrentPos;
        float playerDistance = playerMovement.magnitude;

        if (playerDistance > 0.01f)
        {
            RaycastHit2D hit = Physics2D.Raycast(playerCurrentPos, playerMovement.normalized, playerDistance + peaRadius, groundLayer);
            if (hit.collider != null)
                return false; // Player would hit wall
        }

        // Check each pea's movement based on where they'd need to be relative to new player position
        for (int i = 0; i < peaChain.Count; i++)
        {
            Vector2 peaCurrentPos = currentPeaPositions[i];
            Vector2 peaDesiredPos = playerDesiredPos + Vector2.down * stackSpacing * (i + 1);
            Vector2 peaMovement = peaDesiredPos - peaCurrentPos;
            float peaDistance = peaMovement.magnitude;

            if (peaDistance > 0.01f)
            {
                RaycastHit2D hit = Physics2D.Raycast(peaCurrentPos, peaMovement.normalized, peaDistance + peaRadius, groundLayer);
                if (hit.collider != null)
                    return false; // This pea would hit wall
            }
        }

        return true; // All clear, stack can move
    }
}