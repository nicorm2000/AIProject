public class Node<Coordinate> : INode, INode<Coordinate>
{
    private Coordinate coordinate;

    public void SetCoordinate(Coordinate coordinate)
    {
        this.coordinate = coordinate;
    }

    public Coordinate GetCoordinate()
    {
        return coordinate;
    }

    public bool IsBlocked()
    {
        return false;
    }

    public bool EqualsTo(INode node)
    {
        //return coordinate.Equals(node as (Node<Coordinate>).coordinate); 
        throw new System.NotImplementedException();
    }
}