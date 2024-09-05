using System.Collections.Generic;
using System.Threading.Tasks;
using ECS.Patron;

public class ACSSystemFlocking : ECSSystem
{
    private ParallelOptions parallelOptions;
    private IDictionary<uint, PositionComponent> acsComponent;

    public override void Initialize()
    {
        parallelOptions = new ParallelOptions { MaxDegreeOfParallelism = 32 };
    }

    protected override void Execute(float deltaTime)
    {
        throw new System.NotImplementedException();
    }

    protected override void PostExecute(float deltaTime)
    {
        throw new System.NotImplementedException();
    }

    protected override void PreExecute(float deltaTime)
    {
        throw new System.NotImplementedException();
    }
}