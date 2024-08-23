public interface INode
{
    public bool EqualsTo(INode node);
    public bool IsBlocked();
}

public interface INode<Coordinate> 
{
    public void SetCoordinate(Coordinate coordinateType);
    public Coordinate GetCoordinate();
}
