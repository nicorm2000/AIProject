using System;
using StateMachine.Agents.RTS;
using States;

namespace StateMachine.States.RTSStates
{
    public class GetFoodState : State
    {
        /// <summary>
        /// Gets the tick behaviour for the state, including actions for gathering food and handling transitions based on conditions.
        /// </summary>
        /// <param name="parameters">Parameters for the behaviour: current food amount, food limit, gathering action, and retreat flag.</param>
        /// <returns>A BehaviourActions object containing the behaviours to execute.</returns>
        public override BehaviourActions GetTickBehaviour(params object[] parameters)
        {
            BehaviourActions behaviours = new BehaviourActions();
            int food = Convert.ToInt32(parameters[0]);
            int foodLimit = Convert.ToInt32(parameters[1]);
            Action onGatherFood = parameters[2] as Action;
            bool retreat = Convert.ToBoolean(parameters[3]);

            behaviours.AddMultiThreadableBehaviours(0, () =>
            {
                onGatherFood?.Invoke();
            });

            behaviours.SetTransitionBehaviour(() =>
            {
                if (food >= foodLimit) OnFlag?.Invoke(RTSAgent.Flags.OnFull);
                if (retreat) OnFlag?.Invoke(RTSAgent.Flags.OnRetreat);
            });

            return behaviours;
        }

        /// <summary>
        /// Gets the on enter behaviour for the state.
        /// Currently, it does not perform any actions upon entering this state.
        /// </summary>
        /// <param name="parameters">Parameters for the behaviour (not used).</param>
        /// <returns>A default BehaviourActions object.</returns>
        public override BehaviourActions GetOnEnterBehaviour(params object[] parameters)
        {
            return default;
        }

        /// <summary>
        /// Gets the on exit behaviour for the state.
        /// Currently, it does not perform any actions upon exiting this state.
        /// </summary>
        /// <param name="parameters">Parameters for the behaviour (not used).</param>
        /// <returns>A default BehaviourActions object.</returns>
        public override BehaviourActions GetOnExitBehaviour(params object[] parameters)
        {
            return default;
        }
    }
}