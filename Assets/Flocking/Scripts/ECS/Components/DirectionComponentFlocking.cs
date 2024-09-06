using ECS.Patron;

public class DirectionComponentFlocking : ECSComponent
{
    public float direction;

    public DirectionComponentFlocking(float direction)
    {
        this.direction = direction;
    }
}