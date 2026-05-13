using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(AgentBrain))]
[RequireComponent(typeof(AgentMovement))]

public class ColonistRecruitment : MonoBehaviour
{
    public bool IsRecruited { get; private set; } = false;

    private AgentBrain brain;
    private AgentMovement movement;

    private float lastToggleTime = -999f;
    private const float toggleCooldown = 0.5f;
    private const float brainReactivationDelay = 0.25f;

    private Coroutine reactivateBrainCoroutine;

    private void Awake()
    {
        brain = GetComponent<AgentBrain>();
        movement = GetComponent<AgentMovement>();
    }

    public void ToggleRecruitment()
    {
        if (Time.time - lastToggleTime < toggleCooldown)
        {
            return;
        }

        lastToggleTime = Time.time;

        if (IsRecruited)
        {
            Unrecruit();
        }
        else
        {
            Recruit();
        }
    }

    public void Recruit()
    {
        IsRecruited = true;

        if (reactivateBrainCoroutine != null)
        {
            StopCoroutine(reactivateBrainCoroutine);
            reactivateBrainCoroutine = null;
        }

        if (brain != null)
        {
            brain.AbortCurrentAction();
            brain.enabled = false;
        }

        if (movement != null)
        {
            movement.StopMoving();
        }

        // Debug.Log($"[ColonistRecruitment] {gameObject.name} reclutado.");
    }

    public void Unrecruit()
    {
        IsRecruited = false;

        if (movement != null)
        {
            movement.StopMoving();
        }

        if (reactivateBrainCoroutine != null)
        {
            StopCoroutine(reactivateBrainCoroutine);
        }

        reactivateBrainCoroutine = StartCoroutine(ReactivateBrainAfterDelay());

        // Debug.Log($"[ColonistRecruitment] {gameObject.name} licenciado.");
    }

    private IEnumerator ReactivateBrainAfterDelay()
    {
        yield return new WaitForSeconds(brainReactivationDelay);

        if (!IsRecruited && brain != null)
        {
            brain.enabled = true;
        }

        reactivateBrainCoroutine = null;
    }
}
