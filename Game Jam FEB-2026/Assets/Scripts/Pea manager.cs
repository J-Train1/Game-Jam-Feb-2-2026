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
    [SerializeField] private float stackLerpSpeed = 25f; // How fast peas move into position (higher = faster)

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

        if (keyboard.upArrowKey.wasPressedThisFrame && peaChain.Count > 0 && !isStacked)
        {
            // Start stacking animation
            isStacking = true;
            stackedPeaCount = Mathf.Min(2, peaChain.Count); // Start with 2 peas already stacked
            stackAnimationTimer = 0f;

            // If we have 2 or fewer peas, we're already done
            if (stackedPeaCount >= peaChain.Count)
            {
                isStacking = false;
                isStacked = true;
            }
        }

        if (keyboard.downArrowKey.wasPressedThisFrame && isStacked)
        {
            // Instantly unstack all
            isStacked = false;
            isStacking = false;
            stackedPeaCount = 0;
        }

        // Animate stacking
        if (isStacking)
        {
            stackAnimationTimer += Time.deltaTime;

            if (stackAnimationTimer >= stackAnimationSpeed)
            {
                stackAnimationTimer = 0f;
                stackedPeaCount++;
                Debug.Log($"Stacking animation: {stackedPeaCount} / {peaChain.Count} peas stacked");

                // Once all peas are stacked, finish animation
                if (stackedPeaCount >= peaChain.Count)
                {
                    isStacking = false;
                    isStacked = true;
                    Debug.Log("Stacking complete!");
                }
            }
        }
    }

    void FixedUpdate()
    {
        positionHistory.Insert(0, playerController.transform.position);
        int maxHistory = (peaChain.Count + 1) * frameDelay + 1;
        if (positionHistory.Count > maxHistory)
            positionHistory.RemoveRange(maxHistory, positionHistory.Count - maxHistory);

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
                // Check if this pea has stacked yet
                // Stack from the front (pea 0 first, then 1, then 2...)
                bool thispeaHasStacked = (i < stackedPeaCount) || isStacked;

                if (thispeaHasStacked)
                {
                    // This pea is in stack formation at position (i+1) below player
                    Vector2 targetPos = playerPos + Vector2.down * stackSpacing * (i + 1);
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

            // Update positions - always lerp for smooth movement
            if (!stackBlocked)
            {
                for (int i = 0; i < peaChain.Count; i++)
                {
                    bool thispeaHasStacked = (i < stackedPeaCount) || isStacked;

                    if (thispeaHasStacked)
                    {
                        // Stacked peas lerp smoothly
                        currentPeaPositions[i] = Vector2.MoveTowards(
                            currentPeaPositions[i],
                            desiredPositions[i],
                            stackLerpSpeed * Time.fixedDeltaTime
                        );
                    }
                    else
                    {
                        // Following peas move instantly to follow position
                        currentPeaPositions[i] = desiredPositions[i];
                    }

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