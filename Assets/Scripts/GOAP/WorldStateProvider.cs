using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WorldStateProvider : MonoBehaviour
{
    public ItemDatabase itemDatabase;

    public WorldState BuildWorldState(GameObject agent)
    {
        WorldState state = new WorldState();

        AgentNeeds needs = agent.GetComponent<AgentNeeds>();

        // Estados internos del agente
        state.SetState("is_hungry", needs != null && needs.IsHungry());
        state.SetState("is_sleepy", needs != null && needs.IsSleepy());
        state.SetState("is_bored", needs != null && needs.IsBored());
        state.SetState("is_lonely", needs != null && needs.IsLonely());
        state.SetState("is_dying", needs != null && needs.IsDying());

        // Estados del entorno
        state.SetState("has_food_available", CheckFoodAvailable());
        state.SetState("has_free_bed", CheckFreeBedAvailable());

        // Trabajos pendientes
        state.SetState("has_tree_job", JobManager.Instance != null && JobManager.Instance.HasPendingJob(Job.JobType.Talar));
        state.SetState("has_mining_job", JobManager.Instance != null && JobManager.Instance.HasPendingJob(Job.JobType.Minar));
        state.SetState("has_haul_job", JobManager.Instance != null && JobManager.Instance.HasPendingJob(Job.JobType.Transportar));
        state.SetState("has_harvest_job", JobManager.Instance != null && JobManager.Instance.HasPendingJob(Job.JobType.Recolectar));
        state.SetState("has_build_job", ConstructionManager.Instance != null && ConstructionManager.Instance.HasPendingBlueprint());
        state.SetState("has_deconstruction_job", ConstructionManager.Instance != null && ConstructionManager.Instance.HasPendingDeconstruction());

        // Recursos y almacenamiento
        state.SetState("has_loose_resource", CheckLooseResourceAvailable());
        state.SetState("has_loose_food", CheckLooseFoodAvailable());
        state.SetState("has_storage_available", CheckStorageAvailable());
        state.SetState("has_required_build_resources", CheckRequiredBuildResourcesAvailable());

        // Recursos necesarios para construir
        state.SetState("missing_wood_for_build", CheckMissingResourceForBuild("Madera"));
        state.SetState("missing_stone_for_build", CheckMissingResourceForBuild("Piedra"));

        return state;
    }

    private bool CheckFoodAvailable()
    {
        GameObject[] storages = GameObject.FindGameObjectsWithTag("Storage");

        foreach (GameObject storageObj in storages)
        {
            StorageBuilding storage = storageObj.GetComponent<StorageBuilding>();

            if (storage != null && storage.HasEdibleFood(out string foundFood))
            {
                return true;
            }
        }

        return false;
    }

    private bool CheckFreeBedAvailable()
    {
        GameObject[] beds = GameObject.FindGameObjectsWithTag("Bed");

        foreach (GameObject bedObj in beds)
        {
            Bed bed = bedObj.GetComponent<Bed>();

            if (bed != null && !bed.isOccupied)
            {
                return true;
            }
        }

        return false;
    }

    private bool CheckLooseResourceAvailable()
    {
        ResourceItem[] looseResources = FindObjectsByType<ResourceItem>(FindObjectsSortMode.None);

        foreach (ResourceItem item in looseResources)
        {
            if (item == null) continue;
            if (item.transform.parent != null) continue;

            return true;
        }

        return false;
    }

    private bool CheckLooseFoodAvailable()
    {
        ResourceItem[] looseResources = FindObjectsByType<ResourceItem>(FindObjectsSortMode.None);

        foreach (ResourceItem item in looseResources)
        {
            if (item == null) continue;

            // Si tiene padre, probablemente lo lleva un colono o está asociado a otro objeto.
            if (item.transform.parent != null) continue;

            if (itemDatabase != null && itemDatabase.IsComestible(item.itemID))
            {
                return true;
            }
        }

        return false;
    }

    private bool CheckStorageAvailable()
    {
        GameObject[] storages = GameObject.FindGameObjectsWithTag("Storage");

        foreach (GameObject storageObj in storages)
        {
            StorageBuilding storage = storageObj.GetComponent<StorageBuilding>();

            if (storage != null)
            {
                return true;
            }
        }

        return false;
    }

    private bool CheckStoredItemAvailable(string itemID, int amount)
    {
        GameObject[] storages = GameObject.FindGameObjectsWithTag("Storage");

        foreach (GameObject storageObj in storages)
        {
            StorageBuilding storage = storageObj.GetComponent<StorageBuilding>();

            if (storage != null && storage.HasItemAmount(itemID, amount))
            {
                return true;
            }
        }

        return false;
    }

    private bool CheckRequiredBuildResourcesAvailable()
    {
        if (ConstructionManager.Instance == null) return false;

        Blueprint blueprint = ConstructionManager.Instance.GetFirstPendingBlueprint();

        if (blueprint == null) return false;

        if (blueprint.resourcesDelivered) return true;

        foreach (RequiredResource required in blueprint.requiredResources)
        {
            if (!CheckStoredItemAmount(required.itemID, required.amount))
            {
                return false;
            }
        }

        return true;
    }

    private bool CheckStoredItemAmount(string itemID, int requiredAmount)
    {
        GameObject[] storages = GameObject.FindGameObjectsWithTag("Storage");

        int totalAmount = 0;

        foreach (GameObject storageObj in storages)
        {
            StorageBuilding storage = storageObj.GetComponent<StorageBuilding>();

            if (storage != null)
            {
                totalAmount += storage.GetItemAmount(itemID);
            }
        }

        return totalAmount >= requiredAmount;
    }

    private bool CheckMissingResourceForBuild(string itemID)
    {
        if (ConstructionManager.Instance == null) return false;

        Blueprint blueprint = ConstructionManager.Instance.GetFirstPendingBlueprint();

        if (blueprint == null) return false;
        if (blueprint.resourcesDelivered) return false;

        foreach (RequiredResource required in blueprint.requiredResources)
        {
            if (required.itemID != itemID) continue;

            if (!CheckStoredItemAmount(required.itemID, required.amount))
            {
                return true;
            }
        }

        return false;
    }
}
