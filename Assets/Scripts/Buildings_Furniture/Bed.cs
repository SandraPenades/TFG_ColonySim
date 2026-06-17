using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bed : MonoBehaviour
{
    [Header("Estado de la cama")]
    public bool isReserved = false;
    public bool isOccupied = false;

    [Header("Representación visual")]
    [SerializeField] private SpriteRenderer spriteRenderer;
    [SerializeField] private Sprite emptyBedSprite;
    [SerializeField] private Sprite occupiedBedSprite;

    private void Awake()
    {
        if (spriteRenderer == null)
        {
            spriteRenderer = GetComponentInChildren<SpriteRenderer>();
        }

        UpdateVisual();
    }

    public void SetReserved(bool reserved)
    {
        isReserved = reserved;
    }

    public void SetOccupied(bool occupied)
    {
        isOccupied = occupied;

        if (occupied)
        {
            isReserved = true;
        }

        UpdateVisual();
    }

    public void ClearBed()
    {
        isOccupied = false;
        isReserved = false;

        UpdateVisual();
    }

    private void UpdateVisual()
    {
        if (spriteRenderer == null) return;

        if (isOccupied && occupiedBedSprite != null)
        {
            spriteRenderer.sprite = occupiedBedSprite;
        }
        else if (emptyBedSprite != null)
        {
            spriteRenderer.sprite = emptyBedSprite;
        }
    }
}