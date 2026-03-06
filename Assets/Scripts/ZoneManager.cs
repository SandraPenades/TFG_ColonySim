using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class ZoneManager : MonoBehaviour
{
    public Tilemap zoneTilemap;

    public TileBase loggingZoneTile;
    public TileBase miningZoneTile;

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
            }
        }

        Debug.Log($"Zona {type} aplicada. Total de casillas registradas: {gridZones.Count}");
    }
}
