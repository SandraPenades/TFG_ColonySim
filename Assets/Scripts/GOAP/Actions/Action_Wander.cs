using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Action_Wander : GoapAction
{
    private bool isDone = false;
    private AgentMovement movement;
    private AgentNeeds needs;

    public float wanderRadius = 4f;
    public float maxWaitTime = 2f;
    public float funRegenRate = 2f;

    protected override void Awake()
    {
        base.Awake();
        
        actionName = "Pasear";

        // Para el GOAP
        AddEffect("is_idle", true);

        needs = GetComponent<AgentNeeds>();
    }

    public override bool CheckProceduralPrecondition(GameObject agent)
    {
        // No hay condiciones, siempre es true
        return true;
    }

    public override void Perform(GameObject agent)
    {
        movement = agent.GetComponent<AgentMovement>();

        // Generar punto aleatorio
        Vector2 randomDirection = Random.insideUnitCircle * wanderRadius;
        Vector3 randomPos = agent.transform.position + new Vector3(randomDirection.x, randomDirection.y, 0);

        movement.MoveTo(randomPos);
        StartCoroutine(WanderRoutine());
    }

    private IEnumerator WanderRoutine()
    {
        // Quedarse parado (como si estuviera mirando el paisaje)
        float waitTime = Random.Range(1f, maxWaitTime);
        float timer = 0f;

        while (timer < waitTime)
        {
            timer += Time.deltaTime;
            AumentarDiversionIncremental();
            yield return null;
        }

        // Caminar hasta el punto aleatorio
        while (!movement.HasReachedDestination())
        {
            AumentarDiversionIncremental();
            yield return null;
        }

        movement.StopMoving();

        // Terminar la acción para ver si hay trabajos
        isDone = true;
    }

    private void AumentarDiversionIncremental()
    {
        if (needs != null && needs.fun < 100f)
        {
            needs.fun += funRegenRate * Time.deltaTime;
            if (needs.fun > 100f) needs.fun = 100f; // Que no pase de 100
        }
    }

    public override bool IsDone() => isDone;
    public override void ResetAction()
    {
        StopAllCoroutines();
        isDone = false;
    }
}
