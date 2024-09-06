using ECS.Patron;

public class AlignmentComponentFlocking : ECSComponent
{
    public float alignment;

    public AlignmentComponentFlocking(float alignment)
    {
        this.alignment = alignment;
    }
}