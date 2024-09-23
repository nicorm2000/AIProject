using System;
using System.Collections.Generic;

namespace Pathfinder
{
    /// <summary>
    /// Represents a node in the graph that can be blocked or accessible.
    /// </summary>
    public interface INode
    {
        /// <summary>
        /// Determines whether the node is blocked or not.
        /// </summary>
        /// <returns>True if the node is blocked; otherwise, false.</returns>
        public bool IsBlocked();
    }

    /// <summary>
    /// Represents a node with a coordinate type in the graph.
    /// </summary>
    /// <typeparam name="Coordinate">The type of the coordinate used by the node.</typeparam>
    public interface INode<Coordinate> : IEquatable<Coordinate>
        where Coordinate : IEquatable<Coordinate>
    {
        /// <summary>
        /// Sets the coordinate of the node.
        /// </summary>
        /// <param name="coordinateType">The coordinate to set for the node.</param>
        public void SetCoordinate(Coordinate coordinateType);

        /// <summary>
        /// Gets the coordinate of the node.
        /// </summary>
        /// <returns>The coordinate of the node.</returns>
        public Coordinate GetCoordinate();

        /// <summary>
        /// Sets the neighboring nodes for the current node.
        /// </summary>
        /// <param name="neighbors">A collection of neighboring nodes to set.</param>
        public void SetNeighbors(ICollection<INode<Coordinate>> neighbors);

        /// <summary>
        /// Gets the neighboring nodes of the current node.
        /// </summary>
        /// <returns>A collection of neighboring nodes.</returns>
        public ICollection<INode<Coordinate>> GetNeighbors();
    }
}