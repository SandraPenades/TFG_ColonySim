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

    private void Awake()
    {
        brain = GetComponent<AgentBrain>();
        movement = GetComponent<AgentMovement>();
    }

    public void ToggleRecruitment()
    {
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

        if (brain != null)
        {
            brain.AbortCurrentAction();
            brain.enabled = false;
        }

        if (movement != null)
        {
            movement.StopMoving();
        }

        Debug.Log($"[ColonistRecruitment] {gameObject.name} reclutado.");
    }

    public void Unrecruit()
    {
        IsRecruited = false;

        if (brain != null)
        {
            brain.enabled = true;
        }

        Debug.Log($"[ColonistRecruitment] {gameObject.name} licenciado.");
    }
}
