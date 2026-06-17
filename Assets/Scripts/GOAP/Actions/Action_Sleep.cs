using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Action_Sleep : GoapAction
{
    private bool isDone = false;
    private bool hasStarted = false;

    private GameObject targetBed;
    private Bed bedScript;

    private AgentMovement movement;
    private AgentNeeds needs;
    private ColonistAudio colonistAudio;

    private Collider2D agentCollider;
    private SpriteRenderer[] colonistRenderers;
    private Coroutine sleepCoroutine;

    protected override void Awake()
    {
        base.Awake();

        actionName = "Dormir";

        AddPrecondition("has_free_bed", true);
        AddEffect("is_rested", true);
        AddEffect("is_sleepy", false);

        needs = GetComponent<AgentNeeds>();
        colonistRenderers = GetComponentsInChildren<SpriteRenderer>();
        colonistAudio = GetComponent<ColonistAudio>();
    }

    public override bool CheckProceduralPrecondition(GameObject agent)
    {
        if (needs == null) return false;
        if (!needs.IsSleepy()) return false;

        GameObject[] beds = GameObject.FindGameObjectsWithTag("Bed");
        if (beds.Length == 0) return false;

        float shortestDistance = Mathf.Infinity;
        targetBed = null;
        bedScript = null;

        foreach (GameObject bed in beds)
        {
            Bed tempBedScript = bed.GetComponent<Bed>();

            if (tempBedScript != null && !tempBedScript.isOccupied && !tempBedScript.isReserved)
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

        return targetBed != null && bedScript != null;
    }

    public override void Perform(GameObject agent)
    {
        if (hasStarted) return;

        if (targetBed == null || bedScript == null || needs == null)
        {
            isDone = true;
            return;
        }

        hasStarted = true;
        isDone = false;

        movement = agent.GetComponent<AgentMovement>();
        agentCollider = agent.GetComponent<Collider2D>();

        bedScript.SetReserved(true);

        if (movement != null)
        {
            movement.MoveTo(targetBed.transform.position);
        }

        sleepCoroutine = StartCoroutine(SleepRoutine(agent));
    }

    private IEnumerator SleepRoutine(GameObject agent)
    {
        float timer = 0f;
        float maxMoveTime = 8f;

        while (movement != null && !movement.HasReachedDestination())
        {
            timer += Time.deltaTime;

            if (timer >= maxMoveTime)
            {
                ClearSleepState();
                isDone = true;
                yield break;
            }

            yield return null;
        }

        if (movement != null)
        {
            movement.StopMoving();
        }

        if (targetBed != null)
        {
            agent.transform.position = targetBed.transform.position;
        }

        if (agentCollider != null)
        {
            agentCollider.enabled = false;
        }

        SetColonistVisible(false);

        if (bedScript != null)
        {
            bedScript.SetOccupied(true);
        }

        if (GetComponent<AudioSource>() != null)
        {
            colonistAudio.PlaySleepLoop();
        }

        float energyRegenPerSecond = 20f;

        while (needs != null && needs.energy < 99.9f)
        {
            needs.energy += energyRegenPerSecond * Time.deltaTime;

            if (needs.energy > 100f)
            {
                needs.energy = 100f;
            }

            yield return null;
        }

        ClearSleepState();

        isDone = true;
    }

    private void ClearSleepState()
    {
        if (colonistAudio != null)
        {
            colonistAudio.StopLoop();
        }

        SetColonistVisible(true);

        if (agentCollider != null)
        {
            agentCollider.enabled = true;
        }

        if (bedScript != null)
        {
            bedScript.ClearBed();
        }
    }

    private void SetColonistVisible(bool visible)
    {
        if (colonistRenderers == null)
        {
            colonistRenderers = GetComponentsInChildren<SpriteRenderer>();
        }

        foreach (SpriteRenderer renderer in colonistRenderers)
        {
            if (renderer != null)
            {
                renderer.enabled = visible;
            }
        }
    }

    public override bool IsDone()
    {
        return isDone;
    }

    public override void ResetAction()
    {
        if (sleepCoroutine != null)
        {
            StopCoroutine(sleepCoroutine);
            sleepCoroutine = null;
        }

        ClearSleepState();

        hasStarted = false;
        isDone = false;
        targetBed = null;
        bedScript = null;
    }
}