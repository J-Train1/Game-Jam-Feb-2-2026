using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class HealthUI : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private HealthSystem healthSystem;
    [SerializeField] private GameObject peaIconPrefab;
    [SerializeField] private Transform iconsContainer;

    [Header("UI Settings")]
    [SerializeField] private float iconSpacing = 40f;

    private List<GameObject> currentIcons = new List<GameObject>();
    private int lastHealth = 0;

    void Awake()
    {
        if (healthSystem == null)
        {
            healthSystem = FindObjectOfType<HealthSystem>();
        }
    }

    void Start()
    {
        UpdateHealthUI();
    }

    void Update()
    {
        int currentHealth = healthSystem.GetCurrentHealth();

        if (currentHealth != lastHealth)
        {
            UpdateHealthUI();
            lastHealth = currentHealth;
        }
    }

    void UpdateHealthUI()
    {
        int targetHealth = healthSystem.GetCurrentHealth();

        // Remove excess icons
        while (currentIcons.Count > targetHealth)
        {
            int lastIndex = currentIcons.Count - 1;
            Destroy(currentIcons[lastIndex]);
            currentIcons.RemoveAt(lastIndex);
        }

        // Add missing icons
        while (currentIcons.Count < targetHealth)
        {
            GameObject newIcon = Instantiate(peaIconPrefab, iconsContainer);
            currentIcons.Add(newIcon);
        }

        // Position all icons in a line
        for (int i = 0; i < currentIcons.Count; i++)
        {
            RectTransform rt = currentIcons[i].GetComponent<RectTransform>();
            if (rt != null)
            {
                rt.anchoredPosition = new Vector2(i * iconSpacing, 0);
            }
        }
    }
}