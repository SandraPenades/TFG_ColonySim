using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Action_HaveFun : GoapAction
{
    [Header("Configuración de diversión")]
    public float funRecovered = 100f;
    public float duration = 10f;

    private AgentNeeds needs;
    private AgentMovement movement;
    private bool hasStarted = false;
    private bool isDone = false;
    private Coroutine funCoroutine;

    protected override void Awake()
    {
        base.Awake();

        needs = GetComponent<AgentNeeds>();
        movement = GetComponent<AgentMovement>();

        actionName = "Divertirse";

        AddPrecondition("is_bored", true);
        AddEffect("is_entertained", true);

        cost = 1f;
    }

    public override bool CheckProceduralPrecondition(GameObject agent)
    {
        return needs != null && needs.fun < 40f;
    }

    public override bool IsDone()
    {
        return isDone;
    }

    public override void Perform(GameObject agent)
    {
        if (needs == null)
        {
            isDone = true;
            return;
        }

        if (hasStarted) return;

        hasStarted = true;
        isDone = false;

        if (movement != null)
        {
            movement.StopMoving();
        }

        funCoroutine = StartCoroutine(HaveFunRoutine());
    }

    private IEnumerator HaveFunRoutine()
    {
        float timer = 0f;

        while (timer < duration)
        {
            needs.fun += (funRecovered / duration) * Time.deltaTime;
            needs.fun = Mathf.Clamp(needs.fun, 0f, 100f);

            timer += Time.deltaTime;
            yield return null;
        }

        isDone = true;
        funCoroutine = null;
    }

    public override void ResetAction()
    {
        hasStarted = false;
        isDone = false;

        if (funCoroutine != null)
        {
            StopCoroutine(funCoroutine);
            funCoroutine = null;
        }
    }
}