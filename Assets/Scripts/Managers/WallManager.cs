using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class WallSpriteSet
{
    public WallMaterial material;

    public Sprite wallHorizontal;
    public Sprite wallVerticalLeft;
    public Sprite wallVerticalRight;

    public Sprite wallCornerTopLeft;
    public Sprite wallCornerTopRight;
    public Sprite wallCornerBottomLeft;
    public Sprite wallCornerBottomRight;
}

public class WallManager : MonoBehaviour
{
    public static WallManager Instance;

    [Header("Sprites por material")]
    [SerializeField] private List<WallSpriteSet> wallSpriteSets = new List<WallSpriteSet>();

    private Dictionary<Vector3Int, Wall> walls = new Dictionary<Vector3Int, Wall>();

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    public void RegisterWall(Wall wall)
    {
        if (wall == null) return;

        Vector3Int cell = wall.CellPosition;
        walls[cell] = wall;

        RefreshWallAndNeighbours(cell);
    }

    public void UnregisterWall(Wall wall)
    {
        if (wall == null) return;

        Vector3Int cell = wall.CellPosition;

        if (walls.ContainsKey(cell))
        {
            walls.Remove(cell);
        }

        RefreshWallAndNeighbours(cell);
    }

    public void RefreshWallAndNeighbours(Vector3Int cell)
    {
        UpdateWallSprite(cell);
        UpdateWallSprite(cell + Vector3Int.up);
        UpdateWallSprite(cell + Vector3Int.down);
        UpdateWallSprite(cell + Vector3Int.left);
        UpdateWallSprite(cell + Vector3Int.right);
    }

    private void UpdateWallSprite(Vector3Int cell)
    {
        if (!walls.TryGetValue(cell, out Wall wall)) return;

        bool up = HasWallAt(cell + Vector3Int.up);
        bool down = HasWallAt(cell + Vector3Int.down);
        bool right = HasWallAt(cell + Vector3Int.right);
        bool left = HasWallAt(cell + Vector3Int.left);

        Sprite selectedSprite = GetSpriteForConnections(
            up,
            down,
            left,
            right,
            wall.ManualRotation,
            wall.Material
        );

        wall.SetSprite(selectedSprite);
    }

    public Sprite GetPreviewSprite(Vector3Int cell, float manualRotation, WallMaterial material)
    {
        bool up = HasWallAt(cell + Vector3Int.up);
        bool down = HasWallAt(cell + Vector3Int.down);
        bool right = HasWallAt(cell + Vector3Int.right);
        bool left = HasWallAt(cell + Vector3Int.left);

        return GetSpriteForConnections(up, down, left, right, manualRotation, material);
    }

    private bool HasWallAt(Vector3Int cell)
    {
        if (walls.ContainsKey(cell))
        {
            return true;
        }

        if (BuilderManager.Instance != null && BuilderManager.Instance.HasWallBlueprintAt(cell))
        {
            return true;
        }

        return false;
    }

    private Sprite GetSpriteForConnections(
        bool up,
        bool down,
        bool left,
        bool right,
        float manualRotation,
        WallMaterial material
    )
    {
        WallSpriteSet set = GetSpriteSet(material);

        if (set == null)
        {
            Debug.LogWarning("[WallManager] No hay sprites configurados para el material: " + material);
            return null;
        }

        // Esquinas
        if (right && down && !left && !up)
        {
            return set.wallCornerTopLeft;
        }

        if (!right && down && left && !up)
        {
            return set.wallCornerTopRight;
        }

        if (right && !down && !left && up)
        {
            return set.wallCornerBottomLeft;
        }

        if (!right && !down && left && up)
        {
            return set.wallCornerBottomRight;
        }

        if (right && !down && left && up)
        {
            return set.wallCornerBottomRight;
        }

        if (right && down && left && !up)
        {
            return set.wallCornerTopRight;
        }

        // Horizontal
        if (right || left)
        {
            return set.wallHorizontal;
        }

        // Vertical
        if (down || up)
        {
            bool rightVariant = Mathf.RoundToInt(manualRotation) == 180;

            return rightVariant ? set.wallVerticalRight : set.wallVerticalLeft;
        }

        return set.wallHorizontal;
    }

    private WallSpriteSet GetSpriteSet(WallMaterial material)
    {
        foreach (WallSpriteSet set in wallSpriteSets)
        {
            if (set != null && set.material == material)
            {
                return set;
            }
        }

        return null;
    }
}