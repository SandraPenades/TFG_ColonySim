using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class FloraManager : MonoBehaviour
{
    public static FloraManager Instance;

    // Referencias
    public Tilemap obstaclesMap;
    public TileBase fullBushTile;
    public TileBase emptyBushTile;

    // Ajustes
    public float regrowTime = 30f; // Esto hay que ajustarlo

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    void Start()
    {
        // Al empezar, se escanea el mapa con la flora.
        BoundsInt bounds = obstaclesMap.cellBounds;

        foreach (Vector3Int pos in bounds.allPositionsWithin)
        {
            TileBase tileAtPos = obstaclesMap.GetTile(pos);

            // Si el tile es un arbusto vacío...
            if (tileAtPos != null && tileAtPos == emptyBushTile)
            {
                // Se inicia el ciclo de crecimiento.
                StartBushRegrowth(pos);
            }
        }
    }

    // Cuando un arbusto se queda sin bayas:
    public void StartBushRegrowth(Vector3Int cellPosition)
    {
        StartCoroutine(RegrowRoutine(cellPosition));
    }

    private IEnumerator RegrowRoutine(Vector3Int pos)
    {
        // Se espera el tiempo requerido
        yield return new WaitForSeconds(regrowTime);

        // Vuelve a aparecer el sprite con bayas
        if (obstaclesMap != null && fullBushTile != null)
        {
            obstaclesMap.SetTile(pos, fullBushTile);

            if (ZoneManager.Instance != null)
            {
                ZoneManager.Instance.UpdateJobAtPosition(pos);
            }

            // Debug.Log($"[FloraManager] Un arbusto ha crecido en {pos}");
        }
    }
}
