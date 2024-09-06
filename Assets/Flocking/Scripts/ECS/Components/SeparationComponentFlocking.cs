using ECS.Patron;

public class SeparationComponentFlocking : ECSComponent
{
    public float separation;

    public SeparationComponentFlocking(float separation)
    {
        this.separation = separation;
    }
}