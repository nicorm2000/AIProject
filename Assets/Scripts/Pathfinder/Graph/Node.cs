using StateMachine.Agents.RTS;
using System;
using System.Collections.Generic;

namespace Pathfinder
{
    /// <summary>
    /// Represents different types of nodes in the graph.
    /// </summary>
    public enum NodeType
    {
        Empty,
        Forest,
        Mine,
        TownCenter,
        TreeCutDown,
        Dirt
    }

    /// <summary>
    /// Represents a node in the graph with a specific coordinate type.
    /// Implements INode and INode<Coordinate> interfaces.
    /// </summary>
    /// <typeparam name="Coordinate">The type of the coordinate used by the node.</typeparam>
    public class Node<Coordinate> : INode, INode<Coordinate>, IEquatable<INode<Coordinate>>
        where Coordinate : IEquatable<Coordinate>
    {
        public NodeType NodeType { get; set; }
        private Coordinate coordinate;
        public ICollection<INode<Coordinate>> neighbors;
        public int food;
        public int gold;
        public int zone;

        /// <summary>
        /// Sets the coordinate of the node.
        /// </summary>
        /// <param name="coordinate">The coordinate to set for the node.</param>
        public void SetCoordinate(Coordinate coordinate)
        {
            this.coordinate = coordinate;
        }

        /// <summary>
        /// Gets the coordinate of the node.
        /// </summary>
        /// <returns>The coordinate of the node.</returns>
        public Coordinate GetCoordinate()
        {
            return coordinate;
        }

        /// <summary>
        /// Determines whether the node is blocked or not. 
        /// Currently returns false; should be modified based on node type.
        /// </summary>
        /// <returns>True if the node is blocked; otherwise, false.</returns>
        public bool IsBlocked()
        {
            return NodeType == NodeType.Forest;
        }

        /// <summary>
        /// Sets the neighboring nodes for the current node.
        /// </summary>
        /// <param name="neighbors">A collection of neighboring nodes to set.</param>
        public void SetNeighbors(ICollection<INode<Coordinate>> neighbors)
        {
            this.neighbors = neighbors;
        }

        /// <summary>
        /// Gets the neighboring nodes of the current node.
        /// </summary>
        /// <returns>A collection of neighboring nodes.</returns>
        public ICollection<INode<Coordinate>> GetNeighbors()
        {
            return neighbors as ICollection<INode<Coordinate>>;
        }

        /// <summary>
        /// Gets the node type.
        /// </summary>
        /// <returns>The type of the node.</returns>
        public NodeType GetNodeType()
        {
            return NodeType;
        }

        /// <summary>
        /// Compares the current node with another node for equality based on coordinates.
        /// </summary>
        /// <param name="other">The other node to compare with.</param>
        /// <returns>True if the coordinates are equal; otherwise, false.</returns>
        public bool EqualsTo(INode<Coordinate> other)
        {
            return coordinate.Equals(other.GetCoordinate());
        }

        /// <summary>
        /// Determines whether this node is equal to another node of the same type.
        /// </summary>
        /// <param name="other">The other node to compare with.</param>
        /// <returns>True if the nodes are equal; otherwise, false.</returns>
        protected bool Equals(Node<Coordinate> other)
        {
            return EqualityComparer<Coordinate>.Default.Equals(coordinate, other.coordinate);
        }

        /// <summary>
        /// Determines whether this node's coordinate is equal to the provided coordinate.
        /// </summary>
        /// <param name="other">The coordinate to compare with.</param>
        /// <returns>True if they are equal; otherwise, false.</returns>
        public bool Equals(Coordinate other)
        {
            return coordinate.Equals(other);
        }

        /// <summary>
        /// Checks if this node is equal to another INode based on coordinates.
        /// </summary>
        /// <param name="other">The other node to compare with.</param>
        /// <returns>True if they are equal; otherwise, false.</returns>
        public bool Equals(INode<Coordinate> other)
        {
            return other != null && coordinate.Equals(other.GetCoordinate());
        }

        /// <summary>
        /// Determines whether the specified object is equal to this instance.
        /// </summary>
        /// <param name="obj">The object to compare with this instance.</param>
        /// <returns>True if the specified object is equal to this instance; otherwise, false.</returns>
        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((Node<Coordinate>)obj);
        }

        /// <summary>
        /// Returns a hash code for this instance.
        /// </summary>
        /// <returns>A hash code for this instance.</returns>
        public override int GetHashCode()
        {
            return EqualityComparer<Coordinate>.Default.GetHashCode(coordinate);
        }
    }
}