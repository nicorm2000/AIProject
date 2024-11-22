﻿using System;
using StateMachine.Agents.Simulation;
using States;

namespace StateMachine.States.SimStates
{
    public class SimAttackState : State
    {
        public override BehaviourActions GetTickBehaviour(params object[] parameters)
        {
            if (parameters == null || parameters.Length < 9)
            {
                return default;
            }
            
            BehaviourActions behaviours = new BehaviourActions();

            Action onAttack = parameters[5] as Action;
            float[] outputBrain1 = (float[])parameters[6];
            float[] outputBrain2 = (float[])parameters[7];
            float outputBrain3 = (float)parameters[8];

            if (outputBrain1 == null || outputBrain2 == null)
            {
                return default;
            }
            
            behaviours.AddMultiThreadableBehaviours(0, () =>
            {
                onAttack?.Invoke();
            });

            behaviours.SetTransitionBehaviour(() =>
            {
                if(outputBrain1[0] > 0.5f) OnFlag?.Invoke(Flags.OnEat);
                if(outputBrain2[0] > 0.5f) OnFlag?.Invoke(Flags.OnAttack);
                if(outputBrain3 > 0.5f) OnFlag?.Invoke(Flags.OnSearchFood);
            });
            return behaviours;
        }
        
        public override BehaviourActions GetOnEnterBehaviour(params object[] parameters)
        {
            return default;
        }

        public override BehaviourActions GetOnExitBehaviour(params object[] parameters)
        {
            return default;
        }
    }
}