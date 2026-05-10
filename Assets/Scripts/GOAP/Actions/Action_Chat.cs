using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Action_Chat : GoapAction
{
    protected override void Awake()
    {
        base.Awake();

        actionName = "Socializar";
        cost = 0;

        AddPrecondition("has_social_need", true);
        AddPrecondition("has_available_colonist", true);
        AddEffect("is_socialized", true);
    }

    public override bool CheckProceduralPrecondition(GameObject agent)
    {
        return false;
    }

    public override void Perform(GameObject agent)
    {
    }

    public override bool IsDone()
    {
        return true;
    }

    public override void ResetAction()
    {
    }
}
