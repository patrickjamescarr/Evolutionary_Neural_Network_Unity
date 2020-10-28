using System.Collections.Generic;

public abstract class Transition
{
    protected List<Action> actions;
    protected State state;
    protected Condition condition;

    public State GetTargetState()
    {
        return state;
    }

    public List<Action> GetActions()
    {
        return actions;
    }

    public bool IsTriggered()
    {
        return condition.Test();
    }    
}
