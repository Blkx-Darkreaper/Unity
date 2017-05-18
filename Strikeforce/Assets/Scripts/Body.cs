using UnityEngine;
using UnityEngine.Networking;
using System.Collections;
using Newtonsoft.Json;

namespace Strikeforce
{
    public class Body : Entity
    {
        protected bool isLoadedFromSave = false;
        public float MaxVelocity, TurnSpeed;
        [SyncVar]
        protected bool isMoving, isTurning;
        protected Vector3 currentWaypoint;
        protected Quaternion targetHeading;
        protected GameObject targetEntityGameObject;
        public Rect playingArea { get; set; }
        protected bool isFlashing = false;
        protected float flashDuration { get; set; }
        protected struct BodyProperties
        {
            public const string MESHES = "Meshes";
            public const string POSITION = "Position";
            public const string ROTATION = "Rotation";
            public const string SCALE = "Scale";
            public const string IS_MOVING = "IsMoving";
            public const string IS_TURNING = "IsTurning";
            public const string WAYPOINT = "Waypoint";
            public const string TARGET_HEADING = "TargetHeading";
            public const string TARGET_ID = "TargetId";
        }

        protected override void Awake()
        {
            base.Awake();

            playingArea = new Rect(0f, 0f, 0f, 0f);
        }

        protected virtual void Update() {
            FlashFade();
        }

        public void SetColliders(bool enabled)
        {
            Collider[] allColliders = GetComponentsInChildren<Collider>();
            foreach (Collider collider in allColliders)
            {
                collider.enabled = enabled;
            }
        }

        public Vector3 GetVelocity()
        {
            Rigidbody rigidBody = GetComponent<Rigidbody>();
            if(rigidBody == null)
            {
                return Vector3.zero;
            }

            return rigidBody.velocity;
        }

        public void SetVelocity(float velocityX, float velocityY, float velocityZ)
        {
            Rigidbody rigidBody = GetComponent<Rigidbody>();
            if (rigidBody == null)
            {
                return;
            }

            rigidBody.velocity = new Vector3(velocityX, velocityY, velocityZ);
        }

        public void SetForwardVelocity(float velocity)
        {
            Rigidbody rigidBody = GetComponent<Rigidbody>();
            if (rigidBody == null)
            {
                return;
            }

            rigidBody.velocity = transform.forward * velocity;
        }

        public void Accelerate(float deltaVelocity)
        {
            Rigidbody rigidBody = GetComponent<Rigidbody>();
            if (rigidBody == null)
            {
                return;
            }

            rigidBody.velocity += transform.forward * deltaVelocity;
        }

        public void Stop()
        {
            Rigidbody rigidBody = GetComponent<Rigidbody>();
            if (rigidBody == null)
            {
                return;
            }

            rigidBody.velocity = Vector3.zero;
        }

        public virtual void SetWaypoint(Vector3 destination)
        {
            currentWaypoint = destination;
            isTurning = true;
            isMoving = false;
            targetEntityGameObject = null;
        }

        public virtual void SetWaypoint(Entity target)
        {
            if (target == null)
            {
                return;
            }

            SetWaypoint(target.transform.position);
            targetEntityGameObject = target.gameObject;
        }

        public virtual void Flash(Color colour, float duration)
        {
            MeshRenderer mesh = GetComponentInChildren<MeshRenderer>();
            mesh.material.color = colour;

            this.isFlashing = true;
            this.flashDuration = duration;
        }

        public virtual void FlashFade()
        {
            if(isFlashing == false)
            {
                return;
            }

            MeshRenderer mesh = GetComponentInChildren<MeshRenderer>();
            if(mesh.material.color == Color.clear)
            {
                isFlashing = false;
                return;
            }

            mesh.material.color = Color.Lerp(mesh.material.color, Color.clear, flashDuration * Time.deltaTime);
        }
    }
}