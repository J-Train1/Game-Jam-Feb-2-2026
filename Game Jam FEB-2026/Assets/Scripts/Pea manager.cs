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

    private List<Vector2> positionHistory = new List<Vector2>();
    private List<PeaFollower> peaChain = new List<PeaFollower>();
    private List<Vector2> currentPeaPositions = new List<Vector2>();

    private bool isStacked = false;

    void Awake()
    {
        if (playerController == null)
            playerController = GetComponent<PlayerController>();
    }

    void Update()
    {
        var keyboard = Keyboard.current;
        if (keyboard == null) return;

        if (keyboard.upArrowKey.wasPressedThisFrame && peaChain.Count > 0)
            isStacked = true;

        if (keyboard.downArrowKey.wasPressedThisFrame)
            isStacked = false;
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

        if (isStacked)
        {
            // When stacked, check if the ENTIRE stack can move before moving any pea
            // Calculate where each pea wants to go
            List<Vector2> desiredPositions = new List<Vector2>();
            for (int i = 0; i < peaChain.Count; i++)
            {
                Vector2 targetPos = playerPos + Vector2.down * stackSpacing * (i + 1);
                desiredPositions.Add(targetPos);
            }

            // Check if ANY pea would collide with a wall
            bool stackBlocked = false;
            LayerMask groundLayer = LayerMask.GetMask("Ground"); // Make sure your ground is on "Ground" layer
            float peaRadius = 0.5f;

            for (int i = 0; i < peaChain.Count; i++)
            {
                Vector2 currentPos = currentPeaPositions[i];
                Vector2 desiredPos = desiredPositions[i];
                Vector2 movement = desiredPos - currentPos;
                float distance = movement.magnitude;

                if (distance > 0.01f)
                {
                    // Raycast from current position toward desired position
                    RaycastHit2D hit = Physics2D.Raycast(currentPos, movement.normalized, distance + peaRadius, groundLayer);
                    if (hit.collider != null)
                    {
                        // This pea would hit a wall — block the entire stack
                        stackBlocked = true;
                        break;
                    }
                }
            }

            // If stack is blocked, don't move any pea. Otherwise move all peas.
            if (!stackBlocked)
            {
                for (int i = 0; i < peaChain.Count; i++)
                {
                    currentPeaPositions[i] = desiredPositions[i];
                    peaChain[i].SetPosition(currentPeaPositions[i], false); // false = skip individual collision check
                }
            }
            // If blocked, peas stay where they are (currentPeaPositions unchanged)
        }
        else
        {
            // Follow mode: each pea moves independently
            for (int i = 0; i < peaChain.Count; i++)
            {
                int historyIndex = Mathf.Clamp((i + 1) * frameDelay, 0, positionHistory.Count - 1);
                Vector2 targetPos = positionHistory[historyIndex];
                currentPeaPositions[i] = targetPos;
                peaChain[i].SetPosition(currentPeaPositions[i], true); // true = do individual collision check
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
    public int GetPeaCount() => peaChain.Count;
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