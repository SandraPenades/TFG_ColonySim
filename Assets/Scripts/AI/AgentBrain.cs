using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public enum WorkType
{
    Talar,
    Minar,
    Recolectar,
    Transportar,
    Construir
}

[RequireComponent(typeof(AgentMovement))]
[RequireComponent(typeof(WorldStateProvider))]
public class AgentBrain : MonoBehaviour
{
    [Header("Prioridades (0-5)")]
    public int prioridadTala = 1;
    public int prioridadRecoleccion = 1;
    public int prioridadMineria = 1;
    public int prioridadTransporte = 1;
    public int prioridadConstruccion = 1;

    private AgentMovement movement;
    private AgentNeeds needs;
    private WorldStateProvider worldStateProvider;

    private List<GoapAction> availableActions;  // Lista de habilidades del colono
    private GoapAction currentAction;       // Lo que está haciendo en ese momento

    private GoapPlanner planner;
    private Queue<GoapAction> currentPlan;
    private GoapGoal currentGoal;

    private string currentDecisionReason = "Sin decisión";

    private ColonistThoughtBubble thoughtBubble;

    void Start()
    {
        movement = GetComponent<AgentMovement>();
        needs = GetComponent<AgentNeeds>();
        worldStateProvider = GetComponent<WorldStateProvider>();
        thoughtBubble = GetComponent<ColonistThoughtBubble>();

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

                // Debug.Log($"[AgentBrain] Interrumpiendo acción '{interruptedActionName}' por necesidad crítica.");
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

                // Debug.Log($"[AgentBrain] {gameObject.name} selecciona objetivo: {currentGoal.goalName}");

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

        // Si tiene hambre, el objetivo es comer
        if (needs != null && needs.IsHungry() && currentState.GetState("has_food_available"))
        {
            goals.Add(new GoapGoal(
                "Comer", 
                new Dictionary<string, bool> { { "is_fed", true } }, 
                1
            ));
        }

        // Si tiene sueño, el objetivo es dormir
        if (needs != null && needs.IsSleepy() && currentState.GetState("has_free_bed"))
        {
            goals.Add(new GoapGoal(
                "Dormir", 
                new Dictionary<string, bool> { { "is_rested", true } }, 
                2
            ));
        }

        // Si no tiene socialización, el objetivo es socializar
        if (needs != null && needs.IsLonely())
        {
            goals.Add(new GoapGoal(
                "Socializar",
                new Dictionary<string, bool> { { "is_socialized", true } },
                3
            ));
        }

        // Si está aburrido, el objetivo es divertirse
        if (needs != null && needs.IsBored())
        {
            goals.Add(new GoapGoal(
                "Divertirse",
                new Dictionary<string, bool> { { "is_entertained", true } },
                4
            ));
        }

        // Si tiene un objeto en el suelo y puede transportar, el objetivo es transportar el recurso
        if (currentState.GetState("has_loose_resource") && prioridadTransporte > 0)
        {
            goals.Add(new GoapGoal(
                "Almacenar recursos", 
                new Dictionary<string, bool> { { "resources_stored", true } }, 
                prioridadTransporte + 5
            ));
        }

        // Si hay un trabajo de tala y puede talar, el objetivo es talar
        if (currentState.GetState("has_tree_job") && prioridadTala > 0)
        {
            goals.Add(new GoapGoal(
                "Talar madera", 
                new Dictionary<string, bool> { {"has_loose_wood", true} }, 
                prioridadTala + 5
            ));
        }

        // Si hay un trabajo de minería y puede minar, el objetivo es minar
        if (currentState.GetState("has_mining_job") && prioridadMineria > 0)
        {
            goals.Add(new GoapGoal(
                "Minar piedra", 
                new Dictionary<string, bool> { {"has_loose_stone", true} }, 
                prioridadMineria + 5
            ));
        }

        // Si hay un trabajo de recolección y puede recolectar, el objetivo es recolectar
        if (currentState.GetState("has_harvest_job") && prioridadRecoleccion > 0)
        {
            goals.Add(new GoapGoal(
                "Recolectar comida", 
                new Dictionary<string, bool> { {"has_loose_food", true} }, 
                prioridadRecoleccion + 5
            ));
        }

        // Si hay un trabajo de deconstrucción y puede construir, el objetivo es deconstruir
        if (currentState.GetState("has_deconstruction_job") && prioridadConstruccion > 0)
        {
            goals.Add(new GoapGoal(
                "Deconstruir edificio",
                new Dictionary<string, bool> { { "building_deconstructed", true } },
                prioridadConstruccion + 5
            ));
        }

        // Si hay un trabajo de construcción, tiene recursos y puede construir, el objetivo es construir
        if (currentState.GetState("has_build_job") && currentState.GetState("has_required_build_resources") && prioridadConstruccion > 0)
        {
            goals.Add(new GoapGoal(
                "Construir blueprint",
                new Dictionary<string, bool> { { "blueprint_finished", true } },
                prioridadConstruccion + 5
            ));
        }

        // Objetivos más complejos
        if (currentState.GetState("has_build_job") && 
            !currentState.GetState("has_required_build_resources") &&
            currentState.GetState("missing_wood_for_build") &&
            currentState.GetState("has_tree_job") &&
            prioridadConstruccion > 0 && prioridadTala > 0 && prioridadTransporte > 0)
        {
            goals.Add(new GoapGoal(
                "Preparar madera para construcción",
                new Dictionary<string, bool> { { "resources_stored", true } },
                prioridadConstruccion + 4
            ));
        }

        if (currentState.GetState("has_build_job") && 
            !currentState.GetState("has_required_build_resources") &&
            currentState.GetState("missing_stone_for_build") &&
            currentState.GetState("has_mining_job") &&
            prioridadConstruccion > 0 && prioridadMineria > 0 && prioridadTransporte > 0)
        {
            goals.Add(new GoapGoal(
                "Preparar piedra para construcción",
                new Dictionary<string, bool> { { "resources_stored", true } },
                prioridadConstruccion + 4
            ));
        }

        // Debug.Log(
        //     $"[Jobs] Tala:{JobManager.Instance.CountPendingJobs(Job.JobType.Talar)} " +
        //     $"Minería:{JobManager.Instance.CountPendingJobs(Job.JobType.Minar)} " +
        //     $"Recolección:{JobManager.Instance.CountPendingJobs(Job.JobType.Recolectar)} " +
        //     $"Transporte:{JobManager.Instance.CountPendingJobs(Job.JobType.Transportar)}"
        // );

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
            ClearCurrentPlan();
            currentAction = null;
            currentDecisionReason = $"No se pudo ejecutar: {nextAction.actionName}";
            return;
        }

        currentAction = nextAction;
        currentDecisionReason = currentGoal != null ? $"Objetivo: {currentGoal.goalName}" : GetDecisionReason(currentAction);

        // Debug.Log($"[AgentBrain] Ejecutando acción: {currentAction.actionName}");

        if (thoughtBubble != null)
        {
            thoughtBubble.ShowThought(currentAction.actionName);
        }

        currentAction.Perform(gameObject);
    }

    private void ActualizarCostesAcciones()
    {
        if (needs == null) return;
        
        foreach (var action in availableActions)
        {
            // Las acciones que tienen que ver con las necesidades, se priorizarán con 1
            // o se desactivarán con 0.
            if (action is Action_Eat) action.cost = needs.IsHungry() ? 1 : 0;
            else if (action is Action_Sleep) action.cost = needs.IsSleepy() ? 1 : 0;
            else if (action is Action_HaveFun) action.cost = needs.IsBored() ? 1 : 0;
            else if (action is Action_Chat) action.cost = needs.IsLonely() ? 1 : 0;

            // Las demás acciones siempre valdrán más de 1 (si no es 0) para que las necesidades vayan primero.
            else if (action is Action_ChopTree) action.cost = prioridadTala == 0 ? 0 : prioridadTala + 2;
            else if (action is Action_Haul) action.cost = prioridadTransporte == 0 ? 0 : prioridadTransporte + 2;
            else if (action is Action_Mining) action.cost = prioridadMineria == 0 ? 0 : prioridadMineria + 2;
            else if (action is Action_Harvest) action.cost = prioridadRecoleccion == 0 ? 0 : prioridadRecoleccion + 2;
            else if (action is Action_Build) action.cost = prioridadConstruccion == 0 ? 0 : prioridadConstruccion + 2;
            else if (action is Action_Deconstruct) action.cost = prioridadConstruccion == 0 ? 0 : prioridadConstruccion + 2;

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

            if (thoughtBubble != null)
            {
                thoughtBubble.ShowThought(currentAction.actionName);
            }

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

        if (thoughtBubble != null)
        {
            thoughtBubble.HideThought();
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

    public int GetPriority(WorkType workType)
    {
        switch (workType)
        {
            case WorkType.Talar:
                return prioridadTala;
            case WorkType.Minar:
                return prioridadMineria;
            case WorkType.Recolectar:
                return prioridadRecoleccion;
            case WorkType.Transportar:
                return prioridadTransporte;
            case WorkType.Construir:
                return prioridadConstruccion;
            default:
                return 0;
        }
    }

    public void SetPriority(WorkType workType, int value)
    {
        value = Mathf.Clamp(value, 0, 5);

        switch (workType)
        {
            case WorkType.Talar:
                prioridadTala = value;
                break;
            case WorkType.Minar:
                prioridadMineria = value;
                break;
            case WorkType.Recolectar:
                prioridadRecoleccion = value;
                break;
            case WorkType.Transportar:
                prioridadTransporte = value;
                break;
            case WorkType.Construir:
                prioridadConstruccion = value;
                break;
        }

        // Si cambia mientras el colono ya tiene plan
        ClearCurrentPlan();
        currentDecisionReason = "Pensando en otro plan";
    }

    public void IncreasePriorityValue(WorkType workType)
    {
        SetPriority(workType, GetPriority(workType) + 1);
    }

    public void DecreasePriorityValue(WorkType workType)
    {
        SetPriority(workType, GetPriority(workType) - 1);
    }

    private bool ShouldInterruptCurrentAction()
    {
        if (needs == null) return false;
        if (worldStateProvider == null) return false;

        // No interrumpir acciones de necesidades básicas.
        // Si un colono ya está comiendo o durmiendo, que termine.
        if (currentAction is Action_Eat || currentAction is Action_Sleep)
        {
            return false;
        }

        WorldState state = worldStateProvider.BuildWorldState(gameObject);

        bool criticalHunger = needs.hunger <= needs.hungerThreshold
            && !(currentAction is Action_Eat)
            && state.GetState("has_food_available");

        bool criticalEnergy = needs.energy <= needs.energyThreshold
            && !(currentAction is Action_Sleep)
            && state.GetState("has_free_bed");

        return criticalHunger || criticalEnergy;
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
        if (action is Action_Build) return "Trabajo: construcción";
        if (action is Action_Deconstruct) return "Trabajo: deconstrucción";
        //if (action is Action_Cook) return "Trabajo: cocina";
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