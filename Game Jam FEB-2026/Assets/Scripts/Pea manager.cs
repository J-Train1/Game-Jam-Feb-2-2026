using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections.Generic;

public class PeaManager : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private PlayerController playerController;
    [SerializeField] private GameObject peaFollowerPrefab;

    [Header("Chain Settings")]
    [SerializeField] private float followSpeed = 8f;

    private List<PeaFollower> peaChain = new List<PeaFollower>();

    // Input Action for debug
    private InputAction debugAction;

    void Awake()
    {
        if (playerController == null)
        {
            playerController = GetComponent<PlayerController>();
        }

        // Create debug input action for pressing P
        debugAction = new InputAction(binding: "<Keyboard>/p");
        debugAction.performed += ctx => OnDebugPressed();
    }

    void OnEnable()
    {
        debugAction?.Enable();
    }

    void OnDisable()
    {
        debugAction?.Disable();
    }

    void OnDestroy()
    {
        debugAction?.Dispose();
    }

    private void OnDebugPressed()
    {
        Debug.Log($"Current pea chain size: {peaChain.Count}");
    }

    public void CollectPea(GameObject collectiblePea)
    {
        // Get position where pea was collected
        Vector3 spawnPosition = collectiblePea.transform.position;

        // Create new follower pea
        GameObject newPeaObj = Instantiate(peaFollowerPrefab, spawnPosition, Quaternion.identity);
        PeaFollower newPea = newPeaObj.GetComponent<PeaFollower>();

        if (newPea != null)
        {
            // Determine what this pea should follow
            PeaFollower previousPea = peaChain.Count > 0 ? peaChain[peaChain.Count - 1] : null;
            int chainIndex = peaChain.Count;

            // Initialize the new pea
            newPea.Initialize(playerController, previousPea, chainIndex);
            newPea.SetFollowSpeed(followSpeed);

            // Add to chain
            peaChain.Add(newPea);

            Debug.Log($"Pea collected! Chain size: {peaChain.Count}");
        }

        // Destroy the collectible
        Destroy(collectiblePea);
    }

    public int GetPeaCount()
    {
        return peaChain.Count;
    }

    public List<PeaFollower> GetPeaChain()
    {
        return peaChain;
    }
}