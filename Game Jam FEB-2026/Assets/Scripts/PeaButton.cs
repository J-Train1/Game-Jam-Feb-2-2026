using UnityEngine;
using System.Collections;

public class PeaButton : MonoBehaviour
{
    [Header("Button Settings")]
    [SerializeField] private float activationDuration = 3f; // How long door stays open
    [SerializeField] private bool requiresThrownPea = true; // false = any pea works, true = only thrown peas
    [SerializeField] private bool staysActivated = false; // true = button stays pressed forever

    [Header("Connected Objects")]
    [SerializeField] private GameObject[] doorsToOpen; // Doors that open when activated
    [SerializeField] private MovingPlatform[] platformsToMove; // Platforms that start moving

    [Header("Visual Feedback")]
    [SerializeField] private SpriteRenderer buttonSprite;
    [SerializeField] private Color activatedColor = Color.green;
    [SerializeField] private Color deactivatedColor = Color.red;
    [SerializeField] private float pressDepth = 0.2f; // How far button moves down when pressed

    [Header("Audio (Optional)")]
    [SerializeField] private AudioClip activateSound;
    [SerializeField] private AudioClip deactivateSound;

    private bool isActivated = false;
    private Vector3 startPosition;
    private Color originalColor;
    private Coroutine deactivateCoroutine;

    void Start()
    {
        startPosition = transform.position;

        if (buttonSprite != null)
        {
            originalColor = buttonSprite.color;
            buttonSprite.color = deactivatedColor;
        }

        // Make sure doors are closed initially
        SetDoorsActive(false);
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        bool shouldActivate = false;

        // Check if player stepped on button
        if (other.CompareTag("Player"))
        {
            shouldActivate = true;
            Debug.Log("Button activated by player!");
        }
        else if (requiresThrownPea)
        {
            // Only activate if a thrown pea hits it
            ThrownPea thrownPea = other.GetComponent<ThrownPea>();
            if (thrownPea != null)
            {
                shouldActivate = true;
                Debug.Log("Button hit by thrown pea!");
            }
        }
        else
        {
            // Activate if any pea (collectible, follower, or thrown) touches it
            if (other.GetComponent<CollectiblePea>() != null ||
                other.GetComponent<PeaFollower>() != null ||
                other.GetComponent<ThrownPea>() != null)
            {
                shouldActivate = true;
                Debug.Log("Button activated by pea!");
            }
        }

        if (shouldActivate && !isActivated)
        {
            ActivateButton();
        }
    }

    void ActivateButton()
    {
        isActivated = true;

        Debug.Log($"Button activated! Doors open for {activationDuration} seconds");

        // Visual feedback
        if (buttonSprite != null)
        {
            buttonSprite.color = activatedColor;
        }

        // Move button down
        transform.position = startPosition - Vector3.up * pressDepth;

        // Play sound
        if (activateSound != null)
        {
            AudioSource.PlayClipAtPoint(activateSound, transform.position);
        }

        // Open doors
        SetDoorsActive(true);

        // Start deactivation timer (unless permanent)
        if (!staysActivated)
        {
            if (deactivateCoroutine != null)
            {
                StopCoroutine(deactivateCoroutine);
            }
            deactivateCoroutine = StartCoroutine(DeactivateAfterDelay());
        }
    }

    IEnumerator DeactivateAfterDelay()
    {
        yield return new WaitForSeconds(activationDuration);

        DeactivateButton();
    }

    void DeactivateButton()
    {
        isActivated = false;

        Debug.Log("Button deactivated - doors closing");

        // Visual feedback
        if (buttonSprite != null)
        {
            buttonSprite.color = deactivatedColor;
        }

        // Move button up
        transform.position = startPosition;

        // Play sound
        if (deactivateSound != null)
        {
            AudioSource.PlayClipAtPoint(deactivateSound, transform.position);
        }

        // Close doors
        SetDoorsActive(false);
    }

    void SetDoorsActive(bool active)
    {
        // Enable/disable doors
        foreach (GameObject door in doorsToOpen)
        {
            if (door != null)
            {
                door.SetActive(!active); // Active doors = disabled GameObjects (doors open = invisible)
            }
        }

        // Start/stop platforms
        foreach (MovingPlatform platform in platformsToMove)
        {
            if (platform != null)
            {
                platform.enabled = active;
            }
        }
    }
}