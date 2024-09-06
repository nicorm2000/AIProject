using ECS.Patron;
public class CohesionComponentComponent : ECSComponent
{
    public float cohesion;

    public CohesionComponentComponent(float cohesion)
    {
        this.cohesion = cohesion;
    }
}