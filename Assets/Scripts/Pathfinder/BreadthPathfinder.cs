using System;
using System.Collections.Generic;
using Utils;

namespace Pathfinder
{
    /// <summary>
    /// Implements a Breadth-First Search (BFS) algorithm for pathfinding on a graph.
    /// </summary>
    /// <typeparam name="NodeType">The type of the nodes in the graph.</typeparam>
    /// <typeparam name="TCoordinateType">The type of the coordinates used by the nodes.</typeparam>
    public class BreadthPathfinder<NodeType, TCoordinateType> : Pathfinder<NodeType, TCoordinateType>
        where NodeType : INode<TCoordinateType>, new()
        where TCoordinateType : IEquatable<TCoordinateType>
    {
        /// <summary>
        /// Initializes a new instance of the BreadthPathfinder class.
        /// </summary>
        /// <param name="graph">The collection of nodes representing the graph.</param>
        public BreadthPathfinder(ICollection<NodeType> graph)
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
        protected override int Distance(NodeType A, NodeType B)
        {
            int distance = 0;
            Node<Vec2Int> nodeA = A as Node<Vec2Int>;
            Node<Vec2Int> nodeB = B as Node<Vec2Int>;

            distance += Math.Abs(nodeA.GetCoordinate().x - nodeB.GetCoordinate().x);
            distance += Math.Abs(nodeA.GetCoordinate().y - nodeB.GetCoordinate().y);

            return distance;
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
            return false;
        }

        /// <summary>
        /// Calculates the cost of moving from one node to a neighbor node.
        /// This implementation assumes a cost of zero for simplicity.
        /// </summary>
        /// <param name="A">The current node.</param>
        /// <param name="B">The neighboring node.</param>
        /// <returns>The cost of moving to the neighbor node.</returns>
        protected override int MoveToNeighborCost(NodeType A, NodeType B)
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