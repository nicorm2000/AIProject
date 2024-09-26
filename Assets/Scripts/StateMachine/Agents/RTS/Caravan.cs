using System;
using Game;
using Pathfinder;
using Pathfinder.Graph;
using StateMachine.States.RTSStates;
using UnityEngine;

namespace StateMachine.Agents.RTS
{
    /// <summary>
    /// Represents a caravan agent responsible for gathering and delivering food.
    /// Inherits from RTSAgent and handles FSM (Finite State Machine) behaviors and transitions.
    /// </summary>
    public class Caravan : RTSAgent
    {
        private Action onGather;
        private Action onDeliver;

        /// <summary>
        /// Initializes the Caravan agent, setting its type and starting state.
        /// </summary>
        public override void Init()
        {
            base.Init();
            AgentType = AgentTypes.Caravan;
            Fsm.ForceTransition(Behaviours.GatherResources);
            onGather += Gather;
            onDeliver += DeliverFood;
        }

        /// <summary>
        /// Increases the food resource when called.
        /// </summary>
        private void Gather()
        {
            Food++;
        }

        /// <summary>
        /// Delivers food to the current node, decreasing the Caravan's food count.
        /// </summary>
        private void DeliverFood()
        {
            if (Food <= 0) return;

            Food--;
            CurrentNode.food++;
        }

        /// <summary>
        /// Configures FSM behaviors for the Caravan (e.g., gathering, delivering).
        /// </summary>
        protected override void FsmBehaviours()
        {
            base.FsmBehaviours();
            Fsm.AddBehaviour<GetFoodState>(Behaviours.GatherResources, GetFoodTickParameters);
            Fsm.AddBehaviour<DeliverFoodState>(Behaviours.Deliver, DeliverTickParameters);
        }

        /// <summary>
        /// Configures FSM behaviors for the Caravan (e.g., gathering, delivering).
        /// </summary>
        protected override void FsmTransitions()
        {
            base.FsmTransitions();
            GetFoodTransitions();
            WalkTransitions();
            DeliverTransitions();
        }

        /// <summary>
        /// Configures FSM transitions related to the food gathering behavior.
        /// </summary>
        protected override void GetFoodTransitions()
        {
            Fsm.SetTransition(Behaviours.GatherResources, Flags.OnFull, Behaviours.Walk,
                () =>
                {
                    if (GameManager.MinesWithMiners == null || GameManager.MinesWithMiners.Count <= 0)
                    {
                        Debug.LogWarning("No mines with miners.");
                        return;
                    }
                    Node<Vector2> target = GameManager.MinesWithMiners[0];
                    if (target == null) return;
                    TargetNode = Graph<Node<Vector2>, NodeVoronoi, Vector2>.NodesType.Find(node => node.GetCoordinate() == target.GetCoordinate());
                    if (TargetNode == null) return;
                    Debug.Log("Get Food.");
                });
            Fsm.SetTransition(Behaviours.GatherResources, Flags.OnRetreat, Behaviours.Walk,
                () =>
                {
                    TargetNode = GetTarget(NodeType.TownCenter);
                    if (TargetNode == null) return;
                    Debug.Log("Retreat. Walk to " + TargetNode.GetCoordinate().x + " - " + TargetNode.GetCoordinate().y);
                });
        }

        /// <summary>
        /// Configures FSM transitions related to the walking behavior.
        /// </summary>
        protected override void WalkTransitions()
        {
            Fsm.SetTransition(Behaviours.Walk, Flags.OnRetreat, Behaviours.Walk,
                () =>
                {
                    TargetNode = GetTarget(NodeType.TownCenter);
                    if (TargetNode == null) return;
                    Debug.Log("Retreat. Walk to " + TargetNode.GetCoordinate().x + " - " + TargetNode.GetCoordinate().y);
                });
            Fsm.SetTransition(Behaviours.Walk, Flags.OnTargetLost, Behaviours.Walk,
                () =>
                {
                    if (GameManager.MinesWithMiners == null || GameManager.MinesWithMiners.Count <= 0)
                    {
                        Debug.LogWarning("No mines with miners.");
                        return;
                    }
                    Node<Vector2> target = GameManager.MinesWithMiners[0];
                    if (target == null) return;
                    TargetNode = Graph<Node<Vector2>, NodeVoronoi, Vector2>.NodesType.Find(node => node.GetCoordinate() == target.GetCoordinate());
                    if (TargetNode == null) return;
                    Debug.Log("Walk to " + TargetNode.GetCoordinate().x + " - " + TargetNode.GetCoordinate().y);
                });
            Fsm.SetTransition(Behaviours.Walk, Flags.OnGather, Behaviours.Deliver, () => Debug.Log("Deliver food"));
            Fsm.SetTransition(Behaviours.Walk, Flags.OnWait, Behaviours.GatherResources, () => Debug.Log("Deliver food"));
        }

        /// <summary>
        /// Retrieves parameters for the food gathering FSM tick.
        /// </summary>
        /// <returns>An array containing the current food state and gathering action.</returns>
        protected override object[] GetFoodTickParameters()
        {
            return new object[] { Food, FoodLimit, onGather, Retreat };
        }

        /// <summary>
        /// Retrieves parameters for the gathering FSM tick.
        /// </summary>
        /// <returns>An array containing parameters related to gathering.</returns>
        protected override object[] GatherTickParameters()
        {
            return new object[] { Retreat, Food, CurrentGold, GoldLimit, onGather };
        }

        /// <summary>
        /// Configures FSM transitions related to delivering food.
        /// </summary>
        protected override void DeliverTransitions()
        {
            Fsm.SetTransition(Behaviours.Deliver, Flags.OnHunger, Behaviours.Walk,
                () =>
                {
                    TargetNode = TownCenter;
                    if (TargetNode == null) return;
                    Debug.Log("To town center");
                });
            Fsm.SetTransition(Behaviours.Deliver, Flags.OnRetreat, Behaviours.Walk,
                () =>
                {
                    TargetNode = TownCenter;
                    if (TargetNode == null) return;
                    Debug.Log("Retreat. Walk to " + TargetNode.GetCoordinate().x + " - " + TargetNode.GetCoordinate().y);
                });
        }

        /// <summary>
        /// Retrieves parameters for the delivery FSM tick.
        /// </summary>
        /// <returns>An array containing parameters for food delivery.</returns>
        private object[] DeliverTickParameters()
        {
            return new object[] { Food, onDeliver, Retreat };
        }
    }
}