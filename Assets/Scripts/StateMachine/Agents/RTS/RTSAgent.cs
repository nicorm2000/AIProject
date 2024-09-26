using System;
using System.Collections.Generic;
using Game;
using Pathfinder;
using Pathfinder.Graph;
using Pathfinder.Voronoi;
using StateMachine.States.RTSStates;
using UnityEngine;

namespace StateMachine.Agents.RTS
{
    /// <summary>
    /// Represents an agent.
    /// </summary>
    public class RTSAgent : MonoBehaviour
    {
        public enum Flags
        {
            OnTargetReach,
            OnTargetLost,
            OnHunger,
            OnRetreat,
            OnFull,
            OnGather,
            OnWait
        }

        public enum Behaviours
        {
            Wait,
            Walk,
            GatherResources,
            Deliver
        }

        public enum AgentTypes
        {
            Miner,
            Caravan
        }

        public static Node<Vector2> TownCenter;
        public static bool Retreat;
        public Node<Vector2> CurrentNode;
        public Voronoi<NodeVoronoi, Vector2> Voronoi;

        protected FSM<Behaviours, Flags> Fsm;
        protected AStarPathfinder<Node<Vector2>, Vector2, NodeVoronoi> Pathfinder;
        protected List<Node<Vector2>> Path;
        protected AgentTypes AgentType;
        protected Node<Vector2> TargetNode
        {
            get => targetNode;
            set
            {
                targetNode = value;
                Path = Pathfinder.FindPath(CurrentNode, TargetNode, AgentType);
                PathNodeId = 0;
            }
        }

        protected Action OnMove;
        protected Action OnWait;
        protected int Food = 3;
        protected int CurrentGold = 0;
        protected int LastTimeEat = 0;
        protected const int GoldPerFood = 3;
        protected const int GoldLimit = 15;
        protected const int FoodLimit = 10;
        protected int PathNodeId;

        private Node<Vector2> targetNode;

        /// <summary>
        /// Unity's Update method, called every frame to tick the FSM.
        /// </summary>
        private void Update()
        {
            Fsm.Tick();
        }

        /// <summary>
        /// Initializes the agent, setting up FSM, pathfinding, and delegates.
        /// </summary>
        public virtual void Init()
        {
            Fsm = new FSM<Behaviours, Flags>();
            Pathfinder = GameManager.Pathfinder;

            OnMove += Move;
            OnWait += Wait;

            FsmBehaviours();
            FsmTransitions();
        }

        /// <summary>
        /// Sets up FSM transitions between different behaviors.
        /// </summary>
        protected virtual void FsmTransitions()
        {
            WalkTransitions();
            WaitTransitions();
            GatherTransitions();
            GetFoodTransitions();
            DeliverTransitions();
        }

        /// <summary>
        /// Defines behaviors for the FSM such as wait and walk.
        /// </summary>
        protected virtual void FsmBehaviours()
        {
            Fsm.AddBehaviour<WaitState>(Behaviours.Wait, WaitTickParameters, WaitEnterParameters, WaitExitParameters);
            Fsm.AddBehaviour<WalkState>(Behaviours.Walk, WalkTickParameters, WalkEnterParameters);
        }

        /// <summary>
        /// Sets up transitions for gathering resources.
        /// </summary>
        protected virtual void GatherTransitions()
        {
            Fsm.SetTransition(Behaviours.GatherResources, Flags.OnRetreat, Behaviours.Walk,
                () =>
                {
                    TargetNode = GetTarget(NodeType.TownCenter);
                    if (TargetNode == null) return;
                    Debug.Log("Retreat to " + TargetNode.GetCoordinate().x + " - " + TargetNode.GetCoordinate().y);
                });
        }

        /// <summary>
        /// Parameters for the GatherResources state tick.
        /// </summary>
        protected virtual object[] GatherTickParameters()
        {
            object[] objects = { Retreat, Food, CurrentGold, GoldLimit };
            return objects;
        }

        /// <summary>
        /// Sets up transitions for the Walk state.
        /// </summary>
        protected virtual void WalkTransitions()
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
                    TargetNode = GetTarget(NodeType.Mine);
                    if (TargetNode == null) return;
                    Debug.Log("Walk to " + TargetNode.GetCoordinate().x + " - " + TargetNode.GetCoordinate().y);
                });
            Fsm.SetTransition(Behaviours.Walk, Flags.OnWait, Behaviours.Wait, () => Debug.Log("Wait"));
        }

        /// <summary>
        /// Parameters for the Walk state tick.
        /// </summary>
        protected virtual object[] WalkTickParameters()
        {
            object[] objects = { CurrentNode, TargetNode, Retreat, transform, OnMove };
            return objects;
        }

        /// <summary>
        /// Parameters for entering the Walk state.
        /// </summary>
        protected virtual object[] WalkEnterParameters()
        {
            object[] objects = { CurrentNode, TargetNode, Path, Pathfinder, AgentType };
            return objects;
        }

        /// <summary>
        /// Sets up transitions for the Wait state.
        /// </summary>
        protected virtual void WaitTransitions()
        {
            Fsm.SetTransition(Behaviours.Wait, Flags.OnGather, Behaviours.Walk,
                () =>
                {
                    TargetNode = GetTarget(NodeType.Mine);
                    if (TargetNode == null) return;
                    Debug.Log("walk to " + TargetNode.GetCoordinate().x + " - " + TargetNode.GetCoordinate().y);
                });
            Fsm.SetTransition(Behaviours.Wait, Units.Flags.OnTargetLost, Behaviours.Walk,
                () =>
                {
                    TargetNode = GetTarget(NodeType.Mine);
                    if (TargetNode == null) return;
                    Debug.Log("walk to " + TargetNode.GetCoordinate().x + " - " + TargetNode.GetCoordinate().y);
                });
            Fsm.SetTransition(Behaviours.Wait, Flags.OnRetreat, Behaviours.Walk,
                () =>
                {
                    TargetNode = GetTarget(NodeType.TownCenter);
                    if (TargetNode == null) return;
                    Debug.Log("Retreat. Walk to " + TargetNode.GetCoordinate().x + " - " + TargetNode.GetCoordinate().y);
                });
        }

        /// <summary>
        /// Parameters for the Wait state tick.
        /// </summary>
        protected virtual object[] WaitTickParameters()
        {
            object[] objects = { Retreat, Food, CurrentGold, CurrentNode, OnWait };
            return objects;
        }

        /// <summary>
        /// Parameters for entering the Wait state.
        /// </summary>
        protected virtual object[] WaitEnterParameters()
        {
            return null;
        }

        /// <summary>
        /// Parameters for exiting the Wait state.
        /// </summary>
        protected virtual object[] WaitExitParameters()
        {
            return null;
        }

        /// <summary>
        /// Sets up transitions for acquiring food.
        /// </summary>
        protected virtual void GetFoodTransitions()
        {
            return;
        }

        /// <summary>
        /// Parameters for the GetFood state tick.
        /// </summary>
        protected virtual object[] GetFoodTickParameters()
        {
            object[] objects = { Food, FoodLimit };
            return objects;
        }

        /// <summary>
        /// Configures FSM transitions related to delivering food.
        /// </summary>
        protected virtual void DeliverTransitions()
        {
            return;
        }

        /// <summary>
        /// Method to move the agent towards the target node.
        /// </summary>
        protected virtual void Move()
        {
            if (CurrentNode == null || TargetNode == null) return;
            if (CurrentNode.Equals(TargetNode)) return;
            if (Path.Count <= 0) return;
            if (PathNodeId > Path.Count) PathNodeId = 0;

            CurrentNode = Path[PathNodeId];
            PathNodeId++;
        }

        /// <summary>
        /// Method to execute when the agent is waiting.
        /// </summary>
        private void Wait()
        {
            if (CurrentNode.NodeType == NodeType.Mine && CurrentNode.food > 0)
            {
                if (Food > 1) return;
                Food++;
                CurrentNode.food--;
            }

            if (CurrentNode.NodeType != NodeType.TownCenter || CurrentGold < 1) return;
            CurrentNode.gold++;
            CurrentGold--;
        }

        /// <summary>
        /// Method to retrieve the target node based on its type.
        /// </summary>
        /// <param name="type">The type of node to get.</param>
        /// <returns>The target node, or null if not found.</returns>
        protected Node<Vector2> GetTarget(NodeType nodeType = NodeType.Mine)
        {
            Vector2 position = transform.position;
            Node<Vector2> target;

            switch (nodeType)
            {
                case NodeType.Mine:
                    target = Voronoi.GetMineCloser(GameManager.Graph.CoordNodes.Find(nodeVoronoi => nodeVoronoi.GetCoordinate() == position));
                    break;
                case NodeType.TownCenter:
                    target = TownCenter;
                    break;
                default:
                    target = Voronoi.GetMineCloser(GameManager.Graph.CoordNodes.Find(nodeVoronoi => nodeVoronoi.GetCoordinate() == position));
                    break;
            }

            if (target == null)
            {
                Debug.LogError("No mines with gold.");
                return null;
            }

            return Graph<Node<Vector2>, NodeVoronoi, Vector2>.NodesType.Find(node => node.GetCoordinate() == target.GetCoordinate());
        }
    }
}