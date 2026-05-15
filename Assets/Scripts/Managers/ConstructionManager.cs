using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ConstructionManager : MonoBehaviour
{
    public static ConstructionManager Instance;

    private List<Blueprint> pendingBlueprints = new List<Blueprint>();

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

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
}
