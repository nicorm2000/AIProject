﻿using System;
using System.Collections.Generic;
using Pathfinder;
using StateMachine.Agents.RTS;
using States;
using UnityEngine;

namespace StateMachine.States.RTSStates
{
    public class WalkState : State
    {
        /// <summary>
        /// Gets the tick behaviour for the walk state, updating the agent's position and checking for transitions based on node types.
        /// </summary>
        /// <param name="parameters">Parameters for the behaviour: current node, target node, retreat flag, position transform, and move action.</param>
        /// <returns>A BehaviourActions object containing the behaviours to execute.</returns>
        public override BehaviourActions GetTickBehaviour(params object[] parameters)
        {
            BehaviourActions behaviours = new BehaviourActions();

            Node<Vector2> currentNode = parameters[0] as Node<Vector2>;
            Node<Vector2> targetNode = parameters[1] as Node<Vector2>;
            bool retreat = (bool)parameters[2];
            Transform position = (Transform)parameters[3];
            Action onMove = parameters[4] as Action;

            behaviours.AddMultiThreadableBehaviours(0, () =>
            {
                onMove?.Invoke();
            });

            behaviours.AddMainThreadBehaviours(1, () =>
            {
                if (currentNode == null || targetNode == null)
                {
                    return;
                }

                position.position = new Vector3(currentNode.GetCoordinate().x, currentNode.GetCoordinate().y);
            });

            behaviours.SetTransitionBehaviour(() =>
            {
                if (retreat && (targetNode is null || targetNode.NodeType != NodeType.TownCenter))
                {
                    OnFlag?.Invoke(RTSAgent.Flags.OnRetreat);
                    return;
                }

                if (currentNode == null || targetNode == null || (targetNode.NodeType == NodeType.Mine && targetNode.gold <= 0))
                {
                    OnFlag?.Invoke(RTSAgent.Flags.OnTargetLost);
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
                        OnFlag?.Invoke(RTSAgent.Flags.OnTargetLost);
                        break;
                    case NodeType.Forest:
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            });

            return behaviours;
        }

        /// <summary>
        /// Gets the on enter behaviour for the walk state, finding a path from the current node to the target node.
        /// </summary>
        /// <param name="parameters">Parameters for the behaviour: current node, target node, path list, pathfinder, and agent type.</param>
        /// <returns>A BehaviourActions object containing the behaviours to execute.</returns>
        public override BehaviourActions GetOnEnterBehaviour(params object[] parameters)
        {
            BehaviourActions behaviours = new BehaviourActions();

            Node<Vector2> currentNode = parameters[0] as Node<Vector2>;
            Node<Vector2> targetNode = parameters[1] as Node<Vector2>;
            List<Node<Vector2>> path = (List<Node<Vector2>>)parameters[2];
            Pathfinder<Node<Vector2>, Vector2, NodeVoronoi> pathfinder = parameters[3] as Pathfinder<Node<Vector2>, Vector2, NodeVoronoi>;
            RTSAgent.AgentTypes type = (RTSAgent.AgentTypes)parameters[4];

            behaviours.AddMultiThreadableBehaviours(0, () =>
            {
                if (currentNode != null && targetNode != null)
                    path = pathfinder.FindPath(currentNode, targetNode, type);
            });

            return behaviours;
        }

        /// <summary>
        /// Gets the on exit behaviour for the walk state. Currently does not implement any specific exit behaviour.
        /// </summary>
        /// <param name="parameters">Parameters for the behaviour (currently unused).</param>
        /// <returns>A default BehaviourActions object.</returns>
        public override BehaviourActions GetOnExitBehaviour(params object[] parameters)
        {
            return default;
        }
    }
}