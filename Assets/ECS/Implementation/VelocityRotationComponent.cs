public class VelocityRotationComponent : ECSComponent
{
    public float velocity;

    public float rotationX;
    public float rotationY;
    public float rotationZ;

    public VelocityRotationComponent(float velocity, float rotationX, float rotationY, float rotationZ)
    {
        this.velocity = velocity;
        this.rotationX = rotationX;
        this.rotationY = rotationY;
        this.rotationZ = rotationZ;
    }
}