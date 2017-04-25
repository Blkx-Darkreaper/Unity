using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Networking;
using System.Drawing;
using System;

namespace Strikeforce
{
    public enum Order
    {
        None, Patrol, Attack
    }

    public class Vehicle : Destructible
    {
        protected NavMeshAgent pathfinder;
        [SyncVar]
        protected Order currentOrder;
        public int RepairCost = 1;
        public int MaxSpeed = 5;
        public int Acceleration = 1;
        public int Range = 50;
        [SyncVar]
        protected float distanceTravelled;
        public int MaxFuelCapacity = 0;
        [SyncVar]
        protected float fuelRemaining;
        public int FuelConsumption = 0;

        protected override void Awake()
        {
            base.Awake();

            this.pathfinder = GetComponentInChildren<NavMeshAgent>();
            this.currentOrder = Order.None;
            this.distanceTravelled = 0f;
            this.fuelRemaining = 0f;
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

        public virtual float GetFuelPercentage()
        {
            if(MaxFuelCapacity == 0)
            {
                return 1f;
            }

            float fuelPercentage = fuelRemaining / (float)MaxFuelCapacity;
            return fuelPercentage;
        }

        public virtual void Refuel(float amount)
        {
            fuelRemaining += amount;

            fuelRemaining = Mathf.Clamp(fuelRemaining, 0, MaxFuelCapacity);
        }
    }
}