using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using NavMeshPlus.Components;


public class Action_Deconstruct : GoapAction
{
    private ConstructedBuilding currentBuilding;
    private AgentMovement movement;
    private NavMeshSurface navSurface;

    private bool isDeconstructed = false;
    private bool hasStarted = false;

    protected override void Awake()
    {
        base.Awake();

        actionName = "Deconstruir";

        AddPrecondition("has_deconstruction_job", true);
        AddEffect("building_deconstructed", true);

        navSurface = FindFirstObjectByType<NavMeshSurface>();
    }

    public override bool CheckProceduralPrecondition(GameObject agent)
    {
        if (ConstructionManager.Instance == null) return false;

        currentBuilding = ConstructionManager.Instance.GetFirstPendingDeconstruction();

        return currentBuilding != null;
    }

    public override void Perform(GameObject agent)
    {
        if (hasStarted) return;
        if (currentBuilding == null) return;

        hasStarted = true;
        currentBuilding.isReservedForDeconstruction = true;

        movement = agent.GetComponent<AgentMovement>();
        movement.MoveTo(currentBuilding.transform.position);

        StartCoroutine(DeconstructRoutine());
    }

    private IEnumerator DeconstructRoutine()
    {
        while (!movement.HasReachedDestination())
        {
            yield return null;
        }

        movement.StopMoving();

        yield return new WaitForSeconds(2.0f);

        foreach (RequiredResource resource in currentBuilding.originalResources)
        {
            int amountToReturn = Mathf.CeilToInt(resource.amount / 2f);

            if (amountToReturn <= 0) continue;

            SpawnReturnedResource(resource.itemID, amountToReturn, currentBuilding.transform.position);
        }

        if (ConstructionManager.Instance != null)
        {
            ConstructionManager.Instance.CompleteDeconstruction(currentBuilding);
        }

        Destroy(currentBuilding.gameObject);

        yield return null;

        if (navSurface != null)
        {
            navSurface.BuildNavMesh();
        }

        currentBuilding = null;
        isDeconstructed = true;
    }

    private void SpawnReturnedResource(string itemID, int amount, Vector3 position)
    {
        GameObject prefabToSpawn = GetResourcePrefab(itemID);

        if (prefabToSpawn == null) return;

        GameObject droppedItem = Instantiate(prefabToSpawn, position, Quaternion.identity);

        ResourceItem resourceItem = droppedItem.GetComponent<ResourceItem>();

        if (resourceItem != null)
        {
            resourceItem.itemID = itemID;
            resourceItem.SetAmount(amount);
        }

        Vector3Int gridPos = new Vector3Int(Mathf.RoundToInt(position.x), Mathf.RoundToInt(position.y), 0);

        if (JobManager.Instance != null)
        {
            JobManager.Instance.AddJob(Job.JobType.Transportar, gridPos, itemID);
        }
    }

    private GameObject GetResourcePrefab(string itemID)
    {
        if (ResourceManager.Instance == null) return null;

        string id = itemID.ToLower();

        if(id.Contains("madera")) return ResourceManager.Instance.woodPrefab;
        if(id.Contains("piedra")) return ResourceManager.Instance.stonePrefab;
        // No hay construcciones que necesiten bayas pero por si acaso lo pongo
        if(id.Contains("baya")) return ResourceManager.Instance.berryPrefab; 

        return null;
    }

    public override bool IsDone()
    {
        return isDeconstructed;
    }

    public override void ResetAction()
    {
        StopAllCoroutines();

        if (currentBuilding != null)
        {
            currentBuilding.isReservedForDeconstruction = false;
        }

        currentBuilding = null;
        movement = null;
        isDeconstructed = false;
        hasStarted = false;
    }
}
