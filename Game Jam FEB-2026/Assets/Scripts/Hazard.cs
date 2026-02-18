using UnityEngine;

public class Hazard : MonoBehaviour
{
    void Awake()
    {
        // Make sure this object is tagged as "Hazard"
        if (!gameObject.CompareTag("Hazard"))
        {
            Debug.LogWarning($"{gameObject.name} has Hazard script but is not tagged as 'Hazard'. Adding tag.");
            gameObject.tag = "Hazard";
        }

        // Make sure it has a trigger collider
        Collider2D col = GetComponent<Collider2D>();
        if (col != null)
        {
            col.isTrigger = true;
        }
        else
        {
            Debug.LogError($"{gameObject.name} has Hazard script but no Collider2D!");
        }
    }
}