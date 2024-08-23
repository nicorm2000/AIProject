using System.Collections.Generic;
using UnityEngine;

public class AStarPathfinder<NodeType> : Pathfinder<NodeType> where NodeType : INode
{
    protected override int Distance(NodeType A, NodeType B)
    {
        //int distance = 0;
        //
        //distance = Mathf.Abs(Mathf.Sqrt((A as INode(int x, int y) * A) + (B * B)));
        //
        //return distance;
        throw new System.NotImplementedException();
    }

    protected override ICollection<NodeType> GetNeighbours(NodeType node)
    {
        throw new System.NotImplementedException();
    }

    protected override bool IsBlocked(NodeType node)
    {
        return node.IsBlocked();
    }

    protected override int MoveToNeighbourCost(NodeType A, NodeType B)
    {
        throw new System.NotImplementedException();
    }

    protected override bool NodesEquals(NodeType A, NodeType B)
    {
        throw new System.NotImplementedException();
    }
}
