using ECS.Patron;

public class DirectionOffsetComponentFlocking : ECSComponent
{
    public float direction;

    public DirectionOffsetComponentFlocking(float direction)
    {
        this.direction = direction;
    }
}