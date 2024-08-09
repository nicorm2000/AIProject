using System;
using UnityEngine;

public enum Behaviours
{
    Chase, Patrol, Explode
}

public enum Flags
{
    OnTargetReach, OnTargetNear, OnTargetLost
}

public class Agent : MonoBehaviour
{
    public Transform target;
    public Transform wayPoint1;
    public Transform wayPoint2;
    public float speed;
    public float explodeDistance;
    public float lostDistance;
    public float chaseDistance;

    private FSM fsm;

    private void Start()
    {
        fsm = new FSM(Enum.GetValues(typeof(Behaviour)).Length, Enum.GetValues(typeof(Flags)).Length);

        //Add states and transitions
        fsm.AddBehaviour<ChaseState>((int)Behaviours.Chase,
            onTickParameters: () => { return new object[] { transform, target, speed, explodeDistance, lostDistance }; });
        fsm.SetTransition((int)Behaviours.Chase, (int)Flags.OnTargetReach, (int)Behaviours.Explode);
        fsm.SetTransition((int)Behaviours.Chase, (int)Flags.OnTargetLost, (int)Behaviours.Patrol);

        fsm.AddBehaviour<ChaseState>((int)Behaviours.Patrol,
            onTickParameters: () => { return new object[] { transform, wayPoint1, wayPoint2, target, speed, chaseDistance }; });
        fsm.SetTransition((int)Behaviours.Patrol, (int)Flags.OnTargetNear, (int)Behaviours.Chase);

        fsm.AddBehaviour<ChaseState>((int)Behaviours.Explode);
    }

    private void Update()
    {
        fsm.Tick();
    }
}