using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class ZoneManager : MonoBehaviour
{
    public Tilemap zoneTilemap;
    public Tilemap obstaclesTilemap; // Para comprobar si hay árboles

    public TileBase loggingZoneTile;
    public TileBase miningZoneTile;

    // Añadir nueva variable de zona si hay un nuevo trabajo con zonas

    public enum ZoneType { None, Logging, Mining }
    private Dictionary<Vector3Int, ZoneType> gridZones = new Dictionary<Vector3Int, ZoneType>();
    public void MarkZone(BoundsInt area, ZoneType type)
    {
        TileBase tileToDraw = null;
        if (type == ZoneType.Logging) tileToDraw = loggingZoneTile;
        else if (type == ZoneType.Mining) tileToDraw = miningZoneTile;

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

                    // Añadir else if con nuevo tipo de trabajo usando las zonas si hace falta
                }
            }
        }

        Debug.Log($"Zona {type} aplicada. Total de casillas registradas: {gridZones.Count}");
    }
}
