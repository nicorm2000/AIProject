using System.Collections.Generic;
using System.Threading.Tasks;
using ECS.Patron;

public class ACSSystemFlocking : ECSSystem
{
    private ParallelOptions parallelOptions;
    private IDictionary<uint, AlignmentComponentFlocking> aligmentComponent;
    private IDictionary<uint, CohesionComponentComponent> cohesionComponent;
    private IDictionary<uint, SeparationComponentFlocking> separationComponent;
    private IDictionary<uint, DirectionComponentFlocking> directionComponent;
    private IEnumerable<uint> queryedEntities;

    public override void Initialize()
    {
        parallelOptions = new ParallelOptions { MaxDegreeOfParallelism = 32 };
    }
    protected override void PreExecute(float deltaTime)
    {
        aligmentComponent ??= ECSManager.GetComponents<AlignmentComponentFlocking>();
        cohesionComponent ??= ECSManager.GetComponents<CohesionComponentComponent>();
        separationComponent ??= ECSManager.GetComponents<SeparationComponentFlocking>();
        directionComponent ??= ECSManager.GetComponents<DirectionComponentFlocking>();
        queryedEntities ??= ECSManager.GetEntitiesWhitComponentTypes(typeof(AlignmentComponentFlocking), typeof(CohesionComponentComponent), typeof(SeparationComponentFlocking), typeof(DirectionComponentFlocking));
    }

    protected override void Execute(float deltaTime)
    {
        Parallel.ForEach(queryedEntities, parallelOptions, i =>
        {
            ///*Va un vector3 ACS = */ (aligmentComponent[i] * aligmentComponent[i].alignment)
            //+ (cohesionComponent[i] * cohesionComponent[i].cohesion) +
            //+(aligmentComponent[i] * separationComponent[i].separation) +
            //(directionComponent[i] * directionComponent[i].direction);
        });
    }

    protected override void PostExecute(float deltaTime)
    {
        throw new System.NotImplementedException();
    }
}