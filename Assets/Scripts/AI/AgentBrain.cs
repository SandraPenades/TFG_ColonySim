using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

[RequireComponent(typeof(AgentMovement))]
public class AgentBrain : MonoBehaviour
{
    [Header("Prioridades (0-5)")]
    public int prioridadTala = 1;
    public int prioridadTransporte = 1;
    public int prioridadMineria = 1;
    public int prioridadRecoleccion = 1;
    public int prioridadSocializar = 1;

    private AgentMovement movement;
    private AgentNeeds needs;

    private List<GoapAction> availableActions;  // Lista de habilidades del colono
    private GoapAction currentAction;       // Lo que está haciendo en ese momento

    private string currentDecisionReason = "Sin decisión";

    void Start()
    {
        movement = GetComponent<AgentMovement>();
        needs = GetComponent<AgentNeeds>();

        availableActions = GetComponents<GoapAction>().ToList(); // En lista para poder ordenarla
    }

    void Update()
    {
        // Si el colono está ejecutando una acción
        if (currentAction != null)
        {
            // Primero comprobamos si hay una situación crítica
            if (ShouldInterruptCurrentAction())
            {
                string interruptedActionName = currentAction.actionName;

                AbortCurrentAction();

                Debug.Log($"[AgentBrain] Interrumpiendo acción '{interruptedActionName}' por necesidad crítica.");
                return;
            }

            // Si no hay interrupción, comprobamos si la acción ha terminado
            if (currentAction.IsDone())
            {
                currentAction.ResetAction();
                currentAction = null;
                currentDecisionReason = "Acción finalizada";
            }
            return;
        }

        // Si no hay acción en curso, reevaluamos prioridades
        ActualizarPrioridades();

        // Ordenar acciones disponibles por coste/prioridad
        var accionesOrdenadas = availableActions.Where(action => action.cost > 0).OrderBy(action => action.cost).ToList();

        foreach (GoapAction action in accionesOrdenadas)
        {
            if (action.CheckProceduralPrecondition(gameObject))
            {
                currentAction = action;
                currentDecisionReason = GetDecisionReason(action);

                currentAction.Perform(gameObject);

                break;
            }
        }
    }

    private void ActualizarPrioridades()
    {
        if (needs == null) return;
        
        foreach (var action in availableActions)
        {
            // Las acciones que tienen que ver con las necesidades de hambre y sueño, se priorizarán con 1
            // o se desactivarán con 0.
            if (action is Action_Eat) action.cost = needs.IsHungry() ? 1 : 0;
            else if (action is Action_Sleep) action.cost = needs.IsSleepy() ? 1 : 0;

            // Segunda capa de las necesidades urgentes (menos urgentes que las primeras)
            else if (action is Action_HaveFun) action.cost = needs.IsBored() ? 2 : 0;
            else if (action is Action_Chat) action.cost = needs.IsLonely() ? 2 : 0;

            // Las demás acciones siempre valdrán más de 1 (si no es 0) para que las necesidades vayan primero.
            else if (action is Action_ChopTree) action.cost = prioridadTala == 0 ? 0 : prioridadTala + 2;
            else if (action is Action_Haul) action.cost = prioridadTransporte == 0 ? 0 : prioridadTransporte + 2;
            else if (action is Action_Mining) action.cost = prioridadMineria == 0 ? 0 : prioridadMineria + 2;
            else if (action is Action_Harvest) action.cost = prioridadRecoleccion == 0 ? 0 : prioridadRecoleccion + 2;
            // AQUÍ SE AÑADEN LAS NUEVAS ACCIONES

            // Si no hay nada que hacer, que se de un paseo
            else if (action is Action_Wander) action.cost = 99;
        }
    }

    // Para forzar la parada de cualquier acción que esté en proceso cuando sea necesario
    public void AbortCurrentAction()
    {
        if (currentAction != null)
        {
            currentAction.ResetAction();
            currentAction = null;
            currentDecisionReason = "Acción cancelada";
        }
    }

    private bool ShouldInterruptCurrentAction()
    {
        AgentNeeds needs = GetComponent<AgentNeeds>();

        if (needs == null) return false;

        bool criticalHunger = needs.hunger <= 0f && !(currentAction is Action_Eat);
        bool criticalEnergy = needs.energy <= 0f && !(currentAction is Action_Sleep);

        return criticalHunger || criticalEnergy;
    }

    public string GetCurrentActionName()
    {
        return currentAction != null ? currentAction.actionName : "Sin acción";
    }

    // public string GetCurrentGoalName()
    // {
    //     return currentGoal != null ? GetCurrentGoalName.goalName : "Sin objetivo";
    // }

    public string GetCurrentDecisionReason()
    {
        return currentDecisionReason;
    }

    private string GetDecisionReason(GoapAction action)
    {
        if (action is Action_Eat) return "Necesidad: hambre";
        if (action is Action_Sleep) return "Necesidad: energía";
        if (action is Action_HaveFun) return "Necesidad: diversión";
        if (action is Action_Chat) return "Necesidad: socialización";
        if (action is Action_ChopTree) return "Trabajo: tala";
        if (action is Action_Haul) return "Trabajo: transporte";
        if (action is Action_Mining) return "Trabajo: minería";
        if (action is Action_Harvest) return "Trabajo: recolección";
        if (action is Action_Wander) return "Sin tareas disponibles";

        return "Acción seleccionada";
    }
}