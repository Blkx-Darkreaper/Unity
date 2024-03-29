using UnityEngine;
using UnityEngine.Networking;
using System;
using System.Collections.Generic;

namespace Strikeforce
{
    public class Raider : Aircraft
    {
        public float StartingSpeed;
        //public float MaxEnergy;
        public float CurrentEnergy { get; protected set; }
        //public Vector3[] AllFiringPoints { get; protected set; }
        public RaiderLoadout Loadout { get; protected set; }
        //public Dictionary<HardpointPosition, Hardpoint[]> AllHardpoints { get; protected set; }
        //protected LinkedList<Armour> allArmour { get; set; }
        //protected LinkedListNode<Armour> nextArmourNode { get; set; }
        //public Equipment EquippedItem { get; protected set; }
        //private static HardpointPosition[] positionOrder = new HardpointPosition[] {
        //            HardpointPosition.Center,
        //            HardpointPosition.LeftWing,
        //            HardpointPosition.RightWing,
        //            HardpointPosition.LeftOuterWing,
        //            HardpointPosition.RightOuterWing
        //        };
        protected bool isBoosting { get; set; }
        protected bool isPrimaryFiring { get; set; }
        protected bool isSecondaryFiring { get; set; }
        protected bool isSpecialFiring { get; set; }

        protected override void Awake()
        {
            base.Awake();

            //this.CurrentEnergy = Loadout.MaxEnergy;

            //this.AllFiringPoints = new Vector3[5];
            //this.AllHardpoints = new Dictionary<HardpointPosition, Hardpoint[]>();

            //this.allArmour = new LinkedList<Armour>();
            //this.nextArmourNode = allArmour.First;

            //this.EquippedItem = null;

            this.isBoosting = false;
        }

        protected void FixedUpdate()
        {
            Boost();
        }

        protected override void DestroyEntity()
        {
            base.DestroyEntity();

            RpcDestroyEntity();
        }

        [ClientRpc]
        public void RpcDestroyEntity()
        {
            Debug.Log(string.Format("You have been destroyed."));
        }

        public void ReadyRaider(RaiderLoadout loadout)
        {
            this.Loadout = loadout;
            this.CurrentEnergy = this.Loadout.MaxEnergy;
        }

        //public void SetLayout(Vector3[] firingPoints, Hardpoint[] leftOuterWing, Hardpoint[] leftWing, Hardpoint[] center, Hardpoint[] rightWing, Hardpoint[] rightOuterWing)
        //{
        //    this.AllFiringPoints = firingPoints;

        //    this.AllHardpoints.Add(HardpointPosition.LeftOuterWing, leftOuterWing);
        //    this.AllHardpoints.Add(HardpointPosition.LeftWing, leftWing);
        //    this.AllHardpoints.Add(HardpointPosition.Center, center);
        //    this.AllHardpoints.Add(HardpointPosition.RightWing, rightWing);
        //    this.AllHardpoints.Add(HardpointPosition.RightOuterWing, rightOuterWing);
        //}

        //public void ReadyWeapons()
        //{
        //    foreach (HardpointPosition position in positionOrder)
        //    {
        //        Hardpoint[] hardpoints = AllHardpoints[position];
        //        foreach (Hardpoint hardpoint in hardpoints)
        //        {
        //            hardpoint.ReadyWeapons();
        //        }
        //    }
        //}

        public void SetIsBoosting(bool isBoosting)
        {
            if (FuelRemaining <= 0)
            {
                this.isBoosting = false;
                return;
            }

            this.isBoosting = isBoosting;
        }

        protected void Boost()
        {
            if (isBoosting == false)
            {
                return;
            }

            float fuelConsumed = Mathf.Clamp(FuelConsumption * Time.fixedDeltaTime, 0, FuelRemaining);
            float boostDuration = fuelConsumed / FuelConsumption;

            float deltaVelocity = Acceleration * boostDuration;
            Accelerate(deltaVelocity);

            FuelRemaining -= fuelConsumed;

            if (FuelRemaining > 0)
            {
                return;
            }

            this.isBoosting = false;
        }

        public void SetPrimaryFire(bool isFiring)
        {
            this.isPrimaryFiring = isFiring;
            SetFiring(isPrimaryFiring, isSecondaryFiring, isSpecialFiring);
        }

        public void SetSecondaryFire(bool isFiring)
        {
            this.isSecondaryFiring = isFiring;
            SetFiring(isPrimaryFiring, isSecondaryFiring, isSpecialFiring);
        }

        public void SetSpecialFire(bool isFiring)
        {
            this.isSpecialFiring = isFiring;
            SetFiring(isPrimaryFiring, isSecondaryFiring, isSpecialFiring);
        }

        public void SetFiring(bool primaryFiring, bool secondaryFiring, bool specialFiring)
        {
            foreach (HardpointPosition position in RaiderLoadout.PositionOrder)
            {
                Hardpoint[] hardpoints = Loadout.AllHardpoints[position];
                foreach (Hardpoint hardpoint in hardpoints)
                {
                    hardpoint.SetPrimaryFire(primaryFiring);

                    hardpoint.SetSecondaryFire(secondaryFiring);

                    hardpoint.SetSpecialFire(specialFiring);
                }
            }
        }

        public void SetEquipmentActive(bool isActive)
        {
            Loadout.EquippedItem.Use(isActive);
        }

        public void UpdateEnergy(float amount)
        {
            this.CurrentEnergy += amount;
        }

        public override void TakeDamage(int amount, RaycastHit hit)
        {
            amount = SoakDamage(amount);

            base.TakeDamage(amount, hit);
        }

        protected int SoakDamage(int damage)
        {
            int distribution = Loadout.allArmour.Count;
            if (distribution == 0)
            {
                return damage;
            }

            for (int i = 0; i < damage; i++)
            {
                if (Loadout.allArmour.Count == 0)
                {
                    break;
                }

                LinkedListNode<Armour> currentNode = Loadout.nextArmourNode;
                Armour currentArmour = currentNode.Value;
                currentArmour.TakeDamage(1);

                // If armour is destroyed
                if (currentArmour.CurrentHitPoints == 0)
                {
                    Loadout.allArmour.Remove(currentNode);
                    currentArmour.DestroyArmour();
                }

                damage--;

                // Get next node
                Loadout.nextArmourNode = Loadout.nextArmourNode.Next;
                if (Loadout.nextArmourNode == null)
                {
                    // Loop round to the beginning of the list
                    Loadout.nextArmourNode = Loadout.allArmour.First;
                }
            }

            return damage;
        }

        //public bool EquipItem(Equipment item, HardpointPosition hardpointPosition, int index, int row, int column)
        //{
        //    Hardpoint hardpoint = AllHardpoints[hardpointPosition][index];

        //    bool isEquippable = hardpoint.IsEquippable(item, row, column);
        //    if (isEquippable == false)
        //    {
        //        return false;
        //    }

        //    hardpoint.Equip(item, row, column, this);
        //    this.EquippedItem = item;

        //    return true;
        //}

        //public bool EquipWeapon(Weapon weapon, HardpointPosition hardpointPosition, int index, int row, int column)
        //{
        //    Hardpoint hardpoint = AllHardpoints[hardpointPosition][index];

        //    bool isEquippable = hardpoint.IsEquippable(weapon, row, column);
        //    if (isEquippable == false)
        //    {
        //        return false;
        //    }

        //    hardpoint.Equip(weapon, row, column, this);

        //    // Link to Primary by default
        //    return LinkWeapon(weapon, hardpointPosition, index, TriggerLink.Type.Primary);
        //}

        //public bool LinkWeapon(Weapon weapon, HardpointPosition hardpointPosition, int index, TriggerLink.Type type)
        //{
        //    int positionIndex = (int)hardpointPosition;
        //    Vector3 firingPoint = AllFiringPoints[positionIndex];

        //    Hardpoint hardpoint = AllHardpoints[hardpointPosition][index];
        //    return hardpoint.LinkWeapon(weapon, firingPoint, type);
        //}

        //public void UnlinkWeapon(Weapon weapon, TriggerLink trigger)
        //{
        //    trigger.UnlinkWeapon(weapon);
        //}

        //public bool EquipArmour(Armour armour, HardpointPosition hardpointPosition, int index, int row, int column)
        //{
        //    Hardpoint hardpoint = AllHardpoints[hardpointPosition][index];

        //    bool isEquippable = hardpoint.IsEquippable(armour, row, column);
        //    if (isEquippable == false)
        //    {
        //        return false;
        //    }

        //    hardpoint.Equip(armour, row, column, this);
        //    allArmour.AddLast(armour);

        //    return true;
        //}
    }
}