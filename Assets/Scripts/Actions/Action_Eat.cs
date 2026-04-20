using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Action_Eat : GoapAction
{
    private bool isDone = false;
    private GameObject targetFood;
    private AgentMovement movement;
    private AgentNeeds needs;

    void Awake()
    {
        actionName = "Comer";
        cost = 1f; // Para que tenga prioridad

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

        foreach (GameObject foodObj in foodSources)
        {
            // Miramos si tiene item "comida"
            StorageBuilding storage = foodObj.GetComponent<StorageBuilding>();

            if (storage != null && storage.HasItem("comida"))
            {
                float distance = Vector3.Distance(agent.transform.position, foodObj.transform.position);
                if (distance < shortestDistance)
                {
                    shortestDistance = distance;
                    targetFood = foodObj;
                }
            }
        }

        return targetFood != null;
    }

    public override void Perform(GameObject agent)
    {
        movement = agent.GetComponent<AgentMovement>();
        movement.MoveTo(targetFood.transform.position);
        StartCoroutine(EatRoutine());
    }

    private IEnumerator EatRoutine()
    {
        while (!movement.HasReachedDestination()) yield return null;

        StorageBuilding storage = targetFood.GetComponent<StorageBuilding>();

        if (storage != null && storage.TakeItem("comida", 1))
        {
            Debug.Log("Ñam ñam.. Comiendo...");
            yield return new WaitForSeconds(3.0f); // Esto se ajusta después

            needs.Eat(100f);
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
        
        isDone = false; 
        targetFood = null; 
    }
}
