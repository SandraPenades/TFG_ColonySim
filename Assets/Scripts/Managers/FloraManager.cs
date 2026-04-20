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

    // Ajustes
    public float regrowTime = 30f; // Esto hay que ajustarlo

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
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
            Debug.Log($"[FloraManager] Un arbusto ha crecido en {pos}");
        }
    }
}
