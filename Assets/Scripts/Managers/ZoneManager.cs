using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class ZoneManager : MonoBehaviour
{
    public static ZoneManager Instance;

    public Tilemap zoneTilemap;
    public Tilemap obstaclesTilemap; // Para comprobar si hay árboles

    public TileBase loggingZoneTile;
    public TileBase miningZoneTile;
    public TileBase harvestingZoneTile;

    // Añadir nueva variable de zona si hay un nuevo trabajo con zonas

    public enum ZoneType { None, Logging, Mining, Harvesting }
    private Dictionary<Vector3Int, ZoneType> gridZones = new Dictionary<Vector3Int, ZoneType>();
    
    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    public void MarkZone(BoundsInt area, ZoneType type)
    {
        TileBase tileToDraw = null;
        if (type == ZoneType.Logging) tileToDraw = loggingZoneTile;
        else if (type == ZoneType.Mining) tileToDraw = miningZoneTile;
        else if (type == ZoneType.Harvesting) tileToDraw = harvestingZoneTile;

        foreach (Vector3Int pos in area.allPositionsWithin)
        {
            // 1. Dibujar visualmente en la pantalla
            zoneTilemap.SetTile(pos, tileToDraw);

            // 2. Guardar lógicamente para la IA
            if (type == ZoneType.None)
            {
                gridZones.Remove(pos);
            }
            else
            {
                gridZones[pos] = type;

                // Crear trabajo si hay un recurso
                Sprite tileSprite = obstaclesTilemap.GetSprite(pos);
                TileBase currentTile = obstaclesTilemap.GetTile(pos);

                if (tileSprite != null)
                {
                    string spriteName = tileSprite.name.ToLower();

                    if (type == ZoneType.Logging && (spriteName.Contains("tree")))
                    {
                        JobManager.Instance.AddJob(Job.JobType.Talar, pos);
                    }
                    else if (type == ZoneType.Mining && (spriteName.Contains("rock") || spriteName.Contains("ore")))
                    {
                        JobManager.Instance.AddJob(Job.JobType.Minar, pos);
                    }
                    else if (type == ZoneType.Harvesting && currentTile == FloraManager.Instance.fullBushTile) 
                    {
                        JobManager.Instance.AddJob(Job.JobType.Recolectar, pos);
                    }

                    // Añadir else if con nuevo tipo de trabajo usando las zonas si hace falta
                }
            }
        }

        Debug.Log($"Zona {type} aplicada. Total de casillas registradas: {gridZones.Count}");
    }

    public void UpdateJobAtPosition(Vector3Int pos)
    {
        // Se mira si la casilla tiene asignada una zona
        if (gridZones.ContainsKey(pos))
        {
            ZoneType type = gridZones[pos];
            Sprite tileSprite = obstaclesTilemap.GetSprite(pos);
            TileBase currentTile = obstaclesTilemap.GetTile(pos);

            if (tileSprite == null || currentTile == null) return;

            string spriteName = tileSprite.name.ToLower();

            // Si la zona es de recolección y el arbusto ha crecido
            if (type == ZoneType.Harvesting && currentTile == FloraManager.Instance.fullBushTile)
            {
                JobManager.Instance.AddJob(Job.JobType.Recolectar, pos);
            }
        }
    }
}
