using System;
using Pathfinder;
using StateMachine.States.RTSStates;
using UnityEngine;

namespace StateMachine.Agents.RTS
{
    /// <summary>
    /// Represents a miner agent responsible for gathering gold.
    /// Inherits from RTSAgent and handles FSM (Finite State Machine) behaviors and transitions.
    /// </summary>
    public class Miner : RTSAgent
    {
        private Action onMine;
        public static Action OnEmptyMine;
        public static Action<Node<Vector2>> OnReachMine;
        public static Action<Node<Vector2>> OnLeaveMine;

        /// <summary>
        /// Initializes the miner agent, sets its type, and sets the initial state to Walk.
        /// </summary>
        public override void Init()
        {
            base.Init();
            AgentType = AgentTypes.Miner;
            nodeThatAffects = NodeType.Dirt;
            cost = 2;
            Fsm.ForceTransition(Behaviours.Walk);
            onMine += Mine;
        }

        /// <summary>
        /// Defines the state transitions for the miner agent.
        /// </summary>
        protected override void FsmTransitions()
        {
            base.FsmTransitions();
            GatherTransitions();
            WalkTransitions();
        }

        /// <summary>
        /// Adds the behaviors to the finite state machine (FSM) for the miner agent.
        /// </summary>
        protected override void FsmBehaviours()
        {
            base.FsmBehaviours();
            Fsm.AddBehaviour<GatherGoldState>(Behaviours.GatherResources, GatherTickParameters, GatherEnterParameters, GatherLeaveParameters);
        }

        /// <summary>
        /// Defines the transitions for the gathering state.
        /// </summary>
        protected override void GatherTransitions()
        {
            base.GatherTransitions();
            Fsm.SetTransition(Behaviours.GatherResources, Flags.OnHunger, Behaviours.Wait, () => Debug.Log("Wait"));

            Fsm.SetTransition(Behaviours.GatherResources, Flags.OnFull, Behaviours.Walk,
                () =>
                {
                    TargetNode = GetTarget(NodeType.TownCenter);

                    if (TargetNode == null) return;

                    Debug.Log("Gold full. Walk to " + TargetNode.GetCoordinate().x + " - " + TargetNode.GetCoordinate().y);
                });
            Fsm.SetTransition(Behaviours.GatherResources, Flags.OnTargetLost, Behaviours.Walk,
                () =>
                {
                    TargetNode = GetTarget();
                    if (TargetNode == null) return;
                    Debug.Log("Mine empty. Walk to " + TargetNode.GetCoordinate().x + " - " + TargetNode.GetCoordinate().y);
                });
        }

        /// <summary>
        /// Parameters for the gathering state tick function, including retreat conditions and resource limits.
        /// </summary>
        /// <returns>An array of parameters used during the gathering tick.</returns>
        protected override object[] GatherTickParameters()
        {
            return new object[] { Retreat, Food, CurrentGold, GoldLimit, onMine, CurrentNode };
        }

        /// <summary>
        /// Parameters for when the agent enters the gathering state, including reaching the mine.
        /// </summary>
        /// <returns>An array of parameters used when entering the gathering state.</returns>
        protected object[] GatherEnterParameters()
        {
            return new object[] { OnReachMine, CurrentNode };
        }

        /// <summary>
        /// Parameters for when the agent leaves the gathering state, including leaving the mine.
        /// </summary>
        /// <returns>An array of parameters used when leaving the gathering state.</returns>
        protected object[] GatherLeaveParameters()
        {
            return new object[] { OnLeaveMine, CurrentNode };
        }

        /// <summary>
        /// Defines the transitions for the walking state.
        /// </summary>
        protected override void WalkTransitions()
        {
            base.WalkTransitions();
            Fsm.SetTransition(Behaviours.Walk, Flags.OnGather, Behaviours.GatherResources, () => Debug.Log("Gather gold"));
        }

        /// <summary>
        /// Parameters for when the agent enters the waiting state, related to reaching the mine.
        /// </summary>
        /// <returns>An array of parameters used when entering the wait state.</returns>
        protected override object[] WaitEnterParameters()
        {
            return new object[] { CurrentNode, OnReachMine };
        }

        /// <summary>
        /// Parameters for when the agent exits the waiting state, related to leaving the mine.
        /// </summary>
        /// <returns>An array of parameters used when exiting the wait state.</returns>
        protected override object[] WaitExitParameters()
        {
            return new object[] { CurrentNode, OnLeaveMine };
        }

        /// <summary>
        /// The mining action performed by the miner. Increases gold collected and manages food consumption.
        /// </summary>
        private void Mine()
        {
            if (Food <= 0 || CurrentNode.gold <= 0) return;

            CurrentGold++;

            LastTimeEat++;
            CurrentNode.gold--;
            if (CurrentNode.gold <= 0) OnEmptyMine?.Invoke();

            if (LastTimeEat < GoldPerFood) return;

            Food--;
            LastTimeEat = 0;

            if (Food > 0 || CurrentNode.food <= 0) return;

            Food++;
            CurrentNode.food--;
        }
    }
}