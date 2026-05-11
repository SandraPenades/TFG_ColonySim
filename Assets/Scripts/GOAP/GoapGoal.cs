using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GoapGoal
{
    public string goalName;
    public Dictionary<string, bool> desiredState;
    public int priority;

    public GoapGoal(string goalName, Dictionary<string, bool> desiredState, int priority)
    {
        this.goalName = goalName;
        this.desiredState = desiredState;
        this.priority = priority;
    }
}
