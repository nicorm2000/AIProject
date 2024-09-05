using ECS.Patron;
public class DetectionRadiusComponentFlocking : ECSComponent
{
    public float detectionRadius;

    public DetectionRadiusComponentFlocking(float detectionRadius)
    {
        this.detectionRadius = detectionRadius;
    }
}