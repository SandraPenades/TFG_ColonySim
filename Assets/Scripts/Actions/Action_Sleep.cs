using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Action_Sleep : GoapAction
{
    private bool isDone = false;
    private GameObject targetBed;
    private Bed bedScript;
    private AgentMovement movement;
    private AgentNeeds needs;
    private Collider2D agentCollider;

    void Awake()
    {
        actionName = "Dormir";

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
        bedScript = null;

        foreach (GameObject bed in beds)
        {
            Bed tempBedScript = bed.GetComponent<Bed>();

            // Si la cama está libre
            if (tempBedScript != null && !tempBedScript.isOccupied)
            {
                float distance = Vector3.Distance(agent.transform.position, bed.transform.position);
                if (distance < shortestDistance)
                {
                    shortestDistance = distance;
                    targetBed = bed;
                    bedScript = tempBedScript;
                }
            }
        }

        return targetBed != null;
    }

    public override void Perform(GameObject agent)
    {
        // Reservar la cama
        if (bedScript != null) bedScript.isOccupied = true;

        movement = agent.GetComponent<AgentMovement>();
        agentCollider = agent.GetComponent<Collider2D>();

        movement.MoveTo(targetBed.transform.position);

        StartCoroutine(SleepRoutine(agent));
    }

    private IEnumerator SleepRoutine(GameObject agent)
    {
        while (!movement.HasReachedDestination()) yield return null;

        movement.StopMoving();

        if (agentCollider != null) agentCollider.enabled = false;
        agent.transform.position = targetBed.transform.position;

        // Debug.Log("Zzz.. Durmiendo...");
        
        float energyRegenPerSecond = 20f;

        while (needs.energy < 99.9f)
        {
            needs.energy += energyRegenPerSecond * Time.deltaTime;

            if (needs.energy > 100f) needs.energy = 100f;

            yield return null;
        }

        isDone = true;
    }

    public override bool IsDone() => isDone;
    public override void ResetAction() 
    { 
        StopAllCoroutines();
        
        if (bedScript != null)
        {
            bedScript.isOccupied = false;
            bedScript = null;
        }

        if (agentCollider != null) agentCollider.enabled = true;

        isDone = false; 
        targetBed = null; 
    }
}
