using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ConstructedBuilding : MonoBehaviour
{
    public string buildingName = "Construcción";

    public List<RequiredResource> originalResources = new List<RequiredResource>();

    public bool isMarkedForDeconstruction = false;
    public bool isReservedForDeconstruction = false;

    private SpriteRenderer spriteRenderer;
    private Color originalColor;

    private void Awake()
    {
        spriteRenderer = GetComponentInChildren<SpriteRenderer>();

        if (spriteRenderer != null)
        {
            originalColor = spriteRenderer.color;
        }
    }

    public void SetMarkedForDeconstruction(bool marked)
    {
        isMarkedForDeconstruction = marked;

        if (spriteRenderer != null)
        {
            spriteRenderer.color = marked ? new Color(1f, 0.45f, 0.45f, 0.75f) : originalColor;
        }
    }
}
