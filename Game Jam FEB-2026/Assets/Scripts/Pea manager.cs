using UnityEngine;
using System.Collections.Generic;

public class PeaManager : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private PlayerController playerController;
    [SerializeField] private GameObject peaFollowerPrefab;

    [Header("Chain Settings")]
    [SerializeField] private int frameDelay = 8; // frames between each pea in the chain

    private List<Vector2> positionHistory = new List<Vector2>();
    private List<PeaFollower> peaChain = new List<PeaFollower>();

    void Awake()
    {
        if (playerController == null)
            playerController = GetComponent<PlayerController>();
    }

    void FixedUpdate()
    {
        // Record player position every single frame, no conditions
        positionHistory.Insert(0, playerController.transform.position);

        // Trim history to only what the longest chain needs
        int maxHistory = (peaChain.Count + 1) * frameDelay + 1;
        if (positionHistory.Count > maxHistory)
            positionHistory.RemoveRange(maxHistory, positionHistory.Count - maxHistory);

        // Each pea replays the player's position from N frames ago
        for (int i = 0; i < peaChain.Count; i++)
        {
            int historyIndex = (i + 1) * frameDelay;
            historyIndex = Mathf.Clamp(historyIndex, 0, positionHistory.Count - 1);
            peaChain[i].SetPosition(positionHistory[historyIndex]);
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
            Debug.Log($"Pea collected! Chain size: {peaChain.Count}");
        }

        Destroy(collectiblePea);
    }

    public int GetPeaCount() => peaChain.Count;
    public List<PeaFollower> GetPeaChain() => peaChain;
}