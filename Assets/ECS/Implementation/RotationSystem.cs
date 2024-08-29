using System.Collections.Generic;
using System.Threading.Tasks;

public sealed class RotationSystem : ECSSystem
{
    private ParallelOptions parallelOptions;

    private IDictionary<uint, RotationComponent> rotationComponents;
    private IDictionary<uint, VelocityRotationComponent> velocityRotationComponents;
    private IEnumerable<uint> queryedEntities;

    public override void Initialize()
    {
        parallelOptions = new ParallelOptions { MaxDegreeOfParallelism = 32 };
    }

    protected override void PreExecute(float deltaTime)
    {
        rotationComponents ??= ECSManager.GetComponents<RotationComponent>();
        velocityRotationComponents ??= ECSManager.GetComponents<VelocityRotationComponent>();
        queryedEntities ??= ECSManager.GetEntitiesWhitComponentTypes(typeof(RotationComponent), typeof(VelocityRotationComponent));
    }

    protected override void Execute(float deltaTime)
    {
        Parallel.ForEach(queryedEntities, parallelOptions, i =>
        {
            rotationComponents[i].X += velocityRotationComponents[i].rotationX * velocityRotationComponents[i].velocity * deltaTime;
            rotationComponents[i].Y += velocityRotationComponents[i].rotationY * velocityRotationComponents[i].velocity * deltaTime;
            rotationComponents[i].Z += velocityRotationComponents[i].rotationZ * velocityRotationComponents[i].velocity * deltaTime;
        });
    }

    protected override void PostExecute(float deltaTime)
    {
    }
}