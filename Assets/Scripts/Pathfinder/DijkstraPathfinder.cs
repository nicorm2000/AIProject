using System;
using System.Collections.Generic;
using StateMachine.Agents.RTS;
using UnityEngine;

namespace Pathfinder
{
    /// <summary>
    /// Implements Dijkstra's algorithm for pathfinding on a graph.
    /// </summary>
    /// <typeparam name="NodeType">The type of the nodes in the graph.</typeparam>
    /// <typeparam name="TCoordinateType">The type of the coordinates used by the nodes.</typeparam>
    public class DijkstraPathfinder<NodeType, TCoordinateType, TCoordinate> : Pathfinder<NodeType, TCoordinateType, TCoordinate>
        where NodeType : INode, INode<TCoordinateType>, new()
        where TCoordinateType : IEquatable<TCoordinateType>
        where TCoordinate : ICoordinate<TCoordinateType>, new()
    {
        /// <summary>
        /// Initializes a new instance of the DijkstraPathfinder class.
        /// </summary>
        /// <param name="graph">The collection of nodes representing the graph.</param>
        public DijkstraPathfinder(ICollection<NodeType> graph)
        {
            this.Graph = graph;
        }

        /// <summary>
        /// Calculates the distance between two nodes.
        /// This implementation uses Manhattan distance.
        /// </summary>
        /// <param name="A">The first node.</param>
        /// <param name="B">The second node.</param>
        /// <returns>The calculated distance between the nodes.</returns>
        protected override int Distance(TCoordinate A, TCoordinate B)
        {
            float distance = 0;
            Node<Vector2> nodeA = A as Node<Vector2>;
            Node<Vector2> nodeB = B as Node<Vector2>;

            distance += MathF.Abs(nodeA.GetCoordinate().x - nodeB.GetCoordinate().x);
            distance += MathF.Abs(nodeA.GetCoordinate().y - nodeB.GetCoordinate().y);

            return (int)distance;
        }

        /// <summary>
        /// Retrieves the neighbors of a given node.
        /// </summary>
        /// <param name="node">The node for which to get neighbors.</param>
        /// <returns>A collection of neighboring nodes.</returns>
        protected override ICollection<INode<TCoordinateType>> GetNeighbors(NodeType node)
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
            return node.IsBlocked();
        }

        /// <summary>
        /// Calculates the cost of moving from one node to a neighbor node.
        /// This implementation assumes a cost of zero for simplicity.
        /// </summary>
        /// <param name="A">The current node.</param>
        /// <param name="B">The neighboring node.</param>
        /// <returns>The cost of moving to the neighbor node.</returns>
        protected override int MoveToNeighborCost(NodeType A, NodeType B, RTSAgent.AgentTypes type)
        {
            return 0;
        }

        /// <summary>
        /// Checks if two nodes are equal.
        /// </summary>
        /// <param name="A">The first node.</param>
        /// <param name="B">The second node.</param>
        /// <returns>True if the nodes are equal; otherwise, false.</returns>
        protected override bool NodesEquals(NodeType A, NodeType B)
        {
            return Equals(A, B);
        }
    }
}