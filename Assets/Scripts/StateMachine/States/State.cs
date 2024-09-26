using System;
using System.Collections.Concurrent;
using System.Collections.Generic;

namespace States
{
    /// <summary>
    /// Struct to handle behaviour actions for main thread and multi-threadable operations.
    /// </summary>
    public struct BehaviourActions
    {
        // Dictionary for storing main thread behaviours based on execution order.
        private Dictionary<int, List<Action>> mainThreadBehaviour;
        // Concurrent dictionary for multi-threadable behaviours, allowing safe operations across threads.
        private ConcurrentDictionary<int, ConcurrentBag<Action>> multiThreadablesBehaviour;
        // Transition behaviour that is executed during state transitions.
        private Action transitionBehaviour;

        /// <summary>
        /// Adds a behaviour to the main thread execution list for a specified order.
        /// </summary>
        /// <param name="executionOrder">The order in which this behaviour should be executed.</param>
        /// <param name="behaviour">The action to be added for execution.</param>
        public void AddMainThreadBehaviours(int executionOrder, Action behaviour)
        {
            mainThreadBehaviour ??= new Dictionary<int, List<Action>>();

            if (mainThreadBehaviour.ContainsKey(executionOrder))
            {
                mainThreadBehaviour[executionOrder].Add(behaviour);
            }
            else
            {
                mainThreadBehaviour.Add(executionOrder, new List<Action> { behaviour });
            }
        }

        /// <summary>
        /// Adds a multi-threadable behaviour to the execution list for a specified order.
        /// This allows for concurrent execution of behaviours.
        /// </summary>
        /// <param name="executionOrder">The order in which this behaviour should be executed.</param>
        /// <param name="behaviour">The action to be executed in a multi-threaded context.</param>
        public void AddMultiThreadableBehaviours(int executionOrder, Action behaviour)
        {
            multiThreadablesBehaviour ??= new ConcurrentDictionary<int, ConcurrentBag<Action>>();

            if (multiThreadablesBehaviour.ContainsKey(executionOrder))
            {
                multiThreadablesBehaviour[executionOrder].Add(behaviour);
            }
            else
            {
                multiThreadablesBehaviour.TryAdd(executionOrder, new ConcurrentBag<Action> { behaviour });
            }
        }

        /// <summary>
        /// Sets a behaviour to be executed during a state transition.
        /// </summary>
        /// <param name="behaviour">The action to execute during the transition.</param>
        public void SetTransitionBehaviour(Action behaviour)
        {
            transitionBehaviour = behaviour;
        }

        /// <summary>
        /// Gets the dictionary of main thread behaviours.
        /// </summary>
        public Dictionary<int, List<Action>> MainThreadBehaviour => mainThreadBehaviour;

        /// <summary>
        /// Gets the concurrent dictionary of multi-threadable behaviours.
        /// </summary>
        public ConcurrentDictionary<int, ConcurrentBag<Action>> MultiThreadablesBehaviour => multiThreadablesBehaviour;

        /// <summary>
        /// Gets the behaviour to be executed during state transitions.
        /// </summary>
        public Action TransitionBehaviour => transitionBehaviour;
    }

    /// <summary>
    /// Abstract class representing a state within the state machine.
    /// </summary>
    public abstract class State
    {
        // Event triggered to notify a state transition via a flag.
        public Action<Enum> OnFlag;

        /// <summary>
        /// Abstract method that must be implemented to return tick behaviours (executed continuously while in the state).
        /// </summary>
        /// <param name="parameters">Optional parameters passed to the behaviour.</param>
        /// <returns>Returns the actions to be executed on each tick.</returns>
        public abstract BehaviourActions GetTickBehaviour(params object[] parameters);

        /// <summary>
        /// Abstract method that must be implemented to return the behaviours when entering the state.
        /// </summary>
        /// <param name="parameters">Optional parameters passed to the behaviour.</param>
        /// <returns>Returns the actions to be executed upon entering the state.</returns>
        public abstract BehaviourActions GetOnEnterBehaviour(params object[] parameters);

        /// <summary>
        /// Abstract method that must be implemented to return the behaviours when exiting the state.
        /// </summary>
        /// <param name="parameters">Optional parameters passed to the behaviour.</param>
        /// <returns>Returns the actions to be executed upon exiting the state.</returns>
        public abstract BehaviourActions GetOnExitBehaviour(params object[] parameters);
    }
}