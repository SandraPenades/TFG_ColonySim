using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ConstructionManager : MonoBehaviour
{
    public static ConstructionManager Instance;

    private List<Blueprint> pendingBlueprints = new List<Blueprint>();
    private List<ConstructedBuilding> pendingDeconstructions = new List<ConstructedBuilding>();

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    // CONSTRUCCIÓN
    public void RegisterBlueprint(Blueprint blueprint)
    {
        if (blueprint == null) return;

        if (!pendingBlueprints.Contains(blueprint))
        {
            pendingBlueprints.Add(blueprint);
        }
    }

    public void UnregisterBlueprint(Blueprint blueprint)
    {
        if (blueprint == null) return;

        pendingBlueprints.Remove(blueprint);
    }

    public bool HasPendingBlueprint()
    {
        foreach (Blueprint blueprint in pendingBlueprints)
        {
            if (blueprint != null && !blueprint.isReserved && !blueprint.isCompleted)
            {
                return true;
            }
        }

        return false;
    }
    
    public Blueprint GetFirstPendingBlueprint()
    {
        foreach (Blueprint blueprint in pendingBlueprints)
        {
            if (blueprint != null && !blueprint.isReserved && !blueprint.isCompleted)
            {
                return blueprint;
            }
        }

        return null;
    }

    // DECONSTRUCCIÓN
    public void MarkForDeconstruction(ConstructedBuilding building)
    {
        if (building == null) return;
        if (building.isMarkedForDeconstruction) return;

        building.SetMarkedForDeconstruction(true);
        building.isReservedForDeconstruction = false;

        if (!pendingDeconstructions.Contains(building))
        {
            pendingDeconstructions.Add(building);
        }
    }

    public void UnmarkForDeconstruction(ConstructedBuilding building)
    {
        if (building == null) return;
        if (building.isReservedForDeconstruction) return;

        building.SetMarkedForDeconstruction(false);
        building.isReservedForDeconstruction = false;

        pendingDeconstructions.Remove(building);
    }

    public bool HasPendingDeconstruction()
    {
        foreach (ConstructedBuilding building in pendingDeconstructions)
        {
            if (building == null) continue;
            if (!building.isMarkedForDeconstruction) continue;
            if (building.isReservedForDeconstruction) continue;

            return true;
        }

        return false;
    }

    public ConstructedBuilding GetFirstPendingDeconstruction()
    {
        foreach (ConstructedBuilding building in pendingDeconstructions)
        {
            if (building == null) continue;
            if (!building.isMarkedForDeconstruction) continue;
            if (building.isReservedForDeconstruction) continue;

            return building;
        }

        return null;
    }

    public void CompleteDeconstruction(ConstructedBuilding building)
    {
        if (building == null) return;

        pendingDeconstructions.Remove(building);
    }
}
