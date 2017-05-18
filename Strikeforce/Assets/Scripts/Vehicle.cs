using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Networking;
using System;
using System.Drawing;
using System.Collections.Generic;

namespace Strikeforce
{
    public enum Order
    {
        None, Patrol, Attack
    }

    public class Vehicle : Destructible
    {
        protected NavMeshAgent pathfinder;
        protected Queue<Vector3> allWaypoints;
        protected Order currentOrder;
        public int RepairCost = 1;
        public int Acceleration = 1;
        public int Range = 50;
        protected Vector3 previousPosition;
        public float DistanceTravelled { get; protected set; }
        public int MaxFuelCapacity = 0;
        public float FuelRemaining { get; protected set; }
        public int FuelConsumption = 0;
        public Structure HomeBase { get; protected set; }

        protected override void Awake()
        {
            base.Awake();

            this.MaxVelocity = 5;
            this.pathfinder = GetComponentInChildren<NavMeshAgent>();
            this.allWaypoints = new Queue<Vector3>();
        }

        protected override void Start()
        {
            base.Start();

            this.currentOrder = Order.None;
            if (pathfinder != null)
            {
                this.pathfinder.speed = MaxVelocity;
                this.pathfinder.acceleration = Acceleration;
            }
            this.previousPosition = transform.position;
            this.DistanceTravelled = 0f;
            this.FuelRemaining = 0f;
        }

        protected override void Update()
        {
            base.Update();

            UpdateDistanceTravelled();
        }

        public virtual void SetHomeBase(Structure homeBase)
        {
            this.HomeBase = homeBase;
        }

        public virtual void AddWaypoint(Vector3 waypoint)
        {
            this.allWaypoints.Enqueue(waypoint);
        }

        public virtual void SetNextWaypoint()
        {
            if(allWaypoints.Count == 0)
            {
                return;
            }

            Vector3 nextWaypoint = allWaypoints.Dequeue();
            this.pathfinder.SetDestination(nextWaypoint);
        }

        public virtual void Move(float x, float z)
        {
            if (isServer == false)
            {
                return;
            }

            if (x == 0f)
            {
                if (z == 0f)
                {
                    return;
                }
            }

            Vector3 currentPosition = transform.position;

            CurrentLevel.KeepInBounds(currentPosition.x, currentPosition.z, ref x, ref z);

            transform.Translate(x, 0, z);
        }

        public virtual void SetOrder(Order order)
        {
            this.currentOrder = order;
        }

        public virtual void ConsumeFuel()
        {
            if(DistanceTravelled < Range)
            {
                return;
            }

            if(FuelRemaining <= 0)
            {
                return;
            }

            FuelRemaining -= FuelConsumption * Time.deltaTime;
        }

        public virtual float GetFuelPercentage()
        {
            if(MaxFuelCapacity == 0)
            {
                return 1f;
            }

            float fuelPercentage = FuelRemaining / (float)MaxFuelCapacity;
            return fuelPercentage;
        }

        public virtual void Refuel(float amount)
        {
            FuelRemaining += amount;

            FuelRemaining = Mathf.Clamp(FuelRemaining, 0, MaxFuelCapacity);
        }

        protected virtual void UpdateDistanceTravelled()
        {
            this.DistanceTravelled += Vector3.Distance(transform.position, previousPosition);
            this.previousPosition = transform.position;
        }
    }
}