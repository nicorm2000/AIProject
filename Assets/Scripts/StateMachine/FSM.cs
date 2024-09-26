using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using States;

namespace StateMachine
{
    /// <summary>
    /// Finite State Machine (FSM) class that manages states and transitions between them.
    /// It supports parallel execution of behaviors using multiple threads.
    /// </summary>
    /// <typeparam name="EnumState">The enum type representing the states.</typeparam>
    /// <typeparam name="EnumFlag">The enum type representing the flags triggering transitions.</typeparam>
    public class FSM<EnumState, EnumFlag>
        where EnumState : Enum
        where EnumFlag : Enum
    {
        private const int UNASSIGNED_TRANSITION = -1;
        private int _currentState = 0;
        private readonly Dictionary<int, State> _behaviours;
        private readonly Dictionary<int, Func<object[]>> _behaviourTickParameters;
        private readonly Dictionary<int, Func<object[]>> _behaviourOnEnterParameters;
        private readonly Dictionary<int, Func<object[]>> _behaviourOnExitParameters;
        private readonly (int destinationInState, Action onTransition)[,] _transitions;

        // Parallel options for managing multithreaded tasks.
        ParallelOptions parallelOptions = new ParallelOptions()
        {
            MaxDegreeOfParallelism = 32
        };

        private BehaviourActions GetCurrentStateOnEnterBehaviours => _behaviours[_currentState].GetOnEnterBehaviour(_behaviourOnEnterParameters[_currentState]?.Invoke());

        private BehaviourActions GetCurrentStateOnExitBehaviours => _behaviours[_currentState].GetOnExitBehaviour(_behaviourOnExitParameters[_currentState]?.Invoke());

        private BehaviourActions GetCurrentStateTickBehaviours => _behaviours[_currentState].GetTickBehaviour(_behaviourTickParameters[_currentState]?.Invoke());

        /// <summary>
        /// Constructor initializes the FSM, sets up states, flags, and default transitions.
        /// </summary>
        public FSM()
        {
            int states = Enum.GetValues(typeof(EnumState)).Length;
            int flags = Enum.GetValues(typeof(EnumFlag)).Length;
            _behaviours = new Dictionary<int, State>();
            _transitions = new (int, Action)[states, flags];

            for (int i = 0; i < states; i++)
            {
                for (int j = 0; j < flags; j++)
                {
                    _transitions[i, j] = (UNASSIGNED_TRANSITION, null);
                }
            }

            _behaviourTickParameters = new Dictionary<int, Func<object[]>>();
            _behaviourOnEnterParameters = new Dictionary<int, Func<object[]>>();
            _behaviourOnExitParameters = new Dictionary<int, Func<object[]>>();
        }

        /// <summary>
        /// Forces the FSM to transition to a specific state without any trigger or flag.
        /// </summary>
        /// <param name="state">The state to transition to.</param>
        public void ForceTransition(Enum state)
        {
            _currentState = Convert.ToInt32(state);
        }

        /// <summary>
        /// Adds a behavior to a specified state in the FSM.
        /// </summary>
        /// <typeparam name="T">The type of behavior to add, derived from the State class.</typeparam>
        /// <param name="stateIndexEnum">The state for which the behavior is added.</param>
        /// <param name="onTickParameters">Optional parameters for the Tick behavior.</param>
        /// <param name="onEnterParameters">Optional parameters for the OnEnter behavior.</param>
        /// <param name="onExitParameters">Optional parameters for the OnExit behavior.</param>
        public void AddBehaviour<T>(EnumState stateIndexEnum, Func<object[]> onTickParameters = null,
            Func<object[]> onEnterParameters = null, Func<object[]> onExitParameters = null) where T : State, new()
        {
            int stateIndex = Convert.ToInt32(stateIndexEnum);

            if (_behaviours.ContainsKey(stateIndex)) return;

            State newBehaviour = new T();
            newBehaviour.OnFlag += Transition;
            _behaviours.Add(stateIndex, newBehaviour);
            _behaviourTickParameters.Add(stateIndex, onTickParameters);
            _behaviourOnEnterParameters.Add(stateIndex, onEnterParameters);
            _behaviourOnExitParameters.Add(stateIndex, onExitParameters);
        }

        /// <summary>
        /// Defines a transition between two states, triggered by a flag.
        /// </summary>
        /// <param name="originState">The state from which the transition starts.</param>
        /// <param name="flag">The flag that triggers the transition.</param>
        /// <param name="destinationState">The state to transition to.</param>
        /// <param name="onTransition">Optional action to execute during the transition.</param>
        public void SetTransition(Enum originState, Enum flag, Enum destinationState, Action onTransition = null)
        {
            _transitions[Convert.ToInt32(originState), Convert.ToInt32(flag)] =
                (Convert.ToInt32(destinationState), onTransition);
        }

        /// <summary>
        /// Handles transitions between states when triggered by a flag.
        /// </summary>
        /// <param name="flag">The flag that triggers the transition.</param>
        private void Transition(Enum flag)
        {
            if (_transitions[_currentState, Convert.ToInt32(flag)].destinationInState == UNASSIGNED_TRANSITION) return;

            ExecuteBehaviour(GetCurrentStateOnExitBehaviours);
            _transitions[_currentState, Convert.ToInt32(flag)].onTransition?.Invoke();
            _currentState = _transitions[_currentState, Convert.ToInt32(flag)].destinationInState;
            ExecuteBehaviour(GetCurrentStateOnEnterBehaviours);
        }

        /// <summary>
        /// Executes the Tick behavior for the current state.
        /// This is meant to be called on every frame or tick update.
        /// </summary>
        public void Tick()
        {
            if (!_behaviours.ContainsKey(_currentState)) return;

            ExecuteBehaviour(GetCurrentStateTickBehaviours);
        }

        /// <summary>
        /// Executes the provided behavior actions, including multithreaded and main-thread actions.
        /// </summary>
        /// <param name="behaviourActions">The set of actions to execute.</param>
        private void ExecuteBehaviour(BehaviourActions behaviourActions)
        {
            if (behaviourActions.Equals(default(BehaviourActions))) return;

            int executionOrder = 0;

            while (behaviourActions.MainThreadBehaviour != null && behaviourActions.MainThreadBehaviour.Count > 0 ||
                   behaviourActions.MultiThreadablesBehaviour != null &&
                   behaviourActions.MultiThreadablesBehaviour.Count > 0)
            {
                Task multithreadableBehaviour = new Task(() =>
                {
                    if (behaviourActions.MultiThreadablesBehaviour != null)
                    {
                        if (!behaviourActions.MultiThreadablesBehaviour.ContainsKey(executionOrder)) return;
                        Parallel.ForEach(behaviourActions.MultiThreadablesBehaviour[executionOrder], parallelOptions,
                            (behaviour) => { behaviour?.Invoke(); });
                        behaviourActions.MultiThreadablesBehaviour.TryRemove(executionOrder, out _);
                    }
                });

                multithreadableBehaviour.Start();

                if (behaviourActions.MainThreadBehaviour != null)
                {
                    if (behaviourActions.MainThreadBehaviour.ContainsKey(executionOrder))
                    {
                        foreach (var action in behaviourActions.MainThreadBehaviour[executionOrder])
                        {
                            action.Invoke();
                        }
                        behaviourActions.MainThreadBehaviour.Remove(executionOrder);
                    }
                }

                multithreadableBehaviour.Wait();
                executionOrder++;
            }

            behaviourActions.TransitionBehaviour?.Invoke();
        }
    }
}