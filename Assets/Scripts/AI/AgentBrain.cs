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
    public int prioridadBasicos = 1; // Aquí pueden entrar cosas como reponer combustible en cosas entre otros.

    private AgentMovement movement;
    private List<GoapAction> availableActions;  // Lista de habilidades del colono
    private GoapAction currentAction;       // Lo que está haciendo en ese momento

    void Start()
    {
        movement = GetComponent<AgentMovement>();
        availableActions = GetComponents<GoapAction>().ToList(); // En lista para poder ordenarla
    }

    void Update()
    {
        // Si el colono está ocupado:
        if (currentAction != null)
        {
            if (currentAction.IsDone())
            {
                // Debug.Log($"[AgentBrain] Acción '{currentAction.actionName}' terminada.");
                currentAction.ResetAction();
                currentAction = null;
            }
            return;
        }

        // Por si hay cambios en las prioridades
        ActualizarPrioridades();

        // Ordenar la lista según su prioridad
        var accionesOrdenadas = availableActions.Where(accionesOrdenadas => accionesOrdenadas.cost > 0).OrderBy(accionesOrdenadas => accionesOrdenadas.cost).ToList();

        // Vemos cuál es la mejor tarea disponible
        foreach (GoapAction action in accionesOrdenadas)
        {
            // ¿Se cumplen las condiciones en el mundo para hacerte?
            if (action.CheckProceduralPrecondition(gameObject))
            {
                // Debug.Log($"[AgentBrain] He decidido: {action.actionName}");
                currentAction = action;

                currentAction.Perform(gameObject);

                // No se buscan más tareas de momento
                break;
            }
        }
    }

    private void ActualizarPrioridades()
    {
        AgentNeeds needs = GetComponent<AgentNeeds>();

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
            //else if (action is Action_Basic) action.cost = prioridadBasicos == 0 ? 0 : prioridadBasicos + 2;
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
            StopAllCoroutines();

            currentAction.ResetAction();

            currentAction = null;
        }
    }
}