using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class GoapPlanner
{
    private class Node
    {
        public Node parent;
        public float runningCost;
        public WorldState state;
        public GoapAction action;

        public Node(Node parent, float runningCost, WorldState state, GoapAction action)
        {
            this.parent = parent;
            this.runningCost = runningCost;
            this.state = state;
            this.action = action;
        }
    }

    public Queue<GoapAction> Plan (GameObject agent, List<GoapAction> availableActions, WorldState currentState, GoapGoal goal)
    {
        List<GoapAction> usableActions = new List<GoapAction>();

        foreach (GoapAction action in availableActions)
        {
            if (action.CheckProceduralPrecondition(agent))
            {
                usableActions.Add(action);
            }
        }

        List<Node> leaves = new List<Node>();

        Node start = new Node(null, 0, currentState, null);

        bool success = BuildGraph(start, leaves, usableActions, goal.desiredState);

        if (!success)
        {
            Debug.Log($"[GoapPlanner] No se ha encontrado plan para el objetivo: {goal.goalName}");
            return null;
        }

        Node cheapest = null;

        foreach (Node leaf in leaves)
        {
            if (cheapest == null || leaf.runningCost < cheapest.runningCost)
            {
                cheapest = leaf;
            }
        }

        List<GoapAction> result = new List<GoapAction>();
        Node n = cheapest;

        while (n != null)
        {
            if (n.action != null)
            {
                result.Insert(0, n.action);
            }

            n = n.parent;
        }

        Queue<GoapAction> queue = new Queue<GoapAction>();

        foreach (GoapAction action in result)
        {
            queue.Enqueue(action);
        }

        Debug.Log($"[GoapPlanner] Plan encontrado para '{goal.goalName}': " + string.Join(" -> ", result.Select(a => a.actionName)));

        return queue;
    }

    private bool BuildGraph (Node parent, List<Node> leaves, List<GoapAction> usableActions, Dictionary<string, bool> goal)
    {
        bool foundPath = false;

        foreach (GoapAction action in usableActions)
        {
            if (InState(action.preconditions, parent.state))
            {
                WorldState currentState = parent.state.Clone();
                currentState.ApplyEffects(action.effects);

                Node node = new Node(parent, parent.runningCost + action.cost, currentState, action);

                if (currentState.Satisfies(goal))
                {
                    leaves.Add(node);
                    foundPath = true;
                }
                else
                {
                    List<GoapAction> remainingActions = ActionSubset(usableActions, action);
                    bool found = BuildGraph(node, leaves, remainingActions, goal);

                    if (found)
                    {
                        foundPath = true;
                    }
                }
            }
        }

        return foundPath;
    }

    private bool InState(Dictionary<string, bool> test, WorldState state)
    {
        foreach (var condition in test)
        {
            if (state.GetState(condition.Key) != condition.Value)
            {
                return false;
            }
        }

        return true;
    }

    private List<GoapAction> ActionSubset(List<GoapAction> actions, GoapAction actionToRemove)
    {
        List<GoapAction> subset = new List<GoapAction>();

        foreach (GoapAction action in actions)
        {
            if (!action.Equals(actionToRemove))
            {
                subset.Add(action);
            }
        }

        return subset;
    }
}
