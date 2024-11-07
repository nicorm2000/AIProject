﻿using System;
using Units;
using UnityEngine;
using Utils;

namespace States.Archer
{
    public class Shoot : State
    {
        private const float SHOOTCOOLDOWN = 3;
        private float _lastAttack = -SHOOTCOOLDOWN;

        public override BehaviourActions GetTickBehaviour(params object[] parameters)
        {
            var behaviours = new BehaviourActions();

            var arrowPrefab = parameters[0] as GameObject;
            var ownerTransform = parameters[1] as Transform;
            var targetTransform = parameters[2] as Transform;
            var shootForce = Convert.ToSingle(parameters[3]);
            var lostDistance = Convert.ToSingle(parameters[4]);

            behaviours.AddMainThreadBehaviours(0, () =>
            {
                if (Time.time - _lastAttack < SHOOTCOOLDOWN) return;

                ShootArrow(arrowPrefab, ownerTransform, targetTransform, shootForce);

                _lastAttack = Time.time;
            });

            behaviours.SetTransitionBehaviour(() =>
            {
                if (Vector3.Distance(targetTransform.position, ownerTransform.position) > lostDistance)
                    OnFlag?.Invoke(Flags.OnTargetLost);
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


        private static void ShootArrow(GameObject arrowPrefab, Transform ownerTransform, Transform targetTransform,
            float shootForce)
        {
            var arrow = Helper.InstantiatePrefab(arrowPrefab, ownerTransform.position, ownerTransform.rotation);

            var direction = (targetTransform.position - ownerTransform.position).normalized;

            var arrowRigidbody = arrow.GetComponent<Rigidbody>();

            arrowRigidbody.AddForce(direction * shootForce, ForceMode.Impulse);
        }
    }
}