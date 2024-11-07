using System;
using System.Collections.Generic;
using System.Linq;
using StateMachine.Agents.RTS;

namespace Pathfinder
{
    public abstract class Pathfinder<TNodeType, TCoordinateType, TCoordinate>
        where TNodeType : INode<TCoordinateType>
        where TCoordinateType : IEquatable<TCoordinateType>
        where TCoordinate : ICoordinate<TCoordinateType>, new()
    {
        protected ICollection<TNodeType> Graph;

        public List<TNodeType> FindPath(TNodeType startNode, TNodeType destinationNode)
        {
            var nodes =
                Graph.ToDictionary(key => key, _ => (Parent: default(TNodeType), AcumulativeCost: 0, Heuristic: 0));

            var startCoor = new TCoordinate();
            startCoor.SetCoordinate(startNode.GetCoordinate());

            var openList = new List<TNodeType> { startNode };
            var closedList = new List<TNodeType>();

            while (openList.Count > 0)
            {
                var currentNode = openList.OrderBy(node => nodes[node].AcumulativeCost + nodes[node].Heuristic).First();
                openList.Remove(currentNode);
                closedList.Add(currentNode);

                if (NodesEquals(currentNode, destinationNode)) return GeneratePath(startNode, destinationNode);

                foreach (TNodeType neighbor in GetNeighbors(currentNode))
                {
                    if (!nodes.ContainsKey(neighbor) || IsBlocked(neighbor) || closedList.Contains(neighbor)) continue;

                    var aproxAcumulativeCost = nodes[currentNode].AcumulativeCost
                                               + MoveToNeighborCost(currentNode, neighbor);

                    if (openList.Contains(neighbor) && aproxAcumulativeCost >= nodes[neighbor].AcumulativeCost)
                        continue;

                    var neighborCoor = new TCoordinate();
                    neighborCoor.SetCoordinate(neighbor.GetCoordinate());

                    nodes[neighbor] = (currentNode, aproxAcumulativeCost, Distance(neighborCoor, startCoor));

                    if (!openList.Contains(neighbor)) openList.Add(neighbor);
                }
            }

            return null;

            List<TNodeType> GeneratePath(TNodeType startNode, TNodeType goalNode)
            {
                var path = new List<TNodeType>();
                var currentNode = goalNode;

                while (!NodesEquals(currentNode, startNode))
                {
                    path.Add(currentNode);

                    foreach (var node in nodes.Keys.ToList().Where(node => NodesEquals(currentNode, node)))
                    {
                        currentNode = nodes[node].Parent;
                        break;
                    }
                }

                path.Reverse();
                return path;
            }
        }

        protected abstract ICollection<INode<TCoordinateType>> GetNeighbors(TNodeType node);

        protected abstract int Distance(TCoordinate tCoordinate, TCoordinate coordinate);

        protected abstract bool NodesEquals(TNodeType A, TNodeType B);

        protected abstract int MoveToNeighborCost(TNodeType A, TNodeType B);

        protected abstract bool IsBlocked(TNodeType node);
    }
}