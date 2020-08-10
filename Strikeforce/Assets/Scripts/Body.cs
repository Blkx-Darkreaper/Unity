using UnityEngine;
using UnityEngine.Networking;

namespace Strikeforce
{
    public class Body : Entity
    {
        public float maxVelocity, turnSpeed;
        [SyncVar]
        public bool isAirborne = false;
        protected Rigidbody rigidBody { get; set; }
        
        protected struct BodyProperties
        {
            public const string POSITION = "Position";
            public const string ROTATION = "Rotation";
            public const string SCALE = "Scale";
        }

        protected override void Awake()
        {
            base.Awake();

            this.rigidBody = GetComponent<Rigidbody>();
        }

        public Vector3 GetVelocity()
        {
            if (rigidBody == null)
            {
                return Vector3.zero;
            }

            return rigidBody.velocity;
        }

        public void SetVelocity(float velocityX, float velocityY, float velocityZ)
        {
            if (rigidBody == null)
            {
                return;
            }

            rigidBody.velocity = new Vector3(velocityX, velocityY, velocityZ);
        }

        public void SetForwardVelocity(float velocity)
        {
            if (rigidBody == null)
            {
                return;
            }

            rigidBody.velocity = transform.forward * velocity;
        }

        public void Accelerate(float deltaVelocity)
        {
            if (rigidBody == null)
            {
                return;
            }

            rigidBody.velocity += transform.forward * deltaVelocity;
        }

        public void Stop()
        {
            if (rigidBody == null)
            {
                return;
            }

            rigidBody.velocity = Vector3.zero;
        }
    }
}