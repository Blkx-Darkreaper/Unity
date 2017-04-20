using UnityEngine;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace Strikeforce
{
    public class Shelter : Structure
    {
        protected bool hasPatrolStatus = false;
        protected float patrolProgress = 0f;
        public float PatrolDelay = 2f;
        protected bool isScrambling = false;
        protected float scrambleProgress = 0f;
        public float ScrambleDelay = 1f;
        protected Queue<Vehicle> storage { get; set; }
        public int StorageCapacity = 0;
        public bool CanStoreAircraft = false;
        protected float fuelRemaining;
        public int MaxFuelCapacity = 0;

        protected override void Awake()
        {
            base.Awake();

            this.storage = new Queue<Vehicle>(StorageCapacity);
        }

        protected override void Update()
        {
            base.Update();

            float timeElapsed = Time.time;

            RefuelVehicles(timeElapsed);

            if (isScrambling == true)
            {
                Scramble(timeElapsed);
            }
            else
            {
                Patrol(timeElapsed);
            }
        }

        public void ToggleScrambling()
        {
            this.isScrambling = !isScrambling;

            if(isScrambling == false)
            {
                return;
            }

            scrambleProgress = 0f;
        }

        public void SetPatrolStatus(bool status)
        {
            this.hasPatrolStatus = status;

            if(hasPatrolStatus == false)
            {
                return;
            }

            patrolProgress = 0f;
        }

        protected void RefuelVehicles(float timeElapsed)
        {
            if(fuelRemaining <= 0)
            {
                return;
            }

            foreach (Vehicle vehicle in storage)
            {
                if(fuelRemaining < timeElapsed)
                {
                    return;
                }

                float amount = Mathf.Clamp(timeElapsed, 0, fuelRemaining);
                fuelRemaining -= amount;

                vehicle.Refuel(amount);
            }
        }

        protected void Scramble(float timeElapsed)
        {
            if(isScrambling == false)
            {
                return;
            }

            if(scrambleProgress > 0)
            {
                scrambleProgress -= timeElapsed;
                return;
            }

            Vehicle vehicle = storage.Dequeue();

            throw new NotImplementedException();
        }

        protected void Patrol(float timeElapsed)
        {
            if(hasPatrolStatus == false)
            {
                return;
            }

            if(patrolProgress > 0)
            {
                patrolProgress -= timeElapsed;
                return;
            }

            Vehicle vehicle = storage.Dequeue();

            float fuelPercentage = vehicle.GetFuelPercentage();
            if(1 != (int)fuelPercentage)
            {
                storage.Enqueue(vehicle);
                return;
            }

            throw new NotImplementedException();
        }
    }
}