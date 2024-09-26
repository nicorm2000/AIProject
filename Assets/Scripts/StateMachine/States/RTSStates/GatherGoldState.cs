using System;
using Pathfinder;
using StateMachine.Agents.RTS;
using States;
using UnityEngine;

namespace StateMachine.States.RTSStates
{
    public class GatherGoldState : State
    {
        /// <summary>
        /// Gets the tick behaviour for the state, including actions for gathering gold and potential transitions based on game conditions.
        /// </summary>
        /// <param name="parameters">Parameters for the behaviour: retreat flag, food amount, gold amount, gold limit, mine action, and current node.</param>
        /// <returns>A BehaviourActions object containing the behaviours to execute.</returns>
        public override BehaviourActions GetTickBehaviour(params object[] parameters)
        {
            BehaviourActions behaviours = new BehaviourActions();

            bool retreat = Convert.ToBoolean(parameters[0]);
            int food = Convert.ToInt32(parameters[1]);
            int gold = Convert.ToInt32(parameters[2]);
            int goldLimit = Convert.ToInt32(parameters[3]);
            Action OnMine = parameters[4] as Action;
            Node<Vector2> currentNode = parameters[5] as Node<Vector2>;

            behaviours.AddMultiThreadableBehaviours(0, () =>
            {
                OnMine?.Invoke();
            });

            behaviours.SetTransitionBehaviour(() =>
            {
                if (retreat) OnFlag?.Invoke(RTSAgent.Flags.OnRetreat);
                if (food <= 0) OnFlag?.Invoke(RTSAgent.Flags.OnHunger);
                if (gold >= goldLimit) OnFlag?.Invoke(RTSAgent.Flags.OnFull);
                if (currentNode.gold <= 0) OnFlag?.Invoke(RTSAgent.Flags.OnTargetLost);
            });

            return behaviours;
        }

        /// <summary>
        /// Gets the on enter behaviour for the state, including actions to perform when reaching a mining location.
        /// </summary>
        /// <param name="parameters">Parameters for the behaviour: on reach action and current node.</param>
        /// <returns>A BehaviourActions object representing the on enter behaviour.</returns>
        public override BehaviourActions GetOnEnterBehaviour(params object[] parameters)
        {
            BehaviourActions behaviours = new BehaviourActions();

            Action<Node<Vector2>> onReachMine = parameters[0] as Action<Node<Vector2>>;
            Node<Vector2> currentNode = parameters[1] as Node<Vector2>;

            behaviours.AddMainThreadBehaviours(0, () =>
            {
                onReachMine?.Invoke(currentNode);
            });

            return behaviours;
        }

        /// <summary>
        /// Gets the on exit behaviour for the state, including actions to perform when leaving a mining location.
        /// </summary>
        /// <param name="parameters">Parameters for the behaviour: on leave action and current node.</param>
        /// <returns>A BehaviourActions object representing the on exit behaviour.</returns>
        public override BehaviourActions GetOnExitBehaviour(params object[] parameters)
        {
            BehaviourActions behaviours = new BehaviourActions();

            Action<Node<Vector2>> onLeaveMine = parameters[0] as Action<Node<Vector2>>;
            Node<Vector2> currentNode = parameters[1] as Node<Vector2>;

            behaviours.AddMainThreadBehaviours(0, () =>
            {
                onLeaveMine?.Invoke(currentNode);
            });

            return behaviours;
        }
    }
}