using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Tilemaps;
using System.Collections;
using NavMeshPlus.Components;

public class MovementController : MonoBehaviour
{
    // Configuramos el NavMeshAgent
    private NavMeshAgent agent;

    // Variable para comprobar si está reclutado
    public bool isRecruited = false;

    void Start()
    {
        // Obtenemos la referencia del agente
        agent = GetComponent<NavMeshAgent>();

        // Para que no gire
        agent.updateRotation = false;
        agent.updateUpAxis = false;

    }

    public void MoveToPosition(Vector3 dest)
    {
        StopAllCoroutines();
        agent.SetDestination(dest);
    }

    public void GoAndChop(Vector3Int tilePos, Tilemap map)
    {
        StartCoroutine(ChopRoutine(tilePos, map));
    }

    IEnumerator ChopRoutine(Vector3Int targetCell, Tilemap map)
    {
        // Calcular la posición del árbol
        Vector3 worldPos = map.GetCellCenterWorld(targetCell);

        // Moverse hacia allí
        agent.SetDestination(worldPos);
        Debug.Log("Yendo a talar");

        // Comprobamos si ha llegado dejando un margen para que no esté del todo pegado
        while (agent.pathPending || agent.remainingDistance > 1.2f)
        {
            yield return null;
        }

        // Cuando llega, empieza el trabajo
        Debug.Log("Talando");
        yield return new WaitForSeconds(3.0f); // Lo que tarda en talar

        // Borramos el árbol talado
        map.SetTile(targetCell, null);

        Debug.Log("Árbol talado");

        yield return new WaitForFixedUpdate();

        var surface = FindFirstObjectByType<NavMeshSurface>();

        if (surface != null)
        {
            surface.BuildNavMeshAsync();
        }
    }
}
