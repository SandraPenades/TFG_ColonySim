using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Action_Eat : GoapAction
{
    private bool isDone = false;
    private bool hasStarted = false;
    private GameObject targetFood;
    private string foodToEat = "";
    private AgentMovement movement;
    private AgentNeeds needs;

    [Header("Visualización del recurso")]
    public GameObject genericItemPrefab;
    
    private GameObject targetFoodItem;

    protected override void Awake()
    {
        base.Awake();
        
        actionName = "Comer";

        // Para el GOAP
        AddPrecondition("has_food_available", true);
        AddEffect("is_fed", true);
        AddEffect("is_hungry", false);

        needs = GetComponent<AgentNeeds>();
    }

    public override bool CheckProceduralPrecondition(GameObject agent)
    {
        // Si no tiene hambre, no buscamos comida
        if (!needs.IsHungry()) return false;

        // Buscamos todos los almacenes del mapa
        GameObject[] foodSources = GameObject.FindGameObjectsWithTag("Storage");
        if (foodSources.Length == 0) return false;

        // Localizamos la más cercana
        float shortestDistance = Mathf.Infinity;
        targetFood = null;
        foodToEat = "";

        foreach (GameObject foodObj in foodSources)
        {
            // Miramos si tiene item comestible
            StorageBuilding storage = foodObj.GetComponent<StorageBuilding>();

            if (storage != null && storage.HasEdibleFood(out string foundFood))
            {
                float distance = Vector3.Distance(agent.transform.position, foodObj.transform.position);
                if (distance < shortestDistance)
                {
                    shortestDistance = distance;
                    targetFood = foodObj;
                    foodToEat = foundFood;
                }
            }
        }

        return targetFood != null;
    }

    public override void Perform(GameObject agent)
    {
        if (hasStarted) return;

        movement = agent.GetComponent<AgentMovement>();

        if (movement == null || targetFood == null)
        {
            isDone = true;
            return;
        }

        hasStarted = true;
        isDone = false;

        movement.MoveTo(targetFood.transform.position);

        StartCoroutine(EatRoutine(agent));
    }

    private IEnumerator EatRoutine(GameObject agent)
    {
        float moveTimer = 0f;
        float maxMoveTime = 8f;

        while (!movement.HasReachedDestination())
        {
            moveTimer += Time.deltaTime;

            if (moveTimer >= maxMoveTime)
            {
                isDone = true;
                yield break;
            }

            yield return null;
        }

        movement.StopMoving();

        StorageBuilding storage = targetFood.GetComponent<StorageBuilding>();

        if (storage != null && storage.TakeItem(foodToEat, 1))
        {
            targetFoodItem = Instantiate(genericItemPrefab, agent.transform.position, Quaternion.identity);

            Sprite foodSprite = storage.database.GetSprite(foodToEat);

            SpriteRenderer sr = targetFoodItem.GetComponent<SpriteRenderer>();
            if (sr != null)
            {
                sr.sprite = foodSprite;
                sr.sortingOrder = 10;
            }

            ResourceItem rItem = targetFoodItem.GetComponent<ResourceItem>();
            if (rItem != null)
            {
                rItem.itemID = foodToEat;
                rItem.amount = 1;
            }

            Collider2D col = targetFoodItem.GetComponent<Collider2D>();
            if (col != null)
            {
                col.enabled = false;
            }

            targetFoodItem.transform.SetParent(agent.transform);
            targetFoodItem.transform.localPosition = new Vector3(0, 0.2f, 0);

            ColonistAudio audio = GetComponent<ColonistAudio>();

            if (audio != null)
            {
                audio.PlayEat();
            }

            yield return new WaitForSeconds(3.0f);

            if (targetFoodItem != null)
            {
                Destroy(targetFoodItem);
                targetFoodItem = null;
            }

            needs.hunger = 100f;
            isDone = true;
        }
        else
        {
            // Si cuando llega ya no queda comida, termina para que pueda replantear.
            isDone = true;
        }
    }

    public override bool IsDone() => isDone;
    public override void ResetAction()
    {
        StopAllCoroutines();

        if (targetFoodItem != null)
        {
            ResourceItem rItem = targetFoodItem.GetComponent<ResourceItem>();

            StorageBuilding storage = null;

            if (targetFood != null)
            {
                storage = targetFood.GetComponent<StorageBuilding>();
            }

            if (storage == null)
            {
                storage = FindObjectOfType<StorageBuilding>();
            }

            if (storage != null && rItem != null)
            {
                storage.AddItem(rItem.itemID, rItem.amount);
            }

            Destroy(targetFoodItem);
            targetFoodItem = null;
        }

        isDone = false;
        hasStarted = false;
        targetFood = null;
        foodToEat = "";
    }
}
