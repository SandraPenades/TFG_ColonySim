using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ResourceManager : MonoBehaviour
{
    public static ResourceManager Instance;
    
    [Header("Prefabs de recursos")]
    public GameObject woodPrefab;
    public GameObject stonePrefab;
    public GameObject berryPrefab;

    private Dictionary<string, int> resources = new Dictionary<string, int>();

    public System.Action OnResourcesChanged;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);

        InitializeResources();
    }

    private void InitializeResources()
    {
        resources["Madera"] = 0;
        resources["Piedra"] = 0;
        resources["Baya"] = 0;
    }

    public void AddResource(string itemID, int amount)
    {
        if (string.IsNullOrEmpty(itemID)) return;
        if (amount <= 0) return;

        if (!resources.ContainsKey(itemID))
        {
            resources[itemID] = 0;
        }

        resources[itemID] += amount;

        OnResourcesChanged?.Invoke();
    }

    public bool RemoveResource(string itemID, int amount)
    {
        if (string.IsNullOrEmpty(itemID)) return false;
        if (amount <= 0) return false;

        if (!resources.ContainsKey(itemID)) return false;
        if (resources[itemID] < amount) return false;

        resources[itemID] -= amount;

        OnResourcesChanged?.Invoke();

        return true;
    }

    public int GetResourceAmount(string itemID)
    {
        if (string.IsNullOrEmpty(itemID)) return 0;

        if (!resources.ContainsKey(itemID))
        {
            return 0;
        }

        return resources[itemID];
    }
}