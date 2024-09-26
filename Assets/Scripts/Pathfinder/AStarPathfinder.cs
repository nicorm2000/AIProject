using System;
using System.Collections.Generic;
using StateMachine.Agents.RTS;

namespace Pathfinder
{
    /// <summary>
    /// Implements the A* pathfinding algorithm.
    /// </summary>
    /// <typeparam name="NodeType">The type of the nodes in the graph.</typeparam>
    /// <typeparam name="CoordinateType">The type of the coordinates used by the nodes.</typeparam>
    /// <typeparam name="TCoordinate">The type of the coordinates.</typeparam>
    public class AStarPathfinder<NodeType, CoordinateType, TCoordinate> : Pathfinder<NodeType, CoordinateType, TCoordinate>
           where NodeType : INode, INode<CoordinateType>, new()
           where CoordinateType : IEquatable<CoordinateType>
           where TCoordinate : ICoordinate<CoordinateType>, new()
    {
        /// <summary>
        /// Initializes a new instance of the AStarPathfinder class.
        /// </summary>
        /// <param name="graph">The collection of nodes representing the graph.</param>
        /// <param name="minCost">Minimum cost for a transition.</param>
        /// <param name="maxCost">Maximum cost for a transition.</param>
        public AStarPathfinder(ICollection<NodeType> graph)
        {
            this.Graph = graph;
        }

        /// <summary>
        /// Calculates the distance between two nodes using Manhattan distance.
        /// </summary>
        /// <param name="A">The first node.</param>
        /// <param name="B">The second node.</param>
        /// <returns>The calculated distance between the nodes.</returns>
        protected override int Distance(TCoordinate A, TCoordinate B)
        {
            if (A == null || B == null)
            {
                return int.MaxValue;
            }

            float distance = 0;

            distance += Math.Abs(A.GetX() - B.GetX());
            distance += Math.Abs(A.GetY() - B.GetY());

            return (int)distance;
        }

        /// <summary>
        /// Retrieves the neighbors of a given node.
        /// </summary>
        /// <param name="node">The node for which to get neighbors.</param>
        /// <returns>A collection of neighboring nodes.</returns>
        protected override ICollection<INode<CoordinateType>> GetNeighbors(NodeType node)
        {
            return node.GetNeighbors();
        }

        /// <summary>
        /// Checks whether the specified node is blocked.
        /// </summary>
        /// <param name="node">The node to check.</param>
        /// <returns>True if the node is blocked; otherwise, false.</returns>
        protected override bool IsBlocked(NodeType node)
        {
            return node.GetNodeType() == Pathfinder.NodeType.Forest;
        }

        /// <summary>
        /// Calculates the cost of moving from one node to a neighbor node.
        /// </summary>
        /// <param name="A">The current node.</param>
        /// <param name="B">The neighboring node.</param>
        /// <returns>The cost of moving to the neighbor node.</returns>
        /// <exception cref="InvalidOperationException">Thrown if B is not a neighbor of A.</exception>
        protected override int MoveToNeighborCost(NodeType A, NodeType B, RTSAgent.AgentTypes type)
        {
            if (!GetNeighbors(A).Contains(B))
            {
                throw new InvalidOperationException("B node has to be a neighbor.");
            }

            int cost = 0;

            switch (type)
            {
                case RTSAgent.AgentTypes.Miner:
                    if (B.GetNodeType() == Pathfinder.NodeType.Dirt) cost += 2;
                    break;
                case RTSAgent.AgentTypes.Caravan:
                    if (B.GetNodeType() == Pathfinder.NodeType.TreeCutDown) cost += 1;
                    break;
                default:
                    cost = 0;
                    break;
            }

            return cost;
        }

        /// <summary>
        /// Checks if two nodes are approximately equal based on their coordinates.
        /// </summary>
        /// <param name="A">The first node.</param>
        /// <param name="B">The second node.</param>
        /// <returns>True if the nodes are approximately equal; otherwise, false.</returns>
        protected override bool NodesEquals(NodeType A, NodeType B)
        {
            if (A == null || B == null)
            {
                return false;
            }

            return A.Equals(B);
        }
    }
}