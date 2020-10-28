using System.Collections.Generic;

public abstract class State
{
    public abstract List<Action> GetActions();
    public abstract List<Action> GetEntyActions();
    public abstract List<Action> GetExitActions();
    public abstract List<Transition> GetTransitions();
}
