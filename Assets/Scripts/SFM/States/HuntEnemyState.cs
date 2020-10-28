using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HuntEnemyState : State
{
    private Action hunt;
    private Transition enemySpotted;

    public HuntEnemyState()
    {

    }

    public override List<Action> GetActions()
    {
        return new List<Action> { hunt };
    }

    public override List<Action> GetEntyActions()
    {
        return new List<Action>();
    }

    public override List<Action> GetExitActions()
    {
        return new List<Action>();
    }

    public override List<Transition> GetTransitions()
    {
        return new List<Transition> { enemySpotted };
    }
}
