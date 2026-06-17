using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using NavMeshPlus.Components;

public class Action_Build : GoapAction
{
    private Blueprint currentBlueprint;
    private AgentMovement movement;

    private StorageBuilding targetStorage;
    private List<GameObject> carriedVisuals = new List<GameObject>();

    [Header("Visualización de materiales")]
    public GameObject genericItemPrefab;
    public ItemDatabase database;
    
    private NavMeshSurface navSurface;

    private bool isDone = false;
    private bool hasStarted = false;
    private bool carryingResources = false;

    protected override void Awake()
    {
        base.Awake();

        actionName = "Construir";

        AddPrecondition("has_build_job", true);
        AddPrecondition("has_required_build_resources", true);

        AddEffect("blueprint_finished", true);

        navSurface = FindFirstObjectByType<NavMeshSurface>();
    }

    public override bool CheckProceduralPrecondition(GameObject agent)
    {
        if (ConstructionManager.Instance == null) return false;

        currentBlueprint = ConstructionManager.Instance.GetFirstPendingBlueprint();

        if (currentBlueprint != null && !currentBlueprint.isReserved && !currentBlueprint.isCompleted && (currentBlueprint.resourcesDelivered || HasRequiredResources(currentBlueprint)))
        {
            return true;
        }

        return false;
    }

    public override void Perform(GameObject agent)
    {
        if (hasStarted) return;
        if (currentBlueprint == null) return;

        hasStarted = true;
        currentBlueprint.isReserved = true;

        movement = agent.GetComponent<AgentMovement>();

        StartCoroutine(BuildRoutine());
    }

    private IEnumerator BuildRoutine()
    {
        if (currentBlueprint == null)
        {
            isDone = true;
            yield break;
        }

        if (!currentBlueprint.resourcesDelivered)
        {
            targetStorage = FindStorageWithRequiredResources(currentBlueprint);

            if (targetStorage == null)
            {
                currentBlueprint.isReserved = false;
                isDone = true;
                yield break;
            }

            yield return StartCoroutine(MoveToTarget(targetStorage.transform.position));

            if (isDone)
            {
                yield break;
            }

            if (!ConsumeRequiredResourcesFromStorage(currentBlueprint, targetStorage))
            {
                currentBlueprint.isReserved = false;
                isDone = true;
                yield break;
            }

            CreateCarriedResourceVisuals(currentBlueprint);
            carryingResources = true;

            yield return new WaitForSeconds(0.2f);

            yield return StartCoroutine(MoveToTarget(currentBlueprint.transform.position));

            if (isDone)
            {
                yield break;
            }

            ClearCarriedResourceVisuals();
            carryingResources = false;
            currentBlueprint.resourcesDelivered = true;

            SpriteRenderer sr = currentBlueprint.GetComponent<SpriteRenderer>();
            if (sr != null)
            {
                sr.color = new Color(0.4f, 1f, 0.75f, 0.65f);
            }
        }

        yield return StartCoroutine(MoveToTarget(currentBlueprint.transform.position));

        if (isDone)
        {
            yield break;
        }

        ColonistAudio audio = GetComponent<ColonistAudio>();

        if (audio != null)
        {
            audio.PlayBuildLoop();
        }

        yield return new WaitForSeconds(3.0f);

        if (audio != null)
        {
            audio.StopLoop();
        }

        if (currentBlueprint.finalPrefab != null)
        {
            GameObject finalBuilding = Instantiate(
                currentBlueprint.finalPrefab,
                currentBlueprint.transform.position,
                currentBlueprint.transform.rotation
            );

            Wall blueprintWall = currentBlueprint.GetComponentInChildren<Wall>();
            Wall finalWall = finalBuilding.GetComponent<Wall>();

            if (finalWall == null)
            {
                finalWall = finalBuilding.GetComponentInChildren<Wall>();
            }

            if (finalWall != null)
            {
                float manualRotation = 0f;

                if (blueprintWall != null)
                {
                    manualRotation = blueprintWall.ManualRotation;
                }

                finalWall.Initialize(currentBlueprint.cellPosition, manualRotation);

                if (WallManager.Instance != null)
                {
                    WallManager.Instance.RegisterWall(finalWall);
                }
            }

            ConstructedBuilding constructedBuilding = finalBuilding.GetComponent<ConstructedBuilding>();

            if (constructedBuilding != null)
            {
                constructedBuilding.originalResources.Clear();

                foreach (RequiredResource required in currentBlueprint.requiredResources)
                {
                    RequiredResource copy = new RequiredResource();
                    copy.itemID = required.itemID;
                    copy.amount = required.amount;

                    constructedBuilding.originalResources.Add(copy);
                }
            }
        }

        yield return null;

        if (navSurface != null)
        {
            navSurface.BuildNavMesh();
        }

        currentBlueprint.isCompleted = true;
        ConstructionManager.Instance.UnregisterBlueprint(currentBlueprint);

        if (BuilderManager.Instance != null)
        {
            BuilderManager.Instance.UnregisterBlueprintCells(currentBlueprint.cellPosition, currentBlueprint.occupiedSize);
        }

        Destroy(currentBlueprint.gameObject);

        currentBlueprint = null;
        isDone = true;
    }

    private IEnumerator MoveToTarget(Vector3 destination, float maxTime = 8f)
    {
        if (movement == null)
        {
            FailBuild();
            yield break;
        }

        movement.MoveTo(destination);

        float timer = 0f;

        while (!movement.HasReachedDestination())
        {
            timer += Time.deltaTime;

            if (timer >= maxTime)
            {
                FailBuild();
                yield break;
            }

            yield return null;
        }

        movement.StopMoving();
    }

    private void FailBuild()
    {
        if (currentBlueprint != null && !currentBlueprint.isCompleted)
        {
            currentBlueprint.isReserved = false;
        }

        if (carryingResources && targetStorage != null && currentBlueprint != null)
        {
            foreach (RequiredResource required in currentBlueprint.requiredResources)
            {
                targetStorage.AddItem(required.itemID, required.amount);
            }
        }

        ClearCarriedResourceVisuals();

        currentBlueprint = null;
        targetStorage = null;
        movement = null;
        carryingResources = false;
        hasStarted = false;
        isDone = true;
    }

    private StorageBuilding FindStorageWithRequiredResources(Blueprint blueprint)
    {
        GameObject[] storages = GameObject.FindGameObjectsWithTag("Storage");

        foreach (GameObject storageObj in storages)
        {
            StorageBuilding storage = storageObj.GetComponent<StorageBuilding>();

            if (storage == null) continue;

            bool hasAllResources = true;

            foreach (RequiredResource required in blueprint.requiredResources)
            {
                if (!storage.HasItemAmount(required.itemID, required.amount))
                {
                    hasAllResources = false;
                    break;
                }
            }

            if (hasAllResources)
            {
                return storage;
            }
        }

        return null;
    }

    private bool HasRequiredResources(Blueprint blueprint)
    {
        return FindStorageWithRequiredResources(blueprint) != null;
    }

    private int GetTotalStoredAmount(string itemID)
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

        return totalAmount;
    }

    private bool ConsumeRequiredResourcesFromStorage(Blueprint blueprint, StorageBuilding storage)
    {
        if (blueprint == null || storage == null) return false;

        foreach (RequiredResource required in blueprint.requiredResources)
        {
            if (!storage.HasItemAmount(required.itemID, required.amount))
            {
                return false;
            }
        }

        foreach (RequiredResource required in blueprint.requiredResources)
        {
            storage.TakeItem(required.itemID, required.amount);
        }

        return true;
    }

    private void CreateCarriedResourceVisuals(Blueprint blueprint)
    {
        ClearCarriedResourceVisuals();

        if (genericItemPrefab == null || database == null) return;

        float offset = 0f;

        foreach (RequiredResource required in blueprint.requiredResources)
        {
            GameObject visual = Instantiate(genericItemPrefab, transform.position, Quaternion.identity);

            SpriteRenderer sr = visual.GetComponent<SpriteRenderer>();

            if (sr != null)
            {
                sr.sprite = database.GetSprite(required.itemID);
                sr.sortingOrder = 10;
            }

            ResourceItem resourceItem = visual.GetComponent<ResourceItem>();

            if (resourceItem != null)
            {
                resourceItem.itemID = required.itemID;
                resourceItem.amount = required.amount;
            }

            Collider2D col = visual.GetComponent<Collider2D>();

            if (col != null)
            {
                col.enabled = false;
            }

            visual.transform.SetParent(transform);
            visual.transform.localPosition = new Vector3(offset, 0.6f, 0);

            carriedVisuals.Add(visual);

            offset += 0.25f;
        }
    }

    private void ClearCarriedResourceVisuals()
    {
        foreach (GameObject visual in carriedVisuals)
        {
            if (visual != null)
            {
                Destroy(visual);
            }
        }

        carriedVisuals.Clear();
    }

    public override bool IsDone()
    {
        return isDone;
    }

    public override void ResetAction()
    {
        StopAllCoroutines();

        if (currentBlueprint != null && !currentBlueprint.isCompleted)
        {
            currentBlueprint.isReserved = false;
        }

        if (carryingResources && targetStorage != null && currentBlueprint != null)
        {
            foreach (RequiredResource required in currentBlueprint.requiredResources)
            {
                targetStorage.AddItem(required.itemID, required.amount);
            }
        }

        ClearCarriedResourceVisuals();

        currentBlueprint = null;
        targetStorage = null;
        movement = null;
        carryingResources = false;
        isDone = false;
        hasStarted = false;
    }
}
