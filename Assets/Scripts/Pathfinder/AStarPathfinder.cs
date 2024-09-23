﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using Vector2 = Utils.Vec2Int;

namespace Pathfinder
{
    /// <summary>
    /// Implements the A* pathfinding algorithm.
    /// </summary>
    /// <typeparam name="NodeType">The type of the nodes in the graph.</typeparam>
    /// <typeparam name="CoordinateType">The type of the coordinates used by the nodes.</typeparam>
    public class AStarPathfinder<NodeType, CoordinateType> : Pathfinder<NodeType, CoordinateType>
        where NodeType : INode, INode<CoordinateType>, new()
        where CoordinateType : IEquatable<CoordinateType>
    {
        /// <summary>
        /// Initializes a new instance of the AStarPathfinder class.
        /// </summary>
        /// <param name="graph">The collection of nodes representing the graph.</param>
        /// <param name="minCost">Minimum cost for a transition.</param>
        /// <param name="maxCost">Maximum cost for a transition.</param>
        public AStarPathfinder(ICollection<NodeType> graph, int minCost = -1, int maxCost = 3)
        {
            this.Graph = graph;

            foreach (var node in graph)
            {
                List<Transition<NodeType>> transitionsList = new List<Transition<NodeType>>();

                var neighbors = GetNeighbors(node) as List<NodeType>;
                neighbors?.ForEach(neighbor =>
                {
                    transitionsList.Add(new Transition<NodeType>
                    {
                        to = neighbor,
                        cost = (minCost == 0 && maxCost == 0) ? 0 : RandomNumberGenerator.GetInt32(minCost, maxCost),
                    });
                });

                transitions.Add(node, transitionsList);
            }
        }

        /// <summary>
        /// Calculates the distance between two nodes using Manhattan distance.
        /// </summary>
        /// <param name="A">The first node.</param>
        /// <param name="B">The second node.</param>
        /// <returns>The calculated distance between the nodes.</returns>
        protected override int Distance(NodeType A, NodeType B)
        {
            if (A == null || B == null)
            {
                return int.MaxValue;
            }

            Node<Vector2> nodeA = A as Node<Vector2>;
            Node<Vector2> nodeB = B as Node<Vector2>;

            return (int)(Math.Abs(nodeA.GetCoordinate().x - nodeB.GetCoordinate().x) +
                          Math.Abs(nodeA.GetCoordinate().y - nodeB.GetCoordinate().y));
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
            return node.IsBlocked();
        }

        /// <summary>
        /// Calculates the cost of moving from one node to a neighbor node.
        /// </summary>
        /// <param name="A">The current node.</param>
        /// <param name="B">The neighboring node.</param>
        /// <returns>The cost of moving to the neighbor node.</returns>
        /// <exception cref="InvalidOperationException">Thrown if B is not a neighbor of A.</exception>
        protected override int MoveToNeighborCost(NodeType A, NodeType B)
        {
            if (!GetNeighbors(A).Contains(B))
            {
                throw new InvalidOperationException("B node has to be a neighbor.");
            }

            transitions.TryGetValue(A, out List<Transition<NodeType>> transition);

            return transition?.FirstOrDefault(t => NodesEquals(t.to, B)).cost ?? 0;
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

            var nodeA = A as Node<Vector2>;
            var nodeB = B as Node<Vector2>;

            return nodeA != null && nodeB != null &&
                   Approximately(nodeA.GetCoordinate().x, nodeB.GetCoordinate().x) &&
                   Approximately(nodeA.GetCoordinate().y, nodeB.GetCoordinate().y);
        }

        /// <summary>
        /// Compares two floating-point numbers for approximate equality.
        /// </summary>
        /// <param name="a">The first number.</param>
        /// <param name="b">The second number.</param>
        /// <returns>True if the numbers are approximately equal; otherwise, false.</returns>
        private bool Approximately(float a, float b)
        {
            return Math.Abs(a - b) < 1e-6f;
        }
    }
}