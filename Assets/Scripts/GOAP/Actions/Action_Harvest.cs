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
    private Tilemap resourcesMap;

    protected override void Awake()
    {
        base.Awake();
        
        actionName = "Recolectar Bayas";

        // Para el GOAP
        AddPrecondition("has_harvest_job", true);
        AddEffect("has_loose_resource", true); // Para la acción genérica de transportar
        AddEffect("has_loose_food", true); // Si hace falta comida específicamente

        // Buscar el Grid
        mainGrid = FindFirstObjectByType<Grid>();

        GameObject resObj = GameObject.Find("RecursosNaturales");
        if (resObj != null) resourcesMap = resObj.GetComponent<Tilemap>();
    }

    // Precondición: Podemos recolectar bayas?
    public override bool CheckProceduralPrecondition(GameObject agent)
    {
        // Que el JobManager indique el arbusto lleno más cercano
        currentJob = JobManager.Instance.GetNextJob(Job.JobType.Recolectar, agent.transform.position);

        // Si da algo, es true y si no, es false
        return currentJob != null;
    }

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

        // Esperar a llegar y recolectar
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
        if (resourcesMap != null && emptyBushTile != null)
        {
            resourcesMap.SetTile(currentJob.position, emptyBushTile);

            // Llama al FloraManager para que el arbusto vuelva a crecer
            FloraManager.Instance.StartBushRegrowth(currentJob.position);
        }

        // Spawnear las bayas
        if (prefabToSpawn != null)
        {
            Vector3Int itemPos = currentJob.position;
            Vector3 spawnPos = mainGrid.GetCellCenterWorld(itemPos);
            GameObject droppedBerries = Instantiate(prefabToSpawn, spawnPos, Quaternion.identity);

            int randomAmount = Random.Range(2, 5);

            ResourceItem itemScript = droppedBerries.GetComponent<ResourceItem>();
            if (itemScript != null)
            {
                itemScript.SetAmount(randomAmount);
                JobManager.Instance.AddJob(Job.JobType.Transportar, itemPos, itemScript.itemID);
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
