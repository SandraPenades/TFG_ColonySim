using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
using NavMeshPlus.Components;

public class Action_Mining : GoapAction
{
    public bool isDone = false;
    private Job currentJob;
    private AgentMovement movement;
    private Grid mainGrid;
    private Tilemap obstaclesMap;
    private NavMeshSurface navSurface;

    void Awake()
    {
        actionName = "Minar Roca";

        // Efecto lógico: Cuando la acción termina, hay piedra disponible
        AddEffect("has_stone", true);

        // Buscar el Grid
        mainGrid = FindFirstObjectByType<Grid>();

        GameObject obsObj = GameObject.Find("Obstaculos");
        if (obsObj != null) obstaclesMap = obsObj.GetComponent<Tilemap>();
    }

    // Precondición: Podemos minar?
    public override bool CheckProceduralPrecondition(GameObject agent)
    {
        // Que el JobManager indique la piedra más cercana
        currentJob = JobManager.Instance.GetNextJob(Job.JobType.Minar, agent.transform.position);

        // Si da algo, es true y si no, es false
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

        // Borrar la piedra
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

        // Spawnear la piedra
        if (prefabToSpawn != null)
        {
            // Calcular el centro de la casilla de la roca
            Vector3Int itemPos = currentJob.position;
            Vector3 spawnPos = mainGrid.GetCellCenterWorld(itemPos);
            GameObject droppedStone = Instantiate(prefabToSpawn, spawnPos, Quaternion.identity);
            JobManager.Instance.AddJob(Job.JobType.Transportar, itemPos);

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
        StopAllCoroutines();
        
        if (currentJob != null && currentJob.state == Job.JobState.EnProgreso)
        {
            currentJob.state = Job.JobState.Pendiente;
        }

        isDone = false; 
        currentJob = null; 
    }
}
