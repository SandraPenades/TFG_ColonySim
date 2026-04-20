using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Action_Sleep : GoapAction
{
    private bool isDone = false;
    private GameObject targetBed;
    private AgentMovement movement;
    private AgentNeeds needs;

    void Awake()
    {
        actionName = "Dormir";
        cost = 1f; // Para que tenga prioridad

        // Para el GOAP --> Le indicamos que el resultado de esto será quitar sueño
        AddEffect("is_rested", true);

        needs = GetComponent<AgentNeeds>();
    }

    public override bool CheckProceduralPrecondition(GameObject agent)
    {
        // Si no tiene sueño, no buscamos camas
        if (!needs.IsSleepy()) return false;

        // Buscamos todas las cosas etiquetadas como "Bed" en el mapa
        GameObject[] beds = GameObject.FindGameObjectsWithTag("Bed");
        if (beds.Length == 0) return false;

        // Localizamos la más cercana
        float shortestDistance = Mathf.Infinity;
        targetBed = null;

        foreach (GameObject bed in beds)
        {
            float distance = Vector3.Distance(agent.transform.position, bed.transform.position);
            if (distance < shortestDistance)
            {
                shortestDistance = distance;
                targetBed = bed;
            }
        }

        return targetBed != null;
    }

    public override void Perform(GameObject agent)
    {
        movement = agent.GetComponent<AgentMovement>();
        movement.MoveTo(targetBed.transform.position);
        StartCoroutine(SleepRoutine());
    }

    private IEnumerator SleepRoutine()
    {
        while (!movement.HasReachedDestination()) yield return null;

        Debug.Log("Zzz.. Durmiendo...");
        yield return new WaitForSeconds(5.0f); // Esto se ajusta después

        needs.Sleep(100f);
        isDone = true;
    }

    public override bool IsDone() => isDone;
    public override void ResetAction() 
    { 
        StopAllCoroutines();
        
        isDone = false; 
        targetBed = null; 
    }
}
