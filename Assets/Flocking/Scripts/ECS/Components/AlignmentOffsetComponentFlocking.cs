using ECS.Patron;

public class AlignmentOffsetComponentFlocking : ECSComponent
{
    public float alignment;

    public AlignmentOffsetComponentFlocking(float alignment)
    {
        this.alignment = alignment;
    }
}