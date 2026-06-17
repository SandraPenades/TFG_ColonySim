using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(AgentBrain))]
[RequireComponent(typeof(AgentMovement))]

public class ColonistRecruitment : MonoBehaviour
{
    public bool IsRecruited { get; private set; } = false;
    public bool IsColonyMember { get; private set; } = true;

    public GameObject recruitedIcon;

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

    private void Start()
    {
        ApplyState();
    }

    public void SetAsVisitor()
    {
        IsColonyMember = false;
        IsRecruited = false;

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

        Door.RefreshAllDoorsFor(gameObject);
    }

    public void SetAsColonist()
    {
        if (!CanBeRecruited())
        {
            return;
        }
        
        IsColonyMember = true;
        IsRecruited = false;

        if (UIManager.Instance != null)
        {
            string colonistName = gameObject.name.Replace("Colonist_", "");

            UIManager.Instance.ShowColonistJoinedMessage(colonistName);
        }

        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlayVisitorJoin();
        }

        if (reactivateBrainCoroutine != null)
        {
            StopCoroutine(reactivateBrainCoroutine);
            reactivateBrainCoroutine = null;
        }

        if (brain != null)
        {
            brain.enabled = true;
        }

        Door.RefreshAllDoorsFor(gameObject);
    }

    private void ApplyState()
    {
        if (brain == null) return;

        if (!IsColonyMember)
        {
            brain.enabled = false;

            if (movement != null)
            {
                movement.StopMoving();
            }

            return;
        }

        brain.enabled = !IsRecruited;
    }

    public void ToggleRecruitment()
    {
        if (Time.time - lastToggleTime < toggleCooldown)
        {
            return;
        }

        lastToggleTime = Time.time;

        if (!CanBeRecruited())
        {
            return;
        }

        if (!IsColonyMember)
        {
            SetAsColonist();
            return;
        }

        if (IsRecruited)
        {
            Unrecruit();
        }
        else
        {
            Recruit();
        }
    }

    public bool CanBeRecruited()
    {
        ThiefVisitor thief = GetComponent<ThiefVisitor>();

        if (thief != null && thief.HasStolen)
        {
            return false;
        }

        return true;
    }

    public void Recruit()
    {
        if (!CanBeRecruited())
        {
            return;
        }

        if (!IsColonyMember)
        {
            SetAsColonist();
            return;
        }
        
        IsRecruited = true;
        recruitedIcon.SetActive(true);

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
        if (!IsColonyMember) return;

        IsRecruited = false;
        recruitedIcon.SetActive(false);

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

        if (IsColonyMember && !IsRecruited && brain != null)
        {
            brain.enabled = true;
        }

        reactivateBrainCoroutine = null;
    }
}
