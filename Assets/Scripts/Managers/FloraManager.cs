using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
using NavMeshPlus.Components;

public class FloraManager : MonoBehaviour
{
    public static FloraManager Instance;

    [Header("Tilemaps")]
    public Tilemap groundMap;
    public Tilemap resourcesMap;

    [Header("Arbustos")]
    public TileBase fullBushTile;
    public TileBase emptyBushTile;
    public float bushRegrowTime = 30f;

    [Header("Terreno válido")]
    public TileBase[] grassTiles;

    [Header("Árboles")]
    public TileBase[] saplingTreeTiles;
    public TileBase[] adultTreeTiles;

    [Header("Generación de árboles")]
    [SerializeField] private float treeSpawnCheckTime = 45f;
    [SerializeField] private float treeSpawnChance = 0.35f;
    [SerializeField] private float saplingGrowTime = 120f;
    [SerializeField] private int maxTreeSpawnAttempts = 30;

    [Header("Límites de población vegetal")]
    [SerializeField] private int maxSaplingsAtSameTime = 5;
    [SerializeField] private int maxAdultTrees = 80;
    [SerializeField] private int minDistanceFromOtherTrees = 3;

    [Header("Navegación")]
    [SerializeField] private NavMeshSurface navSurface;

    private Dictionary<Vector3Int, int> growingSaplings = new Dictionary<Vector3Int, int>();

    private HashSet<Vector3Int> initialAdultTreeCells = new HashSet<Vector3Int>();

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    private void Start()
    {
        if (navSurface == null)
        {
            navSurface = FindFirstObjectByType<NavMeshSurface>();
        }

        if (!TreeListsAreValid())
        {
            Debug.LogWarning("[FloraManager] Revisa las listas de saplings y árboles adultos.");
        }

        ScanInitialFlora();

        StartCoroutine(TreeSpawnRoutine());
    }

    private void ScanInitialFlora()
    {
        if (resourcesMap == null) return;

        BoundsInt bounds = resourcesMap.cellBounds;

        foreach (Vector3Int pos in bounds.allPositionsWithin)
        {
            TileBase tileAtPos = resourcesMap.GetTile(pos);

            if (tileAtPos == null) continue;

            if (IsAdultTree(tileAtPos))
            {
                initialAdultTreeCells.Add(pos);
                continue;
            }

            if (tileAtPos == emptyBushTile)
            {
                StartBushRegrowth(pos);
                continue;
            }

            int saplingIndex = GetSaplingIndex(tileAtPos);

            if (saplingIndex != -1)
            {
                StartSaplingGrowth(pos, saplingIndex);
            }
        }
    }

    // BAYAS

    public void StartBushRegrowth(Vector3Int cellPosition)
    {
        StartCoroutine(BushRegrowRoutine(cellPosition));
    }

    private IEnumerator BushRegrowRoutine(Vector3Int pos)
    {
        yield return new WaitForSeconds(bushRegrowTime);

        if (resourcesMap == null) yield break;

        TileBase currentTile = resourcesMap.GetTile(pos);

        if (currentTile != emptyBushTile)
        {
            yield break;
        }

        if (fullBushTile != null)
        {
            resourcesMap.SetTile(pos, fullBushTile);

            if (ZoneManager.Instance != null)
            {
                ZoneManager.Instance.UpdateJobAtPosition(pos);
            }
        }
    }

    // ÁRBOLES

    private IEnumerator TreeSpawnRoutine()
    {
        while (true)
        {
            yield return new WaitForSeconds(treeSpawnCheckTime);

            TrySpawnOneSapling();
        }
    }

    private void TrySpawnOneSapling()
    {
        if (groundMap == null) return;
        if (resourcesMap == null) return;
        if (!TreeListsAreValid()) return;

        if (growingSaplings.Count >= maxSaplingsAtSameTime)
        {
            return;
        }

        if (CountAdultTrees() >= maxAdultTrees)
        {
            return;
        }

        if (Random.value > treeSpawnChance)
        {
            return;
        }

        for (int attempt = 0; attempt < maxTreeSpawnAttempts; attempt++)
        {
            Vector3Int randomCell = GetRandomGroundCell();

            if (!CanSpawnSaplingAt(randomCell))
            {
                continue;
            }

            int randomTreeIndex = Random.Range(0, saplingTreeTiles.Length);

            SpawnSapling(randomCell, randomTreeIndex);
            return;
        }
    }

    private Vector3Int GetRandomGroundCell()
    {
        BoundsInt bounds = groundMap.cellBounds;

        int randomX = Random.Range(bounds.xMin, bounds.xMax);
        int randomY = Random.Range(bounds.yMin, bounds.yMax);

        return new Vector3Int(randomX, randomY, 0);
    }

    private bool CanSpawnSaplingAt(Vector3Int cellPosition)
    {
        if (!IsGrassTile(cellPosition))
        {
            return false;
        }

        TileBase resourceTile = resourcesMap.GetTile(cellPosition);

        if (resourceTile != null)
        {
            return false;
        }

        if (growingSaplings.ContainsKey(cellPosition))
        {
            return false;
        }

        if (initialAdultTreeCells.Contains(cellPosition))
        {
            return false;
        }

        if (IsNearAnotherTreeOrSapling(cellPosition))
        {
            return false;
        }

        return true;
    }

    private bool IsGrassTile(Vector3Int cellPosition)
    {
        if (groundMap == null) return false;

        TileBase groundTile = groundMap.GetTile(cellPosition);

        if (groundTile == null)
        {
            return false;
        }

        if (grassTiles == null || grassTiles.Length == 0)
        {
            return true;
        }

        foreach (TileBase grassTile in grassTiles)
        {
            if (groundTile == grassTile)
            {
                return true;
            }
        }

        return false;
    }

    private bool IsNearAnotherTreeOrSapling(Vector3Int centerCell)
    {
        for (int x = -minDistanceFromOtherTrees; x <= minDistanceFromOtherTrees; x++)
        {
            for (int y = -minDistanceFromOtherTrees; y <= minDistanceFromOtherTrees; y++)
            {
                Vector3Int checkCell = new Vector3Int(
                    centerCell.x + x,
                    centerCell.y + y,
                    0
                );

                TileBase tile = resourcesMap.GetTile(checkCell);

                if (IsSapling(tile) || IsAdultTree(tile))
                {
                    return true;
                }
            }
        }

        return false;
    }

    private void SpawnSapling(Vector3Int cellPosition, int treeIndex)
    {
        if (!IsValidTreeIndex(treeIndex)) return;

        // Seguridad extra: si algo ha aparecido ahí entre medias, no escribimos.
        if (resourcesMap.GetTile(cellPosition) != null)
        {
            return;
        }

        resourcesMap.SetTile(cellPosition, saplingTreeTiles[treeIndex]);

        StartSaplingGrowth(cellPosition, treeIndex);

        Debug.Log("[FloraManager] Ha nacido un sapling en " + cellPosition);
    }

    private void StartSaplingGrowth(Vector3Int cellPosition, int treeIndex)
    {
        if (!IsValidTreeIndex(treeIndex)) return;

        if (growingSaplings.ContainsKey(cellPosition))
        {
            return;
        }

        if (initialAdultTreeCells.Contains(cellPosition))
        {
            return;
        }

        growingSaplings.Add(cellPosition, treeIndex);

        StartCoroutine(SaplingGrowRoutine(cellPosition, treeIndex));
    }

    private IEnumerator SaplingGrowRoutine(Vector3Int cellPosition, int treeIndex)
    {
        yield return new WaitForSeconds(saplingGrowTime);

        if (resourcesMap == null)
        {
            growingSaplings.Remove(cellPosition);
            yield break;
        }

        if (!IsValidTreeIndex(treeIndex))
        {
            growingSaplings.Remove(cellPosition);
            yield break;
        }

        if (initialAdultTreeCells.Contains(cellPosition))
        {
            growingSaplings.Remove(cellPosition);
            yield break;
        }

        TileBase currentTile = resourcesMap.GetTile(cellPosition);

        if (currentTile != saplingTreeTiles[treeIndex])
        {
            growingSaplings.Remove(cellPosition);
            yield break;
        }

        resourcesMap.SetTile(cellPosition, adultTreeTiles[treeIndex]);

        growingSaplings.Remove(cellPosition);

        if (ZoneManager.Instance != null)
        {
            ZoneManager.Instance.UpdateJobAtPosition(cellPosition);
        }

        RebuildNavMesh();
    }

    private int GetSaplingIndex(TileBase tile)
    {
        if (tile == null || saplingTreeTiles == null) return -1;

        for (int i = 0; i < saplingTreeTiles.Length; i++)
        {
            if (tile == saplingTreeTiles[i])
            {
                return i;
            }
        }

        return -1;
    }

    private bool IsSapling(TileBase tile)
    {
        return GetSaplingIndex(tile) != -1;
    }

    private bool IsAdultTree(TileBase tile)
    {
        if (tile == null || adultTreeTiles == null) return false;

        foreach (TileBase adultTreeTile in adultTreeTiles)
        {
            if (tile == adultTreeTile)
            {
                return true;
            }
        }

        return false;
    }

    private bool IsValidTreeIndex(int index)
    {
        if (!TreeListsAreValid()) return false;

        return index >= 0 && index < saplingTreeTiles.Length;
    }

    private bool TreeListsAreValid()
    {
        if (saplingTreeTiles == null || saplingTreeTiles.Length == 0)
        {
            return false;
        }

        if (adultTreeTiles == null || adultTreeTiles.Length == 0)
        {
            return false;
        }

        if (saplingTreeTiles.Length != adultTreeTiles.Length)
        {
            Debug.LogWarning("[FloraManager] SaplingTreeTiles y AdultTreeTiles deben tener el mismo tamaño.");
            return false;
        }

        for (int i = 0; i < saplingTreeTiles.Length; i++)
        {
            if (saplingTreeTiles[i] == null)
            {
                Debug.LogWarning("[FloraManager] Hay un sapling vacío en el índice " + i);
                return false;
            }

            if (adultTreeTiles[i] == null)
            {
                Debug.LogWarning("[FloraManager] Hay un árbol adulto vacío en el índice " + i);
                return false;
            }

            if (saplingTreeTiles[i] == adultTreeTiles[i])
            {
                Debug.LogWarning("[FloraManager] El sapling y el árbol adulto son el mismo tile en el índice " + i);
                return false;
            }
        }

        foreach (TileBase saplingTile in saplingTreeTiles)
        {
            foreach (TileBase adultTile in adultTreeTiles)
            {
                if (saplingTile == adultTile)
                {
                    Debug.LogWarning("[FloraManager] Un tile aparece tanto en saplings como en adultos. Revisa las listas.");
                    return false;
                }
            }
        }

        return true;
    }

    private int CountAdultTrees()
    {
        if (resourcesMap == null) return 0;

        int count = 0;
        BoundsInt bounds = resourcesMap.cellBounds;

        foreach (Vector3Int pos in bounds.allPositionsWithin)
        {
            TileBase tileAtPos = resourcesMap.GetTile(pos);

            if (IsAdultTree(tileAtPos))
            {
                count++;
            }
        }

        return count;
    }

    private void RebuildNavMesh()
    {
        if (navSurface != null)
        {
            navSurface.BuildNavMesh();
        }
    }
}