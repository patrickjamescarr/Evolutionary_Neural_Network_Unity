using System.Collections.Generic;

public class AIFSM
{
    public State initialState;
    public State currentState;

    // Checks and applies transition, returning a list of actions
    public List<Action> GetCurrentActions()
    {
        // assume no transition is found
        Transition triggeredTransition = null;

        foreach(var transition in currentState.GetTransitions())
        {
            if(transition.IsTriggered())
            {
                triggeredTransition = transition;
                break;
            }
        }

        // check if we have a transition
        if (triggeredTransition != null)
        {
            // find the target state
            var targetState = triggeredTransition.GetTargetState();

            // add the exit action of the old state,
            // the transition action and the entry for the new state.
            var actions = currentState.GetExitActions();
            var triggeredActions = triggeredTransition.GetActions();
            var entryActions = targetState.GetActions();

            actions.AddRange(triggeredActions);
            actions.AddRange(entryActions);

            // complete the transition and return the action list.
            currentState = targetState;

            return actions;
        }

        return currentState.GetActions();
    }
}
