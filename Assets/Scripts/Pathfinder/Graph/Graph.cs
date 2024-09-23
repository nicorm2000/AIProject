using System;
using System.Collections.Generic;

namespace Pathfinder.Graph
{
    /// <summary>
    /// Abstract base class representing a graph structure composed of nodes and coordinates. 
    /// Handles creation of the graph and adds neighbors to each node.
    /// </summary>
    /// <typeparam name="TNodeType">The type of the node, which implements <see cref="INode{TCoordinateType}"/>.</typeparam>
    /// <typeparam name="TCoordinateNode">The type of the coordinate node, which implements <see cref="ICoordinate{TCoordinateType}"/>.</typeparam>
    /// <typeparam name="TCoordinateType">The type of the coordinate (e.g., Vector2, Vector3).</typeparam>
    public abstract class Graph<TNodeType, TCoordinateNode, TCoordinateType>
        where TNodeType : INode<TCoordinateType>, new()
        where TCoordinateNode : ICoordinate<TCoordinateType>, new()
        where TCoordinateType : IEquatable<TCoordinateType>, new()
    {
        /// <summary>
        /// A list storing the nodes based on coordinates.
        /// </summary>
        public readonly List<TCoordinateNode> CoordNodes = new List<TCoordinateNode>();

        /// <summary>
        /// A list storing the typed nodes of the graph.
        /// </summary>
        public readonly List<TNodeType> NodesType = new List<TNodeType>();

        /// <summary>
        /// Constructor for initializing the graph and its neighbor relationships.
        /// </summary>
        /// <param name="x">Number of cells along the x-axis.</param>
        /// <param name="y">Number of cells along the y-axis.</param>
        /// <param name="cellSize">Size of each cell in the grid.</param>
        public Graph(int x, int y, float cellSize)
        {
            CreateGraph(x, y, cellSize);
            AddNeighbors(cellSize);
        }

        /// <summary>
        /// Abstract method to be implemented by subclasses to create the graph structure.
        /// </summary>
        /// <param name="x">Number of cells along the x-axis.</param>
        /// <param name="y">Number of cells along the y-axis.</param>
        /// <param name="cellSize">Size of each cell in the grid.</param>
        public abstract void CreateGraph(int x, int y, float cellSize);

        /// <summary>
        /// Adds neighboring nodes based on proximity (within one cell size).
        /// </summary>
        /// <param name="cellSize">The size of each cell, used to determine adjacency.</param>
        private void AddNeighbors(float cellSize)
        {
            List<INode<TCoordinateType>> neighbors = new List<INode<TCoordinateType>>();

            for (int i = 0; i < CoordNodes.Count; i++)
            {
                neighbors.Clear();

                for (int j = 0; j < CoordNodes.Count; j++)
                {
                    if (i == j) continue; // Skip the comparison with the same node

                    // Check if the current node (i) and another node (j) are adjacent based on cell size
                    // Conditions include checking x-axis, y-axis, and diagonal adjacency
                    if ((Approximately(CoordNodes[i].GetX(), CoordNodes[j].GetX()) &&
                         Approximately(Math.Abs(CoordNodes[i].GetY() - CoordNodes[j].GetY()), cellSize)) ||
                        (Approximately(CoordNodes[i].GetY(), CoordNodes[j].GetY()) &&
                         Approximately(Math.Abs(CoordNodes[i].GetX() - CoordNodes[j].GetX()), cellSize)) ||
                        (Approximately(Math.Abs(CoordNodes[i].GetY() - CoordNodes[j].GetY()), cellSize) &&
                         Approximately(Math.Abs(CoordNodes[i].GetX() - CoordNodes[j].GetX()), cellSize)))
                    {
                        neighbors.Add(NodesType[j]);
                    }
                }

                NodesType[i].SetNeighbors(neighbors);
            }
        }

        /// <summary>
        /// Checks if two floating-point values are approximately equal within a small tolerance.
        /// </summary>
        /// <param name="a">The first float value.</param>
        /// <param name="b">The second float value.</param>
        /// <returns>True if the values are approximately equal, otherwise false.</returns>
        public bool Approximately(float a, float b)
        {
            return Math.Abs(a - b) < 1e-6f;
        }
    }
}