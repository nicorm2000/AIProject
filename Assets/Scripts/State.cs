using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using UnityEngine;

public struct BehaviourActions
{
    private Dictionary<int, List<Action>> mainThreadBehaviours;
    private ConcurrentDictionary<int, ConcurrentBag<Action>> multiThreadablesBehaviours;
    private Action transitionBehaviour;

    public void AddMainThreadBehaviours(int executionOrder, Action behaviour)
    {
        mainThreadBehaviours ??= new Dictionary<int, List<Action>>();

        if (!mainThreadBehaviours.ContainsKey(executionOrder))
            mainThreadBehaviours.Add(executionOrder, new List<Action>());

        mainThreadBehaviours[executionOrder].Add(behaviour);
    }

    public void AddMultiThreadableBehaviours(int executionOrder, Action behaviour)
    {
        multiThreadablesBehaviours ??= new ConcurrentDictionary<int, ConcurrentBag<Action>>();

        if (!multiThreadablesBehaviours.ContainsKey(executionOrder))
            multiThreadablesBehaviours.TryAdd(executionOrder, new ConcurrentBag<Action>());

        multiThreadablesBehaviours[executionOrder].Add(behaviour);
    }

    public void SetTransitionBehaviour(Action behaviour)
    {
        transitionBehaviour = behaviour;
    }

    public Dictionary<int, List<Action>> MainThreadBehaviours => mainThreadBehaviours;
    public ConcurrentDictionary<int, ConcurrentBag<Action>> MultiThreadableBehaviours => multiThreadablesBehaviours;
    public Action TransitionBehaviour => transitionBehaviour;
}

public abstract class State
{
    public Action<Enum> OnFlag;
    public abstract BehaviourActions GetTickBehaviours(params object[] parameters);
    public abstract BehaviourActions GetOnEnterBehaviours(params object[] parameters);
    public abstract BehaviourActions GetOnExitBehaviours(params object[] parameters);
}

public sealed class ChaseState : State
{
    public override BehaviourActions GetOnEnterBehaviours(params object[] parameters)
    {
        return default;
    }

    public override BehaviourActions GetOnExitBehaviours(params object[] parameters)
    {
        return default;
    }

    public override BehaviourActions GetTickBehaviours(params object[] parameters)
    {
        Transform OwnerTransform = parameters[0] as Transform;
        Transform TargetTransform = parameters[1] as Transform;
        float speed = Convert.ToSingle(parameters[2]);
        float explodeDistance = Convert.ToSingle(parameters[3]);
        float lostDistance = Convert.ToSingle(parameters[4]);

        BehaviourActions behaviour = new BehaviourActions();
        behaviour.AddMainThreadBehaviours(0,() => 
        {
            OwnerTransform.position += (TargetTransform.position - OwnerTransform.position).normalized * speed * Time.deltaTime;
        });

        behaviour.AddMultiThreadableBehaviours(0,() => 
        {
            Debug.Log("Whistle!");
        });

        behaviour.SetTransitionBehaviour(() =>
        {
            if (Vector3.Distance(TargetTransform.position, OwnerTransform.position) < explodeDistance)
            {
                OnFlag?.Invoke(Flags.OnTargetReach);
            }
            else if (Vector3.Distance(TargetTransform.position, OwnerTransform.position) > explodeDistance)
            {
                OnFlag?.Invoke(Flags.OnTargetLost);
            }
        });

        return behaviour;
    }
}

public sealed class PatrolState : State
{
    private Transform actualTarget;

    public override BehaviourActions GetOnEnterBehaviours(params object[] parameters)
    {
        return default;
    }

    public override BehaviourActions GetOnExitBehaviours(params object[] parameters)
    {
        return default;
    }

    public override BehaviourActions GetTickBehaviours(params object[] parameters)
    {
        Transform ownerTransform = parameters[0] as Transform;
        Transform wayPoint1 = parameters[1] as Transform;
        Transform wayPoint2 = parameters[2] as Transform;
        Transform chaseTarget = parameters[3] as Transform;
        float speed = Convert.ToSingle(parameters[4]);
        float chaseDistance = Convert.ToSingle(parameters[5]);

        BehaviourActions behaviours = new BehaviourActions();
        behaviours.AddMultiThreadableBehaviours(0, () => 
        {
            if (actualTarget == null)
            {
                actualTarget = wayPoint1;
            }
            
            if (Vector3.Distance(ownerTransform.position, actualTarget.position) < 0.2f)
            {
                if (actualTarget == wayPoint1)
                    actualTarget = wayPoint2;
                else
                    actualTarget = wayPoint1;
            }

        });

        behaviours.AddMainThreadBehaviours(1, () =>
        {
            ownerTransform.position += (actualTarget.position - ownerTransform.position).normalized * speed * Time.deltaTime;
        });

        behaviours.SetTransitionBehaviour(() =>
        {
            if (Vector3.Distance(ownerTransform.position, chaseTarget.position) < chaseDistance)
            {
                OnFlag?.Invoke(Flags.OnTargetNear);
            }
        });

        return behaviours;
    }
}

public sealed class ExplodeState : State
{
    public override BehaviourActions GetOnEnterBehaviours(params object[] parameters)
    {
        BehaviourActions behaviours = new BehaviourActions();
        behaviours.AddMultiThreadableBehaviours(0, () =>
        {
            Debug.Log("Boom");
        });
        return behaviours;
    }

    public override BehaviourActions GetOnExitBehaviours(params object[] parameters)
    {
        return default;
    }

    public override BehaviourActions GetTickBehaviours(params object[] parameters)
    {
        BehaviourActions behaviours = new BehaviourActions();
        behaviours.AddMultiThreadableBehaviours(0, () =>
        {
            Debug.Log("F");
        });
        return behaviours;
    }
}