﻿using System;
using System.Collections.Generic;
using Pathfinder;
using StateMachine.Agents.RTS;
using States;
using UnityEngine;
using Vector2 = Utils.Vec2Int;

namespace StateMachine.States.RTSStates
{
    public class WalkState : State
    {
        private refInt i = new refInt(0);

        public override BehaviourActions GetTickBehaviour(params object[] parameters)
        {
            BehaviourActions behaviours = new BehaviourActions();

            Node<Vector2> currentNode = (Node<Vector2>)parameters[0];
            Node<Vector2> targetNode = (Node<Vector2>)parameters[1];
            float speed = Convert.ToSingle(parameters[2]);
            bool retreat = (bool)parameters[3];
            Transform position = (Transform)parameters[4];
            List<Node<Vector2>> path = (List<Node<Vector2>>)parameters[5];
            Pathfinder<Node<Vector2>> pathfinder = parameters[6] as Pathfinder<Node<Vector2>>;


            behaviours.AddMainThreadBehaviours(0, () =>
            {
                if (currentNode == null || targetNode == null || pathfinder == null)
                {
                    Debug.LogError("One or more required parameters are null.");
                    return;
                }

                if (currentNode.Equals(targetNode)) return;

                if ((path is null || i.value >= path.Count) && !currentNode.Equals(targetNode))
                {
                    path = pathfinder.FindPath(currentNode, targetNode);
                }

                if (path.Count <= 0 || i.value >= path.Count) return;

                currentNode = path[i.value];
                i.value++;
                position.position = new Vector3(currentNode.GetCoordinate().x, currentNode.GetCoordinate().y);
            });

            behaviours.SetTransitionBehaviour(() =>
            {
                if (retreat && targetNode.NodeType != NodeType.TownCenter)
                {
                    OnFlag?.Invoke(RTSAgent.Flags.OnRetreat);
                    i.value = 0;
                    return;
                }


                if (currentNode == null || targetNode == null || pathfinder == null ||
                    targetNode.NodeType == NodeType.Mine && targetNode.gold <= 0)
                {
                    OnFlag?.Invoke(RTSAgent.Flags.OnTargetLost);
                    i.value = 0;
                    return;
                }

                if (!currentNode.Equals(targetNode)) return;

                switch (currentNode.NodeType)
                {
                    case NodeType.Mine:
                        OnFlag?.Invoke(RTSAgent.Flags.OnGather);
                        break;
                    case NodeType.TownCenter:
                        OnFlag?.Invoke(RTSAgent.Flags.OnWait);
                        break;
                    case NodeType.Empty:
                    case NodeType.Blocked:
                    default:
                        throw new ArgumentOutOfRangeException();
                }

                i.value = 0;
            });

            return behaviours;
        }

        public override BehaviourActions GetOnEnterBehaviour(params object[] parameters)
        {
            return default;
        }

        public override BehaviourActions GetOnExitBehaviour(params object[] parameters)
        {
            return default;
        }
    }
}