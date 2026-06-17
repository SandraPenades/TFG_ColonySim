using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum WallMaterial
{
    Wood,
    Stone
}

public class Wall : MonoBehaviour
{
    [SerializeField] private SpriteRenderer spriteRenderer;

    [Header("Material de la pared")]
    [SerializeField] private WallMaterial wallMaterial = WallMaterial.Wood;

    public Vector3Int CellPosition { get; private set; }
    public float ManualRotation { get; private set; }
    public WallMaterial Material => wallMaterial;

    private void Awake()
    {
        if (spriteRenderer == null)
        {
            spriteRenderer = GetComponentInChildren<SpriteRenderer>();
        }
    }

    public void Initialize(Vector3Int cellPosition, float manualRotation)
    {
        CellPosition = cellPosition;
        ManualRotation = manualRotation;
    }

    public void SetSprite(Sprite sprite)
    {
        if (spriteRenderer == null)
        {
            spriteRenderer = GetComponentInChildren<SpriteRenderer>();
        }

        if (spriteRenderer != null && sprite != null)
        {
            spriteRenderer.sprite = sprite;
        }
    }
}