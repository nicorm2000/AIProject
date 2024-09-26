using System;
using System.Collections.Generic;
using System.Linq;
using StateMachine.Agents.RTS;

namespace Pathfinder
{
    /// <summary>
    /// Struct representing a transition from one node to another, along with the cost of the transition.
    /// </summary>
    /// <typeparam name="NodeType">The type of node used in the graph.</typeparam>
    public struct Transition<NodeType>
    {
        public NodeType to;
        public int cost;
    }

    /// <summary>
    /// Abstract base class for a generic pathfinding algorithm, which implements a pathfinding process similar to A*.
    /// </summary>
    /// <typeparam name="TNodeType">The type of the node in the graph.</typeparam>
    /// <typeparam name="TCoordinateType">The type of the coordinate used by the nodes.</typeparam>
    /// <typeparam name="TCoordinate">The type of the coordinate.</typeparam>
    public abstract class Pathfinder<TNodeType, TCoordinateType, TCoordinate>
        where TNodeType : INode<TCoordinateType>
        where TCoordinateType : IEquatable<TCoordinateType>
        where TCoordinate : ICoordinate<TCoordinateType>, new()
    {
        /// <summary>
        /// The graph represented as a collection of nodes.
        /// </summary>
        protected ICollection<TNodeType> Graph;

        /// <summary>
        /// Finds the optimal path from a starting node to a destination node using the pathfinding algorithm.
        /// </summary>
        /// <param name="startNode">The starting node.</param>
        /// <param name="destinationNode">The goal node.</param>
        /// <param name="agentType">The agent type.</param>
        /// <returns>A list of nodes representing the path, or null if no path is found.</returns>
        public List<TNodeType> FindPath(TNodeType startNode, TNodeType destinationNode, RTSAgent.AgentTypes agentType)
        {
            Dictionary<TNodeType, (TNodeType Parent, int AcumulativeCost, int Heuristic)> nodes =
                new Dictionary<TNodeType, (TNodeType Parent, int AcumulativeCost, int Heuristic)>();

            foreach (TNodeType node in Graph)
            {
                nodes.Add(node, (default, 0, 0));
            }

            TCoordinate startCoor = new TCoordinate();
            startCoor.SetCoordinate(startNode.GetCoordinate());
            List<TNodeType> openList = new List<TNodeType>(); // Nodes to explore
            List<TNodeType> closedList = new List<TNodeType>(); // Nodes already explored

            openList.Add(startNode);

            while (openList.Count > 0)
            {
                TNodeType currentNode = openList[0];
                int currentIndex = 0;

                // Find the node in the open list with the lowest accumulated cost + heuristic
                for (int i = 1; i < openList.Count; i++)
                {
                    if (nodes[openList[i]].AcumulativeCost + nodes[openList[i]].Heuristic >= nodes[currentNode].AcumulativeCost + nodes[currentNode].Heuristic) continue;

                    currentNode = openList[i];
                    currentIndex = i;
                }

                openList.RemoveAt(currentIndex);
                closedList.Add(currentNode);

                // If the current node is the destination, generate the path
                if (NodesEquals(currentNode, destinationNode))
                {
                    return GeneratePath(startNode, destinationNode);
                }

                // Explore neighbors of the current node
                foreach (TNodeType neighbor in GetNeighbors(currentNode))
                {
                    if (!nodes.ContainsKey(neighbor) || IsBlocked(neighbor) || closedList.Contains(neighbor))
                    {
                        continue;
                    }

                    int aproxAcumulativeCost = 0;

                    aproxAcumulativeCost += nodes[currentNode].AcumulativeCost;
                    aproxAcumulativeCost += MoveToNeighborCost(currentNode, neighbor, agentType);  // Add movement cost to the neighbor

                    // Skip if the neighbor is already in the open list and the cost is not lower
                    if (openList.Contains(neighbor) && aproxAcumulativeCost >= nodes[neighbor].AcumulativeCost) continue;

                    TCoordinate neighborCoor = new TCoordinate();
                    neighborCoor.SetCoordinate(neighbor.GetCoordinate());

                    // Update the node's parent, accumulated cost, and heuristic (distance to destination)
                    nodes[neighbor] = (currentNode, aproxAcumulativeCost, Distance(neighborCoor, startCoor));

                    if (!openList.Contains(neighbor))
                    {
                        openList.Add(neighbor);
                    }
                }
            }

            // Return null if no path is found
            return null;

            /// <summary>
            /// Generates the path by retracing from the goal node to the start node using parent references.
            /// </summary>
            /// <param name="startNode">The starting node.</param>
            /// <param name="goalNode">The goal node.</param>
            /// <returns>A list of nodes representing the path from start to goal.</returns>
            List<TNodeType> GeneratePath(TNodeType startNode, TNodeType goalNode)
            {
                List<TNodeType> path = new List<TNodeType>();
                TNodeType currentNode = goalNode;

                // Trace back the path using the parent of each node
                while (!NodesEquals(currentNode, startNode))
                {
                    path.Add(currentNode);

                    foreach (var node in nodes.Keys.ToList().Where(node => NodesEquals(currentNode, node)))
                    {
                        currentNode = nodes[node].Parent;
                        break;
                    }
                }

                path.Reverse();  // Reverse the list to get the path from start to goal
                return path;
            }
        }

        /// <summary>
        /// Abstract method to retrieve the neighbors of a given node.
        /// </summary>
        /// <param name="node">The current node.</param>
        /// <returns>A collection of neighboring nodes.</returns>
        protected abstract ICollection<INode<TCoordinateType>> GetNeighbors(TNodeType node);

        /// <summary>
        /// Abstract method to calculate the distance between two nodes.
        /// </summary>
        /// <param name="A">TCoordinate A.</param>
        /// <param name="B">TCoordinate B.</param>
        /// <returns>The distance between node A and node B.</returns>
        protected abstract int Distance(TCoordinate tCoordinate, TCoordinate coordinate);

        /// <summary>
        /// Abstract method to determine if two nodes are equal.
        /// </summary>
        /// <param name="A">Node A.</param>
        /// <param name="B">Node B.</param>
        /// <returns>True if the nodes are equal, otherwise false.</returns>
        protected abstract bool NodesEquals(TNodeType A, TNodeType B);

        /// <summary>
        /// Abstract method to calculate the cost of moving from one node to a neighboring node.
        /// </summary>
        /// <param name="A">Node A.</param>
        /// <param name="B">Neighboring node B.</param>
        /// <param name="type">The agent type.</param>
        /// <returns>The movement cost from A to B.</returns>
        protected abstract int MoveToNeighborCost(TNodeType A, TNodeType B, RTSAgent.AgentTypes type);

        /// <summary>
        /// Abstract method to determine if a node is blocked or inaccessible.
        /// </summary>
        /// <param name="node">The node to check.</param>
        /// <returns>True if the node is blocked, otherwise false.</returns>
        protected abstract bool IsBlocked(TNodeType node);
    }
}