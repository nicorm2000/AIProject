using System.Collections.Generic;

public class AStarPathfinder<NodeType> : Pathfinder<NodeType> where NodeType : INode
{
    protected override int Distance(NodeType A, NodeType B)
    {
        throw new System.NotImplementedException();
    }

    protected override ICollection<NodeType> GetNeighbours(NodeType node)
    {
        throw new System.NotImplementedException();
    }

    protected override bool IsBlocked(NodeType node)
    {
        throw new System.NotImplementedException();
    }

    protected override int MoveToNeighbourCost(NodeType A, NodeType b)
    {
        throw new System.NotImplementedException();
    }

    protected override bool NodesEquals(NodeType A, NodeType B)
    {
        //La idea es esto
        //if (A.Equals(B))
        //    return true;
        //else
        //    return false;
        throw new System.NotImplementedException();
    }
}
