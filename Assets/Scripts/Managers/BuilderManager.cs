using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Tilemaps;

public class BuilderManager : MonoBehaviour
{
    public static BuilderManager Instance;

    public Grid mainGrid;
    public Tilemap resourcesTilemap;
    public LayerMask blockingPlacementLayer;
    public LayerMask blueprintLayer;
    private HashSet<Vector3Int> occupiedBlueprintCells = new HashSet<Vector3Int>();

    public GameObject selectedBlueprintPrefab;
    public bool isBuildModeActive = false;
    private float currentRotation = 0f;

    private GameObject previewObject;
    private SpriteRenderer previewRenderer;

    public Color validPreviewColor = new Color(1f, 1f, 1f, 0.45f);
    public Color invalidPreviewColor = new Color(1f, 0.3f, 0.3f, 0.45f);

    public BuildInfoPanel buildInfoPanel;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    private void Update()
    {
        if (!isBuildModeActive || selectedBlueprintPrefab == null) return;
        if (selectedBlueprintPrefab == null) return;

        UpdatePreview();

        if (Input.GetKeyDown(KeyCode.R))
        {
            RotateSelectedBlueprint();
        }

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
            TryCancelBlueprint();
        }

        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (UIManager.Instance != null)
            {
                UIManager.Instance.CloseBuildMenu();
            }
            else
            {
                CancelBuildMode();
            }
            return;
        }
    }

    public void SelectBlueprint(GameObject blueprintPrefab)
    {
        selectedBlueprintPrefab = blueprintPrefab;
        isBuildModeActive = true;

        currentRotation = 0f;

        CreatePreview();

        if (buildInfoPanel != null && selectedBlueprintPrefab != null)
        {
            Blueprint blueprintData = selectedBlueprintPrefab.GetComponent<Blueprint>();
            buildInfoPanel.Show(blueprintData);
        }
    }

    private bool SelectedBlueprintCanRotate()
    {
        if (selectedBlueprintPrefab == null) return false;

        Blueprint blueprint = selectedBlueprintPrefab.GetComponent<Blueprint>();

        if (blueprint == null) return false;

        return blueprint.canRotate;
    }

    private void CreatePreview()
    {
        DestroyPreview();

        if (selectedBlueprintPrefab == null) return;

        previewObject = Instantiate(selectedBlueprintPrefab);
        previewObject.name = "BuildPreview";
        previewObject.transform.rotation = Quaternion.Euler(0f, 0f, currentRotation);

        Blueprint blueprint = previewObject.GetComponent<Blueprint>();
        if (blueprint != null)
        {
            blueprint.enabled = false;
        }

        Collider2D[] colliders = previewObject.GetComponentsInChildren<Collider2D>();
        foreach(Collider2D col in colliders)
        {
            col.enabled = false;
        }

        previewRenderer = previewObject.GetComponentInChildren<SpriteRenderer>();

        if (previewRenderer != null)
        {
            previewRenderer.color = validPreviewColor;
            previewRenderer.sortingOrder = 50;
        }
    }

    private void DestroyPreview()
    {
        if (previewObject != null)
        {
            Destroy(previewObject);
            previewObject = null;
            previewRenderer = null;
        }
    }

    private void UpdatePreview()
    {
        if (previewObject == null) return;
        if (mainGrid == null) return;

        Vector3 mouseWorldPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        mouseWorldPos.z = 0;

        Vector3Int cellPos = mainGrid.WorldToCell(mouseWorldPos);
        Vector3 placeWorldPos = mainGrid.GetCellCenterWorld(cellPos);

        previewObject.transform.position = placeWorldPos;

        bool canPlace = CanPlaceAt(cellPos);

        if (previewRenderer != null)
        {
            previewRenderer.color = canPlace ? validPreviewColor : invalidPreviewColor;
        }
    }

    public void CancelBuildMode()
    {
        selectedBlueprintPrefab = null;
        isBuildModeActive = false;

        DestroyPreview();

        if (buildInfoPanel != null)
        {
            buildInfoPanel.Hide();
        }
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

        Quaternion rotation = Quaternion.Euler(0f, 0f, currentRotation);
        GameObject newBlueprint = Instantiate(selectedBlueprintPrefab, placeWorldPos, rotation);

        Blueprint blueprint = newBlueprint.GetComponent<Blueprint>();
        Vector2Int size = GetSelectedBlueprintSizeWithRotation();

        if (blueprint != null)
        {
            blueprint.cellPosition = cellPos;
            blueprint.occupiedSize = size;
        }

        foreach (Vector3Int occupiedCell in GetOccupiedCells(cellPos, size))
        {
            occupiedBlueprintCells.Add(occupiedCell);
        }
    }

    private void TryCancelBlueprint()
    {
        Vector3 mouseWorldPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        mouseWorldPos.z = 0;

        Collider2D hit = Physics2D.OverlapPoint(mouseWorldPos, blueprintLayer);

        if (hit == null)
        {
            return;
        }

        Blueprint blueprint = hit.GetComponent<Blueprint>();

        if (blueprint == null)
        {
            blueprint = hit.GetComponentInParent<Blueprint>();
        }

        if (blueprint == null)
        {
            return;
        }

        CancelPlacedBlueprint(blueprint);
    }

    private void CancelPlacedBlueprint(Blueprint blueprint)
    {
        if (blueprint == null) return;

        if (blueprint.isReserved)
        {
            Debug.Log("[BuilderManager] No se puede cancelar, está reservado por un colono");
            return;
        }

        if (blueprint.resourcesDelivered)
        {
            Debug.Log("[BuilderManager] No se puede cancelar, tiene recursos");
            return;
        }

        if (blueprint.isCompleted)
        {
            Debug.Log("[BuilderManager] No se puede cancelar, está completo");
            return;
        }

        if (ConstructionManager.Instance != null)
        {
            ConstructionManager.Instance.UnregisterBlueprint(blueprint);
        }

        UnregisterBlueprintCells(blueprint.cellPosition, blueprint.occupiedSize);

        Destroy(blueprint.gameObject);
    }

    private List<Vector3Int> GetOccupiedCells(Vector3Int originCell, Vector2Int size)
    {
        List<Vector3Int> cells = new List<Vector3Int>();

        for (int x = 0; x < size.x; x++)
        {
            for (int y = 0; y < size.y; y++)
            {
                cells.Add(new Vector3Int(originCell.x + x, originCell.y + y, 0));
            }
        }

        return cells;
    }

    private Vector2Int GetSelectedBlueprintSize()
    {
        if (selectedBlueprintPrefab == null)
        {
            return Vector2Int.one;
        }

        Blueprint blueprint = selectedBlueprintPrefab.GetComponent<Blueprint>();

        if (blueprint == null)
        {
            return Vector2Int.one;
        }

        return blueprint.sizeInCells;
    }

    private Vector2Int GetSelectedBlueprintSizeWithRotation()
    {
        Vector2Int size = GetSelectedBlueprintSize();

        if (!SelectedBlueprintCanRotate())
        {
            return size;
        }

        int normalizedRotation = Mathf.RoundToInt(currentRotation) % 360;

        if (normalizedRotation < 0)
        {
            normalizedRotation += 360;
        }

        if (normalizedRotation == 90 || normalizedRotation == 270)
        {
            return new Vector2Int(size.y, size.x);
        }

        return size;
    }

    private Vector3 GetPlacementWorldPosition(Vector3Int originCell, Vector2Int size)
    {
        Vector3 start = mainGrid.GetCellCenterWorld(originCell);

        Vector3 offset = new Vector3(
            (size.x - 1) * mainGrid.cellSize.x / 2f,
            (size.y - 1) * mainGrid.cellSize.y / 2f,
            0
        );

        return start + offset;
    }

    private bool CanPlaceAt(Vector3Int originCell)
    {
        Vector2Int size = GetSelectedBlueprintSizeWithRotation();
        List<Vector3Int> occupiedCells = GetOccupiedCells(originCell, size);

        foreach (Vector3Int cell in occupiedCells)
        {
            if (resourcesTilemap != null && resourcesTilemap.GetTile(cell) != null) return false;

            if (occupiedBlueprintCells.Contains(cell)) return false;

            Vector3 worldPos = mainGrid.GetCellCenterWorld(cell);
            Collider2D hit = Physics2D.OverlapPoint(worldPos, blockingPlacementLayer);

            if (hit != null)
            {
                return false;
            }
        }

        return true;
    }

    public void UnregisterBlueprintCells(Vector3Int originCell, Vector2Int size)
    {
        foreach (Vector3Int cell in GetOccupiedCells(originCell, size))
        {
            occupiedBlueprintCells.Remove(cell);
        }
    }

    private void RotateSelectedBlueprint()
    {
        if (!SelectedBlueprintCanRotate()) return;

        Blueprint blueprint = selectedBlueprintPrefab.GetComponent<Blueprint>();

        int step = blueprint != null ? blueprint.rotationStep : 90;

        currentRotation += step;

        if (currentRotation >= 360f)
        {
            currentRotation = 0f;
        }

        if (previewObject != null)
        {
            previewObject.transform.rotation = Quaternion.Euler(0f, 0f, currentRotation);
        }
    }
}
