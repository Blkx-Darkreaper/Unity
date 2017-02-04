﻿using UnityEngine;
using UnityEngine.Networking;
using System.Collections;
using Newtonsoft.Json;

namespace Strikeforce
{
    public class Body : Entity
    {
        protected bool isLoadedFromSave = false;
        public float MoveSpeed, TurnSpeed;
        protected bool isMoving, isTurning;
        private Vector3 currentWaypoint;
        private Quaternion targetHeading;
        private GameObject targetEntityGameObject;
        public Rect playingArea { get; set; }
        protected struct UnitProperties
        {
            public const string IS_MOVING = "IsMoving";
            public const string IS_TURNING = "IsTurning";
            public const string WAYPOINT = "Waypoint";
            public const string TARGET_HEADING = "TargetHeading";
            public const string TARGET_ID = "TargetId";
        }
        protected struct BodyProperties
        {
            public const string MESHES = "Meshes";
            public const string POSITION = "Position";
            public const string ROTATION = "Rotation";
            public const string SCALE = "Scale";
        }

        protected override void Awake()
        {
            base.Awake();

            playingArea = new Rect(0f, 0f, 0f, 0f);
        }

        public void SetColliders(bool enabled)
        {
            Collider[] allColliders = GetComponentsInChildren<Collider>();
            foreach (Collider collider in allColliders)
            {
                collider.enabled = enabled;
            }
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
    }
}