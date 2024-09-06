using ECS.Patron;

public class ACSComponentFlocking : ECSComponent
{
    public float X;
    public float Y;
    public float Z;

    public ACSComponentFlocking(float X, float Y, float Z)
    {
        this.X = X;
        this.Y = Y;
        this.Z = Z;
    }
}