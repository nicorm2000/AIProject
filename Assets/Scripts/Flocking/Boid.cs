using System;
using UnityEngine;

namespace Flocking
{
    public class Boid : MonoBehaviour
    {
        public float speed = 2.5f;
        public float turnSpeed = 5f;
        public float detectionRadious = 3.0f;
        public float alignmentOffset;
        public float cohesionOffset;
        public float separationOffset;
        public float directionOffset;
        public Transform target;

        private Func<Boid, Vector2> alignment;
        private Func<Boid, Vector2> cohesion;
        private Func<Boid, Vector2> separation;
        private Func<Boid, Vector2> direction;

        public void Init(Func<Boid, Vector2> Alignment,
            Func<Boid, Vector2> Cohesion,
            Func<Boid, Vector2> Separation,
            Func<Boid, Vector2> Direction)
        {
            this.alignment = Alignment;
            this.cohesion = Cohesion;
            this.separation = Separation;
            this.direction = Direction;
        }

        private void Update()
        {
            Vector2 desiredDirection = ACS();
            transform.forward = Vector2.Lerp(transform.forward, desiredDirection, turnSpeed * Time.deltaTime);
            transform.position += transform.forward * (speed * Time.deltaTime);
        }

        public Vector2 ACS()
        {
            Vector2 ACS = (alignment(this) * alignmentOffset) +
                          (cohesion(this) * cohesionOffset) +
                          (separation(this) * separationOffset) +
                          (direction(this) * directionOffset);
            return ACS.normalized;
        }
        
        public Vector2 GetDirection()
        {
            return direction(this);
        }
        
        public Vector2 GetAlignment()
        {
            return alignment(this);
        }
        
        public Vector2 GetCohesion()
        {
            return cohesion(this);
        }
        
        public Vector2 GetSeparation()
        {
            return separation(this);
        }
    }
}