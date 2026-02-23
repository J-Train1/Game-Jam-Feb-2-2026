using UnityEngine;

public class RotatingHazard : MonoBehaviour
{
    [Header("Rotation Settings")]
    [SerializeField] private float rotationSpeed = 90f; // Degrees per second
    [SerializeField] private bool rotateClockwise = true;

    [Header("Hazard Settings")]
    [SerializeField] private string hazardTag = "Hazard"; // Tag for damage detection

    void Start()
    {
        // Make sure this GameObject and its children have the Hazard tag
        gameObject.tag = hazardTag;

        // Tag all children too (for multi-part hazards like spinning blades)
        foreach (Transform child in transform)
        {
            child.gameObject.tag = hazardTag;
        }
    }

    void Update()
    {
        // Rotate continuously
        float rotationAmount = rotationSpeed * Time.deltaTime;

        if (!rotateClockwise)
        {
            rotationAmount = -rotationAmount;
        }

        transform.Rotate(0, 0, rotationAmount);
    }

    void OnDrawGizmos()
    {
        // Draw rotation direction indicator in editor
        Gizmos.color = Color.red;

        // Draw circle showing rotation range
        float radius = 1f;
        int segments = 20;

        for (int i = 0; i < segments; i++)
        {
            float angle1 = (i / (float)segments) * 360f * Mathf.Deg2Rad;
            float angle2 = ((i + 1) / (float)segments) * 360f * Mathf.Deg2Rad;

            Vector3 point1 = transform.position + new Vector3(Mathf.Cos(angle1), Mathf.Sin(angle1)) * radius;
            Vector3 point2 = transform.position + new Vector3(Mathf.Cos(angle2), Mathf.Sin(angle2)) * radius;

            Gizmos.DrawLine(point1, point2);
        }

        // Draw arrow showing direction
        Vector3 arrowStart = transform.position + Vector3.right * radius;
        Vector3 arrowDirection = rotateClockwise ? Vector3.down : Vector3.up;
        Gizmos.DrawRay(arrowStart, arrowDirection * 0.5f);
    }
}