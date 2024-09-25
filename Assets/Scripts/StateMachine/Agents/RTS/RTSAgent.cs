﻿using System;
using System.Collections.Generic;
using Game;
using Pathfinder;
using Pathfinder.Graph;
using Pathfinder.Voronoi;
using StateMachine.States.RTSStates;
using UnityEngine;

namespace StateMachine.Agents.RTS
{
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

        public static Node<Vector2> TownCenter;
        public static bool Retreat;
        public Node<Vector2> CurrentNode;
        public Voronoi<NodeVoronoi, Vector2> Voronoi;

        protected FSM<Behaviours, Flags> Fsm;
        protected AStarPathfinder<Node<Vector2>, Vector2, NodeVoronoi> Pathfinder;
        protected List<Node<Vector2>> Path;

        protected Node<Vector2> TargetNode
        {
            get => targetNode;
            set
            {
                targetNode = value;
                Path = Pathfinder.FindPath(CurrentNode, TargetNode);
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

        private void Update()
        {
            Fsm.Tick();
        }

        public virtual void Init()
        {
            Fsm = new FSM<Behaviours, Flags>();

            Pathfinder = new AStarPathfinder<Node<Vector2>, Vector2, NodeVoronoi>(
                Graph<Node<Vector2>, NodeVoronoi, Vector2>.NodesType, 0, 0);

            OnMove += Move;
            OnWait += Wait;

            FsmBehaviours();

            FsmTransitions();
        }

        protected virtual void FsmTransitions()
        {
            WalkTransitions();
            WaitTransitions();
            GatherTransitions();
            GetFoodTransitions();
            DeliverTransitions();
        }

        protected virtual void FsmBehaviours()
        {
            Fsm.AddBehaviour<WaitState>(Behaviours.Wait, WaitTickParameters);
            Fsm.AddBehaviour<WalkState>(Behaviours.Walk, WalkTickParameters, WalkEnterParameters);
        }

        protected virtual void GatherTransitions()
        {
            Fsm.SetTransition(Behaviours.GatherResources, Flags.OnRetreat, Behaviours.Walk,
                () =>
                {
                    TargetNode = GetTarget(NodeType.TownCenter);

                    Debug.Log("Retreat to " + TargetNode.GetCoordinate().x + " - " + TargetNode.GetCoordinate().y);
                });
        }


        protected virtual object[] GatherTickParameters()
        {
            object[] objects = { Retreat, Food, CurrentGold, GoldLimit };
            return objects;
        }


        protected virtual void WalkTransitions()
        {
            Fsm.SetTransition(Behaviours.Walk, Flags.OnRetreat, Behaviours.Walk,
                () =>
                {
                    TargetNode = GetTarget(NodeType.TownCenter);

                    Debug.Log("Retreat. Walk to " + TargetNode.GetCoordinate().x + " - " +
                              TargetNode.GetCoordinate().y);
                });

            Fsm.SetTransition(Behaviours.Walk, Flags.OnTargetLost, Behaviours.Walk,
                () =>
                {
                    TargetNode = GetTarget(NodeType.Mine);

                    Debug.Log("Walk to " + TargetNode.GetCoordinate().x + " - " + TargetNode.GetCoordinate().y);
                });

            Fsm.SetTransition(Behaviours.Walk, Flags.OnWait, Behaviours.Wait, () => Debug.Log("Wait"));
        }

        protected virtual object[] WalkTickParameters()
        {
            object[] objects = { CurrentNode, TargetNode, Retreat, transform, OnMove };
            return objects;
        }

        protected virtual object[] WalkEnterParameters()
        {
            object[] objects = { CurrentNode, TargetNode, Path, Pathfinder };
            return objects;
        }

        protected virtual void WaitTransitions()
        {
            Fsm.SetTransition(Behaviours.Wait, Flags.OnGather, Behaviours.Walk,
                () =>
                {
                    TargetNode = GetTarget(NodeType.Mine);

                    Debug.Log("walk to " + TargetNode.GetCoordinate().x + " - " + TargetNode.GetCoordinate().y);
                });
            Fsm.SetTransition(Behaviours.Wait, Units.Flags.OnTargetLost, Behaviours.Walk,
                () =>
                {
                    TargetNode = GetTarget(NodeType.Mine); ;

                    Debug.Log("walk to " + TargetNode.GetCoordinate().x + " - " + TargetNode.GetCoordinate().y);
                });
        }

        protected virtual object[] WaitTickParameters()
        {
            object[] objects = { Retreat, Food, CurrentGold, CurrentNode, OnWait };
            return objects;
        }

        protected virtual void GetFoodTransitions()
        {
            return;
        }

        protected virtual object[] GetFoodTickParameters()
        {
            object[] objects = { Food, FoodLimit };
            return objects;
        }

        protected virtual void DeliverTransitions()
        {
            return;
        }

        protected virtual void Move()
        {
            if (CurrentNode == null || TargetNode == null)
            {
                return;
            }

            if (CurrentNode.Equals(TargetNode)) return;

            if (Path.Count <= 0) return;
            if (PathNodeId > Path.Count) PathNodeId = 0;

            CurrentNode = Path[PathNodeId];
            PathNodeId++;
        }

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

        protected Node<Vector2> GetTarget(NodeType nodeType = NodeType.Mine)
        {
            Vector2 position = transform.position;
            Node<Vector2> target;

            switch (nodeType)
            {
                case NodeType.Mine:
                    target = Voronoi.GetMineCloser(GameManager.Graph.CoordNodes.Find(nodeVoronoi =>
                        nodeVoronoi.GetCoordinate() == position));
                    break;

                case NodeType.TownCenter:
                    target = TownCenter;
                    break;

                default:
                    target = Voronoi.GetMineCloser(GameManager.Graph.CoordNodes.Find(nodeVoronoi =>
                        nodeVoronoi.GetCoordinate() == position));
                    break;
            }

            if (target == null)
            {
                Debug.LogError("No mines with gold.");
                return null;
            }

            return Graph<Node<Vector2>, NodeVoronoi, Vector2>.NodesType.Find(node =>
                node.GetCoordinate() == target.GetCoordinate());
        }
    }
}