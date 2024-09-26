using System;
using StateMachine.Agents.RTS;
using States;

namespace StateMachine.States.RTSStates
{
    public class DeliverFoodState : State
    {
        /// <summary>
        /// Gets the tick behaviour for the state, including actions for delivering food and potential transitions.
        /// </summary>
        /// <param name="parameters">Parameters for the behaviour: food amount, delivery action, and retreat flag.</param>
        /// <returns>A BehaviourActions object containing the behaviours to execute.</returns>
        public override BehaviourActions GetTickBehaviour(params object[] parameters)
        {
            BehaviourActions behaviours = new BehaviourActions();
            int food = Convert.ToInt32(parameters[0]);
            Action onDeliverFood = parameters[1] as Action;
            bool retreat = Convert.ToBoolean(parameters[2]);

            behaviours.AddMultiThreadableBehaviours(0, () =>
            {
                onDeliverFood?.Invoke();
            });

            behaviours.SetTransitionBehaviour(() =>
            {
                if (food <= 0) OnFlag?.Invoke(RTSAgent.Flags.OnHunger);
                if (retreat) OnFlag?.Invoke(RTSAgent.Flags.OnRetreat);
            });

            return behaviours;
        }

        /// <summary>
        /// Gets the on enter behaviour for the state.
        /// Currently returns default behaviour with no additional actions.
        /// </summary>
        /// <param name="parameters">Parameters for the behaviour.</param>
        /// <returns>A BehaviourActions object representing the on enter behaviour.</returns>
        public override BehaviourActions GetOnEnterBehaviour(params object[] parameters)
        {
            return default;
        }

        /// <summary>
        /// Gets the on exit behaviour for the state.
        /// Currently returns default behaviour with no additional actions.
        /// </summary>
        /// <param name="parameters">Parameters for the behaviour.</param>
        /// <returns>A BehaviourActions object representing the on exit behaviour.</returns>
        public override BehaviourActions GetOnExitBehaviour(params object[] parameters)
        {
            return default;
        }
    }
}