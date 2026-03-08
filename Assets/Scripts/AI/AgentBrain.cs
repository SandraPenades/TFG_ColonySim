using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(AgentMovement))]
public class AgentBrain : MonoBehaviour
{
    private AgentMovement movement;
    private GoapAction[] availableActions;  // Lista de habilidades del colono
    private GoapAction currentAction;       // Lo que está haciendo en ese momento

    void Start()
    {
        movement = GetComponent<AgentMovement>();
        availableActions = GetComponents<GoapAction>();
    }

    void Update()
    {
        // Si el colono está ocupado:
        if (currentAction != null)
        {
            if (currentAction.IsDone())
            {
                Debug.Log($"[AgentBrain] Acción '{currentAction.actionName}' terminada.");
                currentAction.ResetAction();
                currentAction = null;
            }
            return;
        }

        // Si el colono está libre
        foreach (GoapAction action in availableActions)
        {
            // ¿Se cumplen las condiciones en el mundo para hacerte?
            if (action.CheckProceduralPrecondition(gameObject))
            {
                Debug.Log($"[AgentBrain] He decidido: {action.actionName}");
                currentAction = action;

                currentAction.Perform(gameObject);

                // No se buscan más tareas de momento
                break;
            }
        }
    }
}