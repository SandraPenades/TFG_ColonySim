using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
using NavMeshPlus.Components;

public class Action_ChopTree : GoapAction
{
    public bool isDone = false;
    private Job currentJob;
    private AgentMovement movement;
    private Grid mainGrid;
    private Tilemap obstaclesMap;
    private NavMeshSurface navSurface;

    void Awake()
    {
        actionName = "Talar Árbol";
        cost = 2f;

        // Efecto lógico: Cuando la acción termina, hay madera disponible
        AddEffect("has_wood", true);

        // Buscar el Grid
        mainGrid = FindFirstObjectByType<Grid>();

        GameObject obsObj = GameObject.Find("Obstaculos");
        if (obsObj != null) obstaclesMap = obsObj.GetComponent<Tilemap>();

        navSurface = FindFirstObjectByType<NavMeshSurface>();
    }

    // Precondición: Podemos talar?
    public override bool CheckProceduralPrecondition(GameObject agent)
    {
        currentJob = null;
        Job closestJob = null;
        float shortestDistance = Mathf.Infinity;

        // Buscamos en la lista de trabajos en JobManager si hay orden de tala
        foreach (Job job in JobManager.Instance.pendingJobs)
        {
            if (job.type == Job.JobType.Talar && job.state == Job.JobState.Pendiente)
            {
                // 1. Dónde está el árbol en el mundo real
                Vector3 jobWorldPos = mainGrid.GetCellCenterWorld(job.position);
                
                // 2. Medimos la distancia en línea recta desde Pepe hasta el árbol
                float distance = Vector3.Distance(agent.transform.position, jobWorldPos);

                // 3. Si esta distancia es más pequeña que la que teníamos guardada...
                if (distance < shortestDistance)
                {
                    shortestDistance = distance; // Actualizamos el récord de cercanía
                    closestJob = job;            // Este es nuestro nuevo candidato favorito
                }
            }
        }

        currentJob = closestJob;
        return currentJob != null;
    }

    // Ejecución: El GOAP decide que hagamos esto
    public override void Perform(GameObject agent)
    {
        if (currentJob == null) return;

        // Marcar como en progreso para que no hayan 2 colonos con el mismo trabajo
        currentJob.state = Job.JobState.EnProgreso;

        movement = agent.GetComponent<AgentMovement>();

        // Convertir las coordenadas a la posición del mundo real
        Vector3 worldPos = mainGrid.GetCellCenterWorld(currentJob.position);

        // Decirle al colono que vaya
        movement.MoveTo(worldPos);

        // Esperar a llegar y talar
        StartCoroutine(ChopRoutine());
    }

    // Corrutina que gestiona el tiempo
    private IEnumerator ChopRoutine()
    {
        GameObject prefabToSpawn = ResourceManager.Instance.woodPrefab;

        while (!movement.HasReachedDestination())
        {
            yield return null;
        }

        Debug.Log("He llegado al árbol. Empezando a talar...");

        yield return new WaitForSeconds(3.0f);

        // Borrar el árbol
        if (obstaclesMap != null)
        {
            obstaclesMap.SetTile(currentJob.position, null);
        }

        yield return null;

        // Recalcular el mapa del NavMesh
        if (navSurface != null)
        {
            navSurface.BuildNavMesh();
        }

        // Spawnear la madera
        if (prefabToSpawn != null)
        {
            // Calcular el centro de la casilla del árbol
            Vector3 spawnPos = mainGrid.GetCellCenterWorld(currentJob.position);
            GameObject droppedWood = Instantiate(prefabToSpawn, spawnPos, Quaternion.identity);

            int randomAmount = Random.Range(1, 4);

            ResourceItem itemScript = droppedWood.GetComponent<ResourceItem>();
            if (itemScript != null)
            {
                itemScript.SetAmount(randomAmount);
            }
        }
        else
        {
            Debug.LogWarning("No asignado el prefab de madera en el Inspector");
        }

        // Tachar el trabajo de la lista
        JobManager.Instance.pendingJobs.Remove(currentJob);

        Debug.Log("Árbol talado");

        // Indicar al GOAP que se ha cumplido
        isDone = true;
    }

    // Estado: El GOAP pregunta si ya está acabado
    public override bool IsDone()
    {
        return isDone;
    }

    // Limpieza: Resetear los valores para la próxima acción
    public override void ResetAction()
    {
        isDone = false;
        currentJob = null;
    }
}
