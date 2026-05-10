using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Action_Eat : GoapAction
{
    private bool isDone = false;
    private GameObject targetFood;
    private string foodToEat = "";
    private AgentMovement movement;
    private AgentNeeds needs;
    public GameObject genericItemPrefab;
    private GameObject targetFoodItem;

    void Awake()
    {
        actionName = "Comer";

        // Para el GOAP --> Le indicamos que el resultado de esto será quitar hambre
        AddEffect("is_fed", true);

        needs = GetComponent<AgentNeeds>();
    }

    public override bool CheckProceduralPrecondition(GameObject agent)
    {
        // Si no tiene sueño, no buscamos camas
        if (!needs.IsHungry()) return false;

        // Buscamos todas las cosas etiquetadas como "Bed" en el mapa
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
        movement = agent.GetComponent<AgentMovement>();
        movement.MoveTo(targetFood.transform.position);

        StartCoroutine(EatRoutine(agent));
    }

    private IEnumerator EatRoutine(GameObject agent)
    {
        while (!movement.HasReachedDestination()) yield return null;
        movement.StopMoving();

        StorageBuilding storage = targetFood.GetComponent<StorageBuilding>();

        if (storage != null && storage.TakeItem(foodToEat, 1))
        {
            targetFoodItem = Instantiate(genericItemPrefab, agent.transform.position, Quaternion.identity);

            Sprite foodSprite = storage.database.GetSprite(foodToEat);
            targetFoodItem.GetComponent<SpriteRenderer>().sprite = foodSprite;
            targetFoodItem.GetComponent<SpriteRenderer>().sortingOrder = 10; // Para que se vea por delante del colono

            ResourceItem rItem = targetFoodItem.GetComponent<ResourceItem>();
            if (rItem != null)
            {
                rItem.itemID = foodToEat;
                rItem.amount = 1;
            }

            Collider2D col = targetFoodItem.GetComponent<Collider2D>();
            if (col != null) col.enabled = false;

            targetFoodItem.transform.SetParent(agent.transform);
            targetFoodItem.transform.localPosition = new Vector3(0, 0.2f, 0);

            yield return new WaitForSeconds(0.2f);

            GameObject[] tables = new GameObject[0];

            try
            {
                tables = GameObject.FindGameObjectsWithTag("Table");
            }
            catch
            {
                Debug.Log("No se ha encontrado ninguna mesa.");
            }
            
            Vector3 eatPosition;

            if (tables.Length > 0)
            {
                float shortestDist = Mathf.Infinity;
                GameObject bestTable = tables[0];

                foreach (GameObject table in tables)
                {
                    float dist = Vector3.Distance(agent.transform.position, table .transform.position);
                    if (dist < shortestDist)
                    {
                        shortestDist = dist;
                        bestTable = table;
                    }
                }
                eatPosition = bestTable.transform.position;
            }
            else
            {
                // Si no hay mesas, que coma "en el suelo"
                eatPosition = agent.transform.position + new Vector3(1.5f, -0.5f, 0);
            }

            movement.MoveTo(eatPosition);
            while (!movement.HasReachedDestination()) yield return null;
            movement.StopMoving();

            // Debug.Log($"Ñam ñam.. Comiendo {foodToEat}...");
            yield return new WaitForSeconds(3.0f); // Esto se ajusta después

            if (targetFoodItem != null) Destroy(targetFoodItem);
            needs.hunger = 100f;
            isDone = true;
        }
        else
        {
            // Si había pero cuando llega ya no hay
            isDone = true;
        }
    }

    public override bool IsDone() => isDone;
    public override void ResetAction() 
    { 
        StopAllCoroutines();

        if (targetFoodItem != null && targetFoodItem.transform.parent != null)
        {
            targetFoodItem.transform.SetParent(null);

            Collider2D col = targetFoodItem.GetComponent<Collider2D>();
            if (col != null) col.enabled = true;

            Vector3 dropPos = targetFoodItem.transform.position;
            targetFoodItem.transform.position = new Vector3(dropPos.x, dropPos.y, 0);

            Vector3Int gridPos = new Vector3Int(Mathf.RoundToInt(dropPos.x), Mathf.RoundToInt(dropPos.y), 0);
            JobManager.Instance.AddJob(Job.JobType.Transportar, gridPos);
        }
        
        isDone = false; 
        targetFood = null; 
        foodToEat = "";
        targetFoodItem = null;
    }
}
