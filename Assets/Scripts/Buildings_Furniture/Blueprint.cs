using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class RequiredResource
{
    public string itemID;
    public int amount;
}

public class Blueprint : MonoBehaviour
{
    [Header("Información del plano")]
    public string blueprintName = "Blueprint";
    public GameObject finalPrefab;

    [Header("Recursos necesarios")]
    public List<RequiredResource> requiredResources = new List<RequiredResource>();

    [Header("Estado del plano")]
    public bool isReserved = false;
    public bool resourcesDelivered = false;
    public bool isCompleted = false;

    [Header("Ocupación en cuadrícula")]
    public Vector2Int sizeInCells = Vector2Int.one;

    [Header("Rotación")]
    public bool canRotate = false;
    public int rotationStep = 90;

    [HideInInspector]
    public Vector3Int cellPosition;
    public Vector2Int occupiedSize;

    private void Start()
    {
        if (ConstructionManager.Instance != null)
        {
            ConstructionManager.Instance.RegisterBlueprint(this);
        }
    }
}
