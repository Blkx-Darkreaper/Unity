using UnityEngine;
using UnityEngine.Networking;

namespace Strikeforce
{
    public class _Targeting : NetworkBehaviour
    {
        [SyncVar]
        protected bool isMoving, isTurning;
        protected Vector3 currentWaypoint;
        protected Quaternion targetHeading;
        protected GameObject targetEntityGameObject;
        protected struct TargetingProperties
        {
            public const string IS_MOVING = "IsMoving";
            public const string IS_TURNING = "IsTurning";
            public const string WAYPOINT = "Waypoint";
            public const string TARGET_HEADING = "TargetHeading";
            public const string TARGET_ID = "TargetId";
        }

        public virtual void SetWaypoint(Vector3 destination)
        {
            this.currentWaypoint = destination;
            this.isTurning = true;
            this.isMoving = false;
            this.targetEntityGameObject = null;
        }

        public virtual void SetWaypoint(Entity target)
        {
            if (target == null)
            {
                return;
            }

            SetWaypoint(target.transform.position);
            this.targetEntityGameObject = target.gameObject;
        }
    }
}