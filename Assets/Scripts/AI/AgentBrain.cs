using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

[RequireComponent(typeof(AgentMovement))]
[RequireComponent(typeof(WorldStateProvider))]
public class AgentBrain : MonoBehaviour
{
    [Header("Prioridades (0-5)")]
    public int prioridadTala = 1;
    public int prioridadTransporte = 1;
    public int prioridadMineria = 1;
    public int prioridadRecoleccion = 1;
    public int prioridadSocializar = 1;
    public int prioridadContruccion = 1;

    private AgentMovement movement;
    private AgentNeeds needs;
    private WorldStateProvider worldStateProvider;

    private List<GoapAction> availableActions;  // Lista de habilidades del colono
    private GoapAction currentAction;       // Lo que está haciendo en ese momento

    private GoapPlanner planner;
    private Queue<GoapAction> currentPlan;
    private GoapGoal currentGoal;

    private string currentDecisionReason = "Sin decisión";

    void Start()
    {
        movement = GetComponent<AgentMovement>();
        needs = GetComponent<AgentNeeds>();
        worldStateProvider = GetComponent<WorldStateProvider>();

        availableActions = GetComponents<GoapAction>().ToList(); // En lista para poder ordenarla
        planner = new GoapPlanner();
        currentPlan = new Queue<GoapAction>();
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

                ExecuteNextActionInPlan();
            }
            return;
        }

        // Si no hay acción en curso, pero queda plan, seguimos con el plan
        if (currentPlan != null && currentPlan.Count > 0)
        {
            ExecuteNextActionInPlan();
            return;
        }

        // Si no hay acción ni plan, generamos uno nuevo
        CreateAndStartNewPlan();
    }

    private void CreateAndStartNewPlan()
    {
        ActualizarCostesAcciones();

        WorldState currentState = worldStateProvider.BuildWorldState(gameObject);
        List<GoapGoal> goals = BuildGoals(currentState);

        foreach (GoapGoal goal in goals.OrderBy(g => g.priority))
        {
            Queue<GoapAction> plan = planner.Plan(gameObject, availableActions, currentState, goal);

            if (plan != null && plan.Count > 0)
            {
                currentGoal = goal;
                currentPlan = plan;

                currentDecisionReason = $"Objetivo: {currentGoal.goalName}";

                Debug.Log($"[AgentBrain] {gameObject.name} selecciona objetivo: {currentGoal.goalName}");

                ExecuteNextActionInPlan();
                return;
            }
        }

        // Si no hay plan posible, pasear como fallback
        TryWander();
    }

    private List<GoapGoal> BuildGoals(WorldState currentState)
    {
        List<GoapGoal> goals = new List<GoapGoal>();

        if (needs != null && needs.IsHungry())
        {
            goals.Add(new GoapGoal(
                "Comer", 
                new Dictionary<string, bool> { { "is_fed", true } }, 
                1
            ));
        }

        if (needs != null && needs.IsSleepy())
        {
            goals.Add(new GoapGoal(
                "Dormir", 
                new Dictionary<string, bool> { { "is_rested", true } }, 
                1
            ));
        }

        if (currentState.GetState("has_loose_resource") && prioridadTransporte > 0)
        {
            goals.Add(new GoapGoal(
                "Almacenar recursos", 
                new Dictionary<string, bool> { { "resources_stored", true } }, 
                prioridadTransporte + 2
            ));
        }

        if (currentState.GetState("has_tree_job") && prioridadTala > 0)
        {
            goals.Add(new GoapGoal(
                "Talar y almacenar madera", 
                new Dictionary<string, bool> { {"has_loose_wood", true},  { "resources_stored", true } }, 
                prioridadTala + 3
            ));
        }

        if (currentState.GetState("has_mining_job") && prioridadMineria > 0)
        {
            goals.Add(new GoapGoal(
                "Minar y almacenar piedra", 
                new Dictionary<string, bool> { {"has_loose_stone", true},  { "resources_stored", true } }, 
                prioridadMineria + 3
            ));
        }

        if (currentState.GetState("has_harvest_job") && prioridadRecoleccion > 0)
        {
            goals.Add(new GoapGoal(
                "Recolectar y almacenar comida", 
                new Dictionary<string, bool> { {"has_loose_food", true}, { "resources_stored", true } }, 
                prioridadRecoleccion + 3
            ));
        }

        if (currentState.GetState("has_build_job") && prioridadContruccion > 0)
        {
            goals.Add(new GoapGoal(
                "Construir blueprint",
                new Dictionary<string, bool> { { "blueprint_finished", true } },
                prioridadContruccion + 3
            ));
        }

        // De momento socialización y diversión pueden quedarse fuera hasta que estén implementadas
        // if (needs != null && needs.IsLonely())
        // {
        //     goals.Add(new GoapGoal(
        //         "Socializar",
        //         new Dictionary<string, bool> { { "is_socialized", true } },
        //         prioridadSocializar + 2
        //     ));
        // }

        // if (needs != null && needs.IsBored())
        // {
        //     goals.Add(new GoapGoal(
        //         "Divertirse",
        //         new Dictionary<string, bool> { { "is_not_bored", true } },
        //         prioridadDivertirse + 2
        //     ));
        // }

        Debug.Log(
            $"[Jobs] Tala:{JobManager.Instance.CountPendingJobs(Job.JobType.Talar)} " +
            $"Minería:{JobManager.Instance.CountPendingJobs(Job.JobType.Minar)} " +
            $"Recolección:{JobManager.Instance.CountPendingJobs(Job.JobType.Recolectar)} " +
            $"Transporte:{JobManager.Instance.CountPendingJobs(Job.JobType.Transportar)}"
        );

        return goals;

    }

    private void ExecuteNextActionInPlan()
    {
        if (currentPlan == null || currentPlan.Count == 0)
        {
            currentGoal = null;
            currentDecisionReason = "Plan finalizado";
            return;
        }

        GoapAction nextAction = currentPlan.Dequeue();

        // Comprobamos qué acción puede ejecutarse físicamente en Unity
        if (!nextAction.CheckProceduralPrecondition(gameObject))
        {
            Debug.Log($"[AgentBrain] La acción '{nextAction.actionName}' ya no es válida. Replanificando...");

            ClearCurrentPlan();
            CreateAndStartNewPlan();
            return;
        }

        currentAction = nextAction;
        currentDecisionReason = currentGoal != null ? $"Objetivo: {currentGoal.goalName}" : GetDecisionReason(currentAction);

        Debug.Log($"[AgentBrain] Ejecutando acción: {currentAction.actionName}");

        currentAction.Perform(gameObject);
    }

    private void ActualizarCostesAcciones()
    {
        if (needs == null) return;
        
        foreach (var action in availableActions)
        {
            // Las acciones que tienen que ver con las necesidades de hambre y sueño, se priorizarán con 1
            // o se desactivarán con 0.
            if (action is Action_Eat) action.cost = needs.IsHungry() ? 1 : 0;
            else if (action is Action_Sleep) action.cost = needs.IsSleepy() ? 1 : 0;

            // Segunda capa de las necesidades urgentes (menos urgentes que las primeras)
            else if (action is Action_HaveFun) action.cost = 0;
            else if (action is Action_Chat) action.cost = 0;

            // Las demás acciones siempre valdrán más de 1 (si no es 0) para que las necesidades vayan primero.
            else if (action is Action_ChopTree) action.cost = prioridadTala == 0 ? 0 : prioridadTala + 2;
            else if (action is Action_Haul) action.cost = prioridadTransporte == 0 ? 0 : prioridadTransporte + 2;
            else if (action is Action_Mining) action.cost = prioridadMineria == 0 ? 0 : prioridadMineria + 2;
            else if (action is Action_Harvest) action.cost = prioridadRecoleccion == 0 ? 0 : prioridadRecoleccion + 2;
            else if (action is Action_Build) action.cost = prioridadContruccion == 0 ? 0 : prioridadContruccion + 2;
            // AQUÍ SE AÑADEN LAS NUEVAS ACCIONES

            // Si no hay nada que hacer, que se de un paseo
            else if (action is Action_Wander) action.cost = 0; // Esta acción no entra en el GOAP por lo que no tiene coste en el plan
        }
    }

    private void TryWander()
    {
        GoapAction wanderAction = availableActions.FirstOrDefault(action => action is Action_Wander);

        if (wanderAction != null && wanderAction.CheckProceduralPrecondition(gameObject))
        {
            currentAction = wanderAction;
            currentGoal = null;
            currentDecisionReason = "Sin plan: deambular";

            currentAction.Perform(gameObject);
        }
        else
        {
            currentDecisionReason = "Sin plan disponible";
        }
    }

    // Para forzar la parada de cualquier acción que esté en proceso cuando sea necesario
    public void AbortCurrentAction()
    {
        if (currentAction != null)
        {
            currentAction.ResetAction();
            currentAction = null;
        }

        if (movement != null)
        {
            movement.StopMoving();
        }

        ClearCurrentPlan();

        currentDecisionReason = "Acción cancelada";
    }

    private void ClearCurrentPlan()
    {
        if (currentPlan != null)
        {
            currentPlan.Clear();
        }

        currentGoal = null;
    }

    private bool ShouldInterruptCurrentAction()
    {
        AgentNeeds needs = GetComponent<AgentNeeds>();

        if (needs == null) return false;

        bool criticalHunger = needs.hunger <= 0f && !(currentAction is Action_Eat) && CanPerformAction<Action_Eat>();
        bool criticalEnergy = needs.energy <= 0f && !(currentAction is Action_Sleep) && CanPerformAction<Action_Sleep>();

        return criticalHunger || criticalEnergy;
    }

    private bool CanPerformAction<T>() where T : GoapAction
    {
        foreach (GoapAction action in availableActions)
        {
            if (action is T)
            {
                return action.CheckProceduralPrecondition(gameObject);
            }
        }

        return false;
    }

    public string GetCurrentActionName()
    {
        return currentAction != null ? currentAction.actionName : "Sin acción";
    }

    public string GetCurrentGoalName()
    {
        return currentGoal != null ? currentGoal.goalName : "Sin objetivo";
    }

    public string GetCurrentDecisionReason()
    {
        return currentDecisionReason;
    }

    public string GetCurrentPlanDescription()
    {
        if (currentPlan == null || currentPlan.Count == 0)
        {
            return "Sin plan";
        }

        return string.Join(" -> ", currentPlan.Select(action => action.actionName));
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

/// El flujo de AgentBrain será:
/// construye WorldState
/// crea objetivos posibles
/// pide plan al GoapPlanner
/// recibe una cola de acciones
/// ejecuta acción 1
/// cuando termina, ejecuta acción 2
/// si algo falla, replanifica