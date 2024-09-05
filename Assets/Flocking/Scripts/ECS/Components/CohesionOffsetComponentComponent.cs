using ECS.Patron;
public class CohesionOffsetComponentComponent : ECSComponent
{
    public float cohesion;

    public CohesionOffsetComponentComponent(float cohesion)
    {
        this.cohesion = cohesion;
    }
}