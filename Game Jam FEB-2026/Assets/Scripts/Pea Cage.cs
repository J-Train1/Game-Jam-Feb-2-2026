using UnityEngine;

public class PeaCage : MonoBehaviour
{
    [Header("Cage Parts")]
    [SerializeField] private Transform cageDoor; // The bottom door that opens
    [SerializeField] private CagedPea trappedPea; // The pea inside

    [Header("Door Animation")]
    [SerializeField] private float doorOpenAngle = 90f; // How far the door rotates
    [SerializeField] private float doorOpenSpeed = 200f; // Degrees per second
    [SerializeField] private bool doorOpensLeft = true; // true = rotate left, false = rotate right

    [Header("Audio (Optional)")]
    [SerializeField] private AudioClip cageOpenSound;

    private bool isOpen = false;
    private bool isOpening = false;
    private float currentDoorAngle = 0f;
    private Quaternion doorStartRotation;

    void Start()
    {
        if (cageDoor != null)
        {
            doorStartRotation = cageDoor.localRotation;
        }

        // Auto-find trapped pea if not assigned
        if (trappedPea == null)
        {
            trappedPea = GetComponentInChildren<CagedPea>();
        }
    }

    void Update()
    {
        if (isOpening && !isOpen)
        {
            AnimateDoor();
        }
    }

    void OnCollisionEnter2D(Collision2D collision)
    {
        // Check if a thrown pea hit the cage
        ThrownPea thrownPea = collision.gameObject.GetComponent<ThrownPea>();

        if (thrownPea != null && !isOpen && !isOpening)
        {
            Debug.Log("Cage hit by thrown pea - opening!");
            OpenCage();
        }
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        // Also check trigger collisions
        ThrownPea thrownPea = other.GetComponent<ThrownPea>();

        if (thrownPea != null && !isOpen && !isOpening)
        {
            Debug.Log("Cage hit by thrown pea (trigger) - opening!");
            OpenCage();
        }
    }

    void OpenCage()
    {
        isOpening = true;

        // Play sound if available
        if (cageOpenSound != null)
        {
            AudioSource.PlayClipAtPoint(cageOpenSound, transform.position);
        }

        // Release the trapped pea
        if (trappedPea != null)
        {
            trappedPea.Release();
        }
    }

    void AnimateDoor()
    {
        if (cageDoor == null)
        {
            isOpen = true;
            return;
        }

        // Animate door rotation
        currentDoorAngle += doorOpenSpeed * Time.deltaTime;

        if (currentDoorAngle >= doorOpenAngle)
        {
            currentDoorAngle = doorOpenAngle;
            isOpen = true;
            Debug.Log("Cage door fully open!");
        }

        // Apply rotation
        float rotationDirection = doorOpensLeft ? -1f : 1f;
        Quaternion targetRotation = doorStartRotation * Quaternion.Euler(0, 0, currentDoorAngle * rotationDirection);
        cageDoor.localRotation = targetRotation;
    }
}