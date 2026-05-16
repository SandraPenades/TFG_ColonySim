using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Tilemaps;

public class BuilderManager : MonoBehaviour
{
    public static BuilderManager Instance;

    public Grid mainGrid;
    public Tilemap obstaclesTilemap;
    public LayerMask blockingPlacementLayer;
    private HashSet<Vector3Int> occupiedBlueprintCells = new HashSet<Vector3Int>();

    public GameObject selectedBlueprintPrefab;
    public bool isBuildModeActive = false;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    private void Update()
    {
        if (!isBuildModeActive || selectedBlueprintPrefab == null) return;

        if (EventSystem.current != null && EventSystem.current.IsPointerOverGameObject())
        {
            return;
        }

        if (Input.GetMouseButtonDown(0))
        {
            TryPlaceBlueprint();
        }

        if (Input.GetMouseButtonDown(1))
        {
            CancelBuildMode();
        }
    }

    public void SelectBlueprint(GameObject blueprintPrefab)
    {
        selectedBlueprintPrefab = blueprintPrefab;
        isBuildModeActive = true;
    }

    public void CancelBuildMode()
    {
        selectedBlueprintPrefab = null;
        isBuildModeActive = false;
    }

    private void TryPlaceBlueprint()
    {
        Vector3 mouseWorldPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        mouseWorldPos.z = 0;

        Vector3Int cellPos = mainGrid.WorldToCell(mouseWorldPos);
        Vector3 placeWorldPos = mainGrid.GetCellCenterWorld(cellPos);

        if (!CanPlaceAt(cellPos))
        {
            Debug.Log("[BuilderManager] No se puede colocar aquí.");
            return;
        }

        GameObject newBlueprint = Instantiate(selectedBlueprintPrefab, placeWorldPos, Quaternion.identity);

        Blueprint blueprint = newBlueprint.GetComponent<Blueprint>();

        if (blueprint != null)
        {
            blueprint.cellPosition = cellPos;
        }

        occupiedBlueprintCells.Add(cellPos);
    }

    private bool CanPlaceAt(Vector3Int cellPos)
    {
        if (obstaclesTilemap != null && obstaclesTilemap.GetTile(cellPos) != null) return false;
        if (occupiedBlueprintCells.Contains(cellPos)) return false;

        Vector3 worldPos = mainGrid.GetCellCenterWorld(cellPos);
        Collider2D hit = Physics2D.OverlapPoint(worldPos, blockingPlacementLayer);

        if (hit != null) return false;

        return true;
    }

    public void UnregisterBlueprintCell(Vector3Int cellPos)
    {
        if (occupiedBlueprintCells.Contains(cellPos))
        {
            occupiedBlueprintCells.Remove(cellPos);
        }
    }
}
