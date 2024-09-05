using ECS.Patron;

public class TurnSpeedComponentFlocking : ECSComponent
{
    public float turnSpeed;

    public TurnSpeedComponentFlocking(float turnSpeed)
    {
        this.turnSpeed = turnSpeed;
    }
}