using UnityEngine;

namespace Pathfinder.Graph
{
    /// <summary>
    /// Represents a graph specifically for handling 2D Vector data. Inherits from the generic Graph class.
    /// </summary>
    public class Vector2Graph : Graph<Node<Vector2>, NodeVoronoi, Vector2>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Vector2Graph"/> class, setting up the graph dimensions and cell size.
        /// </summary>
        /// <param name="x">Number of cells along the x-axis.</param>
        /// <param name="y">Number of cells along the y-axis.</param>
        /// <param name="cellSize">The size of each cell in the graph.</param>
        public Vector2Graph(int x, int y, float cellSize) : base(x, y, cellSize)
        {
        }

        /// <summary>
        /// Creates the graph by generating nodes based on the grid dimensions and cell size.
        /// </summary>
        /// <param name="x">Number of cells along the x-axis.</param>
        /// <param name="y">Number of cells along the y-axis.</param>
        /// <param name="cellSize">The size of each cell in the graph.</param>
        public override void CreateGraph(int x, int y, float cellSize)
        {
            for (int i = 0; i < x; i++)
            {
                for (int j = 0; j < y; j++)
                {
                    // Create a new NodeVoronoi and set its coordinates based on the grid position
                    NodeVoronoi node = new NodeVoronoi();
                    node.SetCoordinate(i * cellSize, j * cellSize);
                    CoordNodes.Add(node); // Add the node to the coordinate-based node list

                    // Create a generic node of type Vector2 and set its coordinate in Unity's Vector2 format
                    Node<Vector2> nodeType = new Node<Vector2>();
                    nodeType.SetCoordinate(new Vector2(i * cellSize, j * cellSize));
                    NodesType.Add(nodeType); // Add the node to the NodesType list
                }
            }
        }
    }
}