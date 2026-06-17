using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Tilemaps;

public class BuilderManager : MonoBehaviour
{
    public static BuilderManager Instance;

    [Header("Referencias de escena")]
    public Grid mainGrid;
    public Tilemap resourcesTilemap;

    [Header("Capas de validación")]
    public LayerMask blockingPlacementLayer;
    public LayerMask blueprintLayer;

    private HashSet<Vector3Int> occupiedBlueprintCells = new HashSet<Vector3Int>();
    private Dictionary<Vector3Int, Wall> wallBlueprintsByCell = new Dictionary<Vector3Int, Wall>();

    [Header("Modo de construcción")]
    public GameObject selectedBlueprintPrefab;
    public bool isBuildModeActive = false;

    private float currentRotation = 0f;

    private GameObject previewObject;
    private SpriteRenderer previewRenderer;

    private bool isInitialShelfPlacementMode = false;
    private System.Action<Vector3> onInitialShelfPlaced;

    [Header("Vista previa de colocación")]
    public Color validPreviewColor = new Color(1f, 1f, 1f, 0.45f);
    public Color invalidPreviewColor = new Color(1f, 0.3f, 0.3f, 0.45f);

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
            if (!isInitialShelfPlacementMode)
            {
                TryCancelBlueprint();
            }
        }

        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (isInitialShelfPlacementMode)
            {
                return;
            }

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
    }

    public void StartInitialShelfPlacement(GameObject shelfBlueprintPrefab, System.Action<Vector3> onPlacedCallback)
    {
        if (shelfBlueprintPrefab == null)
        {
            Debug.LogWarning("[BuilderManager] No se ha asignado el blueprint de la estantería inicial.");
            return;
        }

        isInitialShelfPlacementMode = true;
        onInitialShelfPlaced = onPlacedCallback;

        SelectBlueprint(shelfBlueprintPrefab);

        Debug.Log("[BuilderManager] Modo colocación inicial de estantería activado.");
    }

    public bool HasWallBlueprintAt(Vector3Int cell)
    {
        if (!wallBlueprintsByCell.TryGetValue(cell, out Wall wall))
        {
            return false;
        }

        if (wall == null)
        {
            wallBlueprintsByCell.Remove(cell);
            return false;
        }

        return true;
    }

    public Wall GetWallBlueprintAt(Vector3Int cell)
    {
        if (!wallBlueprintsByCell.TryGetValue(cell, out Wall wall))
        {
            return null;
        }

        if (wall == null)
        {
            wallBlueprintsByCell.Remove(cell);
            return null;
        }

        return wall;
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

        if (SelectedBlueprintIsWall())
        {
            previewObject.transform.rotation = Quaternion.identity;
        }
        else
        {
            previewObject.transform.rotation = Quaternion.Euler(0f, 0f, currentRotation);
        }

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

    private bool SelectedBlueprintIsWall()
    {
        if (selectedBlueprintPrefab == null) return false;

        Wall wall = selectedBlueprintPrefab.GetComponentInChildren<Wall>();

        return wall != null;
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
        Vector3 placeWorldPos = GetRotatedPlacementPosition(cellPos);

        previewObject.transform.position = placeWorldPos;

        UpdateWallPreviewSprite(cellPos);

        bool canPlace = CanPlaceAt(cellPos);

        if (previewRenderer != null)
        {
            previewRenderer.color = canPlace ? validPreviewColor : invalidPreviewColor;
        }
    }

    private Vector3 GetRotatedPlacementPosition(Vector3Int cellPos)
    {
        Vector3 basePos = mainGrid.GetCellCenterWorld(cellPos);

        if (SelectedBlueprintIsWall())
        {
            return basePos;
        }

        Vector2Int originalSize = GetSelectedBlueprintSize();
        Vector2Int rotatedSize = GetSelectedBlueprintSizeWithRotation();

        if (!SelectedBlueprintCanRotate())
        {
            return basePos;
        }

        int normalizedRotation = Mathf.RoundToInt(currentRotation) % 360;

        if (normalizedRotation < 0)
        {
            normalizedRotation += 360;
        }

        Vector3 offset = Vector3.zero;

        if (originalSize.x == 2 && originalSize.y == 1)
        {
            if (normalizedRotation == 90)
            {
                offset = new Vector3(0.5f, -0.5f, 0f);
            }
            else if (normalizedRotation == 270)
            {
                offset = new Vector3(-0.5f, 0.5f, 0f);
            }
        }

        return basePos + offset;
    }

    private void UpdateWallPreviewSprite(Vector3Int cellPos)
    {
        if (previewObject == null) return;
        if (WallManager.Instance == null) return;

        Wall wall = previewObject.GetComponentInChildren<Wall>();

        if (wall == null) return;

        SpriteRenderer sr = previewObject.GetComponentInChildren<SpriteRenderer>();

        if (sr == null) return;

        Sprite previewSprite = WallManager.Instance.GetPreviewSprite(
            cellPos,
            currentRotation,
            wall.Material
        );

        if (previewSprite != null)
        {
            sr.sprite = previewSprite;
        }
    }

    public void CancelBuildMode()
    {
        if (isInitialShelfPlacementMode)
        {
            return;
        }

        selectedBlueprintPrefab = null;
        isBuildModeActive = false;

        DestroyPreview();
    }

    private void TryPlaceBlueprint()
    {
        Vector3 mouseWorldPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        mouseWorldPos.z = 0;

        Vector3Int cellPos = mainGrid.WorldToCell(mouseWorldPos);
        Vector3 placeWorldPos = GetRotatedPlacementPosition(cellPos);

        if (!CanPlaceAt(cellPos))
        {
            if (AudioManager.Instance != null)
            {
                AudioManager.Instance.PlayNotAllowed();
            }

            return;
        }

        if (isInitialShelfPlacementMode)
        {
            PlaceInitialShelf(cellPos, placeWorldPos);
            return;
        }

        Quaternion rotation = SelectedBlueprintIsWall()
            ? Quaternion.identity
            : Quaternion.Euler(0f, 0f, currentRotation);

        GameObject newBlueprint = Instantiate(selectedBlueprintPrefab, placeWorldPos, rotation);

        Blueprint blueprint = newBlueprint.GetComponent<Blueprint>();
        Vector2Int size = GetSelectedBlueprintSizeWithRotation();

        if (blueprint != null)
        {
            blueprint.cellPosition = cellPos;
            blueprint.occupiedSize = size;
        }

        ApplyWallSpriteToPlacedBlueprint(newBlueprint, cellPos, currentRotation);
        RegisterWallBlueprint(newBlueprint, cellPos);
        RefreshWallBlueprintsAround(cellPos);

        foreach (Vector3Int occupiedCell in GetOccupiedCells(cellPos, size))
        {
            occupiedBlueprintCells.Add(occupiedCell);
        }
    }

    private void PlaceInitialShelf(Vector3Int cellPos, Vector3 placeWorldPos)
    {
        if (selectedBlueprintPrefab == null) return;

        Blueprint blueprintData = selectedBlueprintPrefab.GetComponent<Blueprint>();

        if (blueprintData == null)
        {
            Debug.LogWarning("[BuilderManager] El prefab de estantería inicial no tiene componente Blueprint.");
            return;
        }

        if (blueprintData.finalPrefab == null)
        {
            Debug.LogWarning("[BuilderManager] El blueprint de estantería inicial no tiene finalPrefab asignado.");
            return;
        }

        Quaternion rotation = Quaternion.Euler(0f, 0f, currentRotation);

        GameObject shelf = Instantiate(
            blueprintData.finalPrefab,
            placeWorldPos,
            rotation
        );

        Vector2Int size = GetSelectedBlueprintSizeWithRotation();

        foreach (Vector3Int occupiedCell in GetOccupiedCells(cellPos, size))
        {
            occupiedBlueprintCells.Add(occupiedCell);
        }

        isInitialShelfPlacementMode = false;
        isBuildModeActive = false;
        selectedBlueprintPrefab = null;

        DestroyPreview();

        onInitialShelfPlaced?.Invoke(placeWorldPos);
        onInitialShelfPlaced = null;

        Debug.Log("[BuilderManager] Estantería inicial colocada.");
    }

    private void ApplyWallSpriteToPlacedBlueprint(GameObject blueprintObject, Vector3Int cellPos, float rotation)
    {
        if (blueprintObject == null) return;
        if (WallManager.Instance == null) return;

        Wall wall = blueprintObject.GetComponentInChildren<Wall>();

        if (wall == null) return;

        wall.Initialize(cellPos, rotation);

        SpriteRenderer sr = blueprintObject.GetComponentInChildren<SpriteRenderer>();

        if (sr == null) return;

        Sprite sprite = WallManager.Instance.GetPreviewSprite(
            cellPos,
            rotation,
            wall.Material
        );

        if (sprite != null)
        {
            sr.sprite = sprite;
        }
    }

    private void RegisterWallBlueprint(GameObject blueprintObject, Vector3Int cellPos)
    {
        if (blueprintObject == null) return;

        Wall wall = blueprintObject.GetComponentInChildren<Wall>();

        if (wall == null) return;

        wallBlueprintsByCell[cellPos] = wall;
    }

    private void UnregisterWallBlueprint(Vector3Int cellPos)
    {
        if (wallBlueprintsByCell.ContainsKey(cellPos))
        {
            wallBlueprintsByCell.Remove(cellPos);
        }
    }

    private void RefreshWallBlueprintsAround(Vector3Int cellPos)
    {
        if (WallManager.Instance != null)
        {
            WallManager.Instance.RefreshWallAndNeighbours(cellPos);
        }

        RefreshWallBlueprintSpriteAt(cellPos);
        RefreshWallBlueprintSpriteAt(cellPos + Vector3Int.up);
        RefreshWallBlueprintSpriteAt(cellPos + Vector3Int.down);
        RefreshWallBlueprintSpriteAt(cellPos + Vector3Int.left);
        RefreshWallBlueprintSpriteAt(cellPos + Vector3Int.right);
    }

    private void RefreshWallBlueprintSpriteAt(Vector3Int cellPos)
    {
        if (WallManager.Instance == null) return;

        Wall wall = GetWallBlueprintAt(cellPos);

        if (wall == null) return;

        SpriteRenderer sr = wall.GetComponentInChildren<SpriteRenderer>();

        if (sr == null) return;

        Sprite sprite = WallManager.Instance.GetPreviewSprite(
            cellPos,
            wall.ManualRotation,
            wall.Material
        );

        if (sprite != null)
        {
            sr.sprite = sprite;
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
            // Debug.Log("[BuilderManager] No se puede cancelar, está reservado por un colono");
            return;
        }

        if (blueprint.resourcesDelivered)
        {
            // Debug.Log("[BuilderManager] No se puede cancelar, tiene recursos");
            return;
        }

        if (blueprint.isCompleted)
        {
            // Debug.Log("[BuilderManager] No se puede cancelar, está completo");
            return;
        }

        if (ConstructionManager.Instance != null)
        {
            ConstructionManager.Instance.UnregisterBlueprint(blueprint);
        }

        UnregisterBlueprintCells(blueprint.cellPosition, blueprint.occupiedSize);

        UnregisterWallBlueprint(blueprint.cellPosition);
        RefreshWallBlueprintsAround(blueprint.cellPosition);

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
        if (SelectedBlueprintIsWall())
        {
            return GetSelectedBlueprintSize();
        }

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
        if (SelectedBlueprintIsWall())
        {
            currentRotation = Mathf.RoundToInt(currentRotation) == 180 ? 0f : 180f;

            if (previewObject != null)
            {
                previewObject.transform.rotation = Quaternion.identity;

                Vector3 mouseWorldPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
                mouseWorldPos.z = 0;

                Vector3Int cellPos = mainGrid.WorldToCell(mouseWorldPos);
                UpdateWallPreviewSprite(cellPos);
            }

            return;
        }

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
