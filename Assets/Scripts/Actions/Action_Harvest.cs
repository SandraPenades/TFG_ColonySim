using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class Action_Harvest : GoapAction
{
    public bool isDone = false;
    private Job currentJob;
    private AgentMovement movement;
    private Grid mainGrid;
    private Tilemap obstaclesMap;

    void Awake()
    {
        actionName = "Recolectar Bayas";
        cost = 2f;

        // Efecto lógico: Cuando la acción termina, hay bayas disponibles
        AddEffect("has_berries", true);

        // Buscar el Grid
        mainGrid = FindFirstObjectByType<Grid>();

        GameObject obsObj = GameObject.Find("Obstaculos");
        if (obsObj != null) obstaclesMap = obsObj.GetComponent<Tilemap>();
    }

    // Precondición: Podemos recolectar bayas?
    public override bool CheckProceduralPrecondition(GameObject agent)
    {
        currentJob = null;
        Job closestJob = null;
        float shortestDistance = Mathf.Infinity;

        // Buscamos en la lista de trabajos en JobManager si hay orden de recolectar bayas
        foreach (Job job in JobManager.Instance.pendingJobs)
        {
            if (job.type == Job.JobType.Minar && job.state == Job.JobState.Pendiente)
            {
                // 1. Dónde está la piedra en el mundo real
                Vector3 jobWorldPos = mainGrid.GetCellCenterWorld(job.position);
                
                // 2. Medimos la distancia en línea recta desde el colono hasta la piedra
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

        // Esperar a llegar y minar
        StartCoroutine(MineRoutine());
    }

    // Corrutina que gestiona el tiempo
    private IEnumerator MineRoutine()
    {
        GameObject prefabToSpawn = ResourceManager.Instance.stonePrefab;

        while (!movement.HasReachedDestination())
        {
            yield return null;
        }

        Debug.Log("He llegado a la piedra. Empezando a minar...");

        yield return new WaitForSeconds(3.0f);

        // Borrar el árbol
        if (obstaclesMap != null)
        {
            obstaclesMap.SetTile(currentJob.position, null);
        }

        // Spawnear la piedra
        if (prefabToSpawn != null)
        {
            // Calcular el centro de la casilla de la roca
            Vector3 spawnPos = mainGrid.GetCellCenterWorld(currentJob.position);
            GameObject droppedStone = Instantiate(prefabToSpawn, spawnPos, Quaternion.identity);

            int randomAmount = Random.Range(1, 4);

            ResourceItem itemScript = droppedStone.GetComponent<ResourceItem>();
            if (itemScript != null)
            {
                itemScript.SetAmount(randomAmount);
            }
        }
        else
        {
            Debug.LogWarning("No asignado el prefab de piedra en el Inspector");
        }

        // Tachar el trabajo de la lista
        JobManager.Instance.pendingJobs.Remove(currentJob);

        Debug.Log("Piedra minada");

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
