using System;
using System.Collections.Generic;
using System.Threading.Tasks;

public class FSM<EnumState, EnumFlag>
    where EnumState : Enum
    where EnumFlag : Enum
{
    private const int UNNASSIGNED_TRANSITION = -1;
    public int currentState = 0;
    private Dictionary<int, State> behaviour;
    private Dictionary<int, Func<object[]>> behaviourTickParameters;
    private Dictionary<int, Func<object[]>> behaviourOnEnterParameters;
    private Dictionary<int, Func<object[]>> behaviourOnExitParameters;
    private int[,] transitions;
    ParallelOptions parallelOptions = new ParallelOptions() { MaxDegreeOfParallelism = 32 };
    private BehaviourActions GetCurrentStateOnEnterBehaviours => behaviour[currentState].GetOnEnterBehaviours(behaviourOnEnterParameters[currentState]?.Invoke());
    private BehaviourActions GetCurrentStateOnExitBehaviours => behaviour[currentState].GetOnExitBehaviours(behaviourOnExitParameters[currentState]?.Invoke());
    private BehaviourActions GetCurrentStateTickBehaviours => behaviour[currentState].GetTickBehaviours(behaviourTickParameters[currentState]?.Invoke());

    public FSM()
    {
        int states = Enum.GetValues(typeof(EnumState)).Length;
        int flags = Enum.GetValues(typeof(EnumFlag)).Length;

        behaviour = new Dictionary<int, State>();
        transitions = new int[states, flags];

        for (int i = 0; i < states; i++)
        {
            for (int j = 0; j < flags; j++)
            {
                transitions[i, j] = UNNASSIGNED_TRANSITION;
            }
        }

        behaviourTickParameters = new Dictionary<int, Func<object[]>>();
        behaviourOnEnterParameters = new Dictionary<int, Func<object[]>>();
        behaviourOnExitParameters = new Dictionary<int, Func<object[]>>();
    }

    public void AddBehaviour<T>(EnumState state, Func<object[]> onTickParameters = null, Func<object[]> onEnterParameters = null, Func<object[]> onExitParameters = null) where T : State, new()
    {
        int stateIndex = Convert.ToInt32(state);

        if (!behaviour.ContainsKey(stateIndex))
        {
            State newBehaviour = new T();
            newBehaviour.OnFlag += Transition;
            behaviour.Add(stateIndex, newBehaviour);
            behaviourTickParameters.Add(stateIndex, onTickParameters);
            behaviourOnEnterParameters.Add(stateIndex, onEnterParameters);
            behaviourOnExitParameters.Add(stateIndex, onExitParameters);
        }
    }

    public void ForceState(EnumState state)
    {
        currentState = Convert.ToInt32(state);
    }

    public void SetTransition(EnumState originState, EnumFlag flag, EnumState destinationState)
    {
        transitions[Convert.ToInt32(originState), Convert.ToInt32(flag)] = Convert.ToInt32(destinationState);
    }

    private void Transition(Enum flag)
    {
        if (transitions[currentState, Convert.ToInt32(flag)] != UNNASSIGNED_TRANSITION)
        {
            ExecuteBehaviour(GetCurrentStateOnExitBehaviours);

            currentState = transitions[currentState, Convert.ToInt32(flag)];

            ExecuteBehaviour(GetCurrentStateOnEnterBehaviours);
        }
    }

    public void Tick()
    {
        if (behaviour.ContainsKey(currentState))
        {
            ExecuteBehaviour(GetCurrentStateTickBehaviours);
        }
    }

    private void ExecuteBehaviour(BehaviourActions behaviourActions)
    {
        if (behaviourActions.Equals(default(BehaviourActions)))
            return;

        int executionOrder = 0;

        while (behaviourActions.MainThreadBehaviours.Count > 0 || behaviourActions.MultiThreadableBehaviours.Count > 0)
        {
            Task multiThreadableBehaviour = new Task(() =>
            {
                if (behaviourActions.MultiThreadableBehaviours.ContainsKey(executionOrder))
                {
                    Parallel.ForEach(behaviourActions.MultiThreadableBehaviours[executionOrder], parallelOptions, (behaviour) =>
                    {
                        behaviour?.Invoke();
                    });
                    behaviourActions.MultiThreadableBehaviours.TryRemove(executionOrder, out _);
                }
            });

            multiThreadableBehaviour.Start();

            if (behaviourActions.MainThreadBehaviours.ContainsKey(executionOrder))
            {
                foreach (Action behaviour in behaviourActions.MainThreadBehaviours[executionOrder])
                {
                    behaviour?.Invoke();
                }
                behaviourActions.MainThreadBehaviours.Remove(executionOrder);
            }

            multiThreadableBehaviour.Wait();

            executionOrder++;
        }

        behaviourActions.TransitionBehaviour?.Invoke();
    }
}