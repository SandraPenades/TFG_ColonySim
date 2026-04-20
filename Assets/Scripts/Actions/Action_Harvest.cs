using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class Action_Harvest : GoapAction
{
    // Visuales
    public TileBase emptyBushTile;

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
            if (job.type == Job.JobType.Recolectar && job.state == Job.JobState.Pendiente)
            {
                Vector3 jobWorldPos = mainGrid.GetCellCenterWorld(job.position);
                float distance = Vector3.Distance(agent.transform.position, jobWorldPos);
                
                if (distance < shortestDistance)
                {
                    shortestDistance = distance; 
                    closestJob = job;
                }
            }
        }

        currentJob = closestJob;
        return currentJob != null;
    }

    public override void Perform(GameObject agent)
    {
        if (currentJob == null) return;

        currentJob.state = Job.JobState.EnProgreso;
        movement = agent.GetComponent<AgentMovement>();
        Vector3 worldPos = mainGrid.GetCellCenterWorld(currentJob.position);
        movement.MoveTo(worldPos);

        StartCoroutine(HarvestRoutine());
    }

    private IEnumerator HarvestRoutine()
    {
        GameObject prefabToSpawn = ResourceManager.Instance.berryPrefab;

        while (!movement.HasReachedDestination())
        {
            yield return null;
        }

        Debug.Log("He llegado al arbusto. Empezando a recolectar...");

        yield return new WaitForSeconds(1.5f);

        // Cambiar el sprite de con bayas a vacío
        if (obstaclesMap != null && emptyBushTile != null)
        {
            obstaclesMap.SetTile(currentJob.position, emptyBushTile);

            // Llama al FloraManager para que el arbusto vuelva a crecer
            FloraManager.Instance.StartBushRegrowth(currentJob.position);
        }

        // Spawnear las bayas
        if (prefabToSpawn != null)
        {
            Vector3 spawnPos = mainGrid.GetCellCenterWorld(currentJob.position);
            GameObject droppedBerries = Instantiate(prefabToSpawn, spawnPos, Quaternion.identity);

            int randomAmount = Random.Range(2, 5);

            ResourceItem itemScript = droppedBerries.GetComponent<ResourceItem>();
            if (itemScript != null)
            {
                itemScript.SetAmount(randomAmount);
            }
        }
        else
        {
            Debug.LogWarning("No asignado el prefab de bayas en el ResourceManager");
        }

        // Tachar el trabajo de la lista
        JobManager.Instance.pendingJobs.Remove(currentJob);
        Debug.Log("Bayas recolectadas");
        isDone = true;
    }

    public override bool IsDone()
    {
        return isDone;
    }
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
