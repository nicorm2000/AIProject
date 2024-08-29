public class RotationComponent : ECSComponent
{
    public float X;
    public float Y;
    public float Z;

    public RotationComponent(float X, float Y, float Z)
    {
        this.X = X;
        this.Y = Y;
        this.Z = Z;
    }
}