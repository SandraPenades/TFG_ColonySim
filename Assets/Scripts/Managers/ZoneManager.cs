using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class ZoneManager : MonoBehaviour
{
    public static ZoneManager Instance;

    [Header("Tilemaps de Zonas")]
    public Tilemap loggingZoneTilemap;
    public Tilemap miningZoneTilemap;
    public Tilemap harvestingZoneTilemap;

    [Header("Tilemap de Obstáculos")]
    public Tilemap obstaclesTilemap; // Para comprobar si hay árboles

    [Header("Tiles visuales de zonas")]
    public TileBase loggingZoneTile;
    public TileBase miningZoneTile;
    public TileBase harvestingZoneTile;

    // Añadir nueva variable de zona si hay un nuevo trabajo con zonas

    public enum ZoneType { None, Logging, Mining, Harvesting }

    private HashSet<Vector3Int> loggingZones = new HashSet<Vector3Int>();
    private HashSet<Vector3Int> miningZones = new HashSet<Vector3Int>();
    private HashSet<Vector3Int> harvestingZones = new HashSet<Vector3Int>();
    
    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    void Start()
    {
        HideAllZones();
    }

    public void MarkZone(BoundsInt area, ZoneType type)
    {
        Tilemap targetTilemap = GetTilemapForZone(type);
        TileBase tileToDraw = GetTileForZone(type);

        if (targetTilemap == null || tileToDraw == null) return;

        ShowOnlyZone(type);

        foreach (Vector3Int pos in area.allPositionsWithin)
        {
            targetTilemap.SetTile(pos, tileToDraw);
            RegisterZonePosition(pos, type);
            TryCreateJobAtPosition(pos, type);
        }

        Debug.Log($"Zona {type} aplicada.");
    }

    public void EraseZone(BoundsInt area, ZoneType type)
    {
        Tilemap targetTilemap = GetTilemapForZone(type);

        if (targetTilemap == null) return;

        ShowOnlyZone(type);

        foreach (Vector3Int pos in area.allPositionsWithin)
        {
            targetTilemap.SetTile(pos, null);
            UnregisterZonePosition(pos, type);
            RemoveJobsAtPosition(pos, type);
        }

        Debug.Log($"Zona {type} borrada.");
    }

    private void RegisterZonePosition(Vector3Int pos, ZoneType type)
    {
        switch (type)
        {
            case ZoneType.Logging:
                loggingZones.Add(pos);
                break;
            case ZoneType.Mining:
                miningZones.Add(pos);
                break;
            case ZoneType.Harvesting:
                harvestingZones.Add(pos);
                break;
        }
    }

    private void UnregisterZonePosition(Vector3Int pos, ZoneType type)
    {
        switch (type)
        {
            case ZoneType.Logging:
                loggingZones.Remove(pos);
                break;
            case ZoneType.Mining:
                miningZones.Remove(pos);
                break;
            case ZoneType.Harvesting:
                harvestingZones.Remove(pos);
                break;
        }
    }

    public Tilemap GetTilemapForZone(ZoneType zoneType)
    {
        switch (zoneType)
        {
            case ZoneType.Logging:
                return loggingZoneTilemap;
            case ZoneType.Mining:
                return miningZoneTilemap;
            case ZoneType.Harvesting:
                return harvestingZoneTilemap;
            default:
                return null;
        }
    }

    private TileBase GetTileForZone(ZoneType zoneType)
    {
        switch (zoneType)
        {
            case ZoneType.Logging:
                return loggingZoneTile;
            case ZoneType.Mining:
                return miningZoneTile;
            case ZoneType.Harvesting:
                return harvestingZoneTile;
            default:
                return null;
        }
    }

    public void ShowOnlyZone(ZoneType zoneType)
    {
        if (loggingZoneTilemap != null)
            loggingZoneTilemap.GetComponent<TilemapRenderer>().enabled = zoneType == ZoneType.Logging;
        if (miningZoneTilemap != null)
            miningZoneTilemap.GetComponent<TilemapRenderer>().enabled = zoneType == ZoneType.Mining;
        if (harvestingZoneTilemap != null)
            harvestingZoneTilemap.GetComponent<TilemapRenderer>().enabled = zoneType == ZoneType.Harvesting;
    }

    public void ShowAllZones()
    {
        if (loggingZoneTilemap != null)
            loggingZoneTilemap.GetComponent<TilemapRenderer>().enabled = true;
        if (miningZoneTilemap != null)
            miningZoneTilemap.GetComponent<TilemapRenderer>().enabled = true;
        if (harvestingZoneTilemap != null)
            harvestingZoneTilemap.GetComponent<TilemapRenderer>().enabled = true;
    }

    public void HideAllZones()
    {
        if (loggingZoneTilemap != null)
            loggingZoneTilemap.GetComponent<TilemapRenderer>().enabled = false;
        if (miningZoneTilemap != null)
            miningZoneTilemap.GetComponent<TilemapRenderer>().enabled = false;
        if (harvestingZoneTilemap != null)
            harvestingZoneTilemap.GetComponent<TilemapRenderer>().enabled = false;
    }

    private void TryCreateJobAtPosition(Vector3Int pos, ZoneType type)
    {
        if (obstaclesTilemap == null) return;

        Sprite tileSprite = obstaclesTilemap.GetSprite(pos);
        TileBase currentTile = obstaclesTilemap.GetTile(pos);

        if (tileSprite == null || currentTile == null) return;

        string spriteName = tileSprite.name.ToLower();

        if (type == ZoneType.Logging && spriteName.Contains("tree"))
        {
            JobManager.Instance.AddJob(Job.JobType.Talar, pos);
        }
        else if (type == ZoneType.Mining && spriteName.Contains("rock"))
        {
            JobManager.Instance.AddJob(Job.JobType.Minar, pos);
        }
        else if (type == ZoneType.Harvesting && currentTile == FloraManager.Instance.fullBushTile)
        {
            JobManager.Instance.AddJob(Job.JobType.Recolectar, pos);
        }
    }

    private void RemoveJobsAtPosition(Vector3Int pos, ZoneType type)
    {
        if (JobManager.Instance == null) return;

        Job.JobType jobType;

        switch (type)
        {
            case ZoneType.Logging:
                jobType = Job.JobType.Talar;
                break;
            case ZoneType.Mining:
                jobType = Job.JobType.Minar;
                break;
            case ZoneType.Harvesting:
                jobType = Job.JobType.Recolectar;
                break;
            default:
                return;
        }

        JobManager.Instance.RemovePendingJobsAtPosition(jobType, pos);
    }

    public void UpdateJobAtPosition(Vector3Int pos)
    {
        if (obstaclesTilemap == null) return;

        Sprite tileSprite = obstaclesTilemap.GetSprite(pos);
        TileBase currentTile = obstaclesTilemap.GetTile(pos);

        if (tileSprite == null || currentTile == null) return;

        string spriteName = tileSprite.name.ToLower();

        if (loggingZones.Contains(pos) && spriteName.Contains("tree"))
        {
            JobManager.Instance.AddJob(Job.JobType.Talar, pos);
        }

        if (miningZones.Contains(pos) && spriteName.Contains("rock"))
        {
            JobManager.Instance.AddJob(Job.JobType.Minar, pos);
        }

        if (harvestingZones.Contains(pos) && currentTile == FloraManager.Instance.fullBushTile)
        {
            JobManager.Instance.AddJob(Job.JobType.Recolectar, pos);
        }
    }
}
