using ECS.Patron;

public class SeparationOffsetComponentFlocking : ECSComponent
{
    public float separation;

    public SeparationOffsetComponentFlocking(float separation)
    {
        this.separation = separation;
    }
}