using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Action_HaveFun : GoapAction
{
    protected override void Awake()
    {
        base.Awake();

        actionName = "Divertirse";
        cost = 0;

        AddPrecondition("has_fun_need", true);
        AddPrecondition("has_recreation_available", true);
        AddEffect("is_entertained", true);
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