using UnityEngine;
using UnityEngine.Networking;
using System;
using System.Collections.Generic;

namespace Strikeforce
{
    public class Raider : Aircraft
    {
        public float MaxEnergy;
        public float CurrentEnergy { get; protected set; }
        public Hardpoint[] AllHardpoints;
        public TriggerLink PrimaryFire { get; protected set; }
        public TriggerLink SecondaryFire { get; protected set; }
        public TriggerLink SpecialFire { get; protected set; } // Only Ordnance weapons can be linked
        public Equipment EquippedItem { get; protected set; }

        protected override void Awake()
        {
            base.Awake();

            this.CurrentEnergy = MaxEnergy;

            this.PrimaryFire = new TriggerLink();
            this.SecondaryFire = new TriggerLink();
            this.SpecialFire = new TriggerLink();
            this.EquippedItem = null;
        }

        public void FirePrimary()  // Testing
        {
            //// create the bullet object from the bullet prefab
            //GameObject bullet = (GameObject)Instantiate(
            //    NetworkManager.singleton.spawnPrefabs[0],
            //    transform.position + transform.forward,
            //    Quaternion.identity);

            //// make the bullet move away in front of the player
            //bullet.GetComponentInChildren<Rigidbody>().velocity = transform.forward * 4;

            //// spawn the bullet on the clients
            //NetworkServer.Spawn(bullet);

            //// make bullet disappear after 2 seconds
            //Destroy(bullet, 2.0f);
            PrimaryFire.IsFiring = true;
        }

        protected override void Update()
        {
            PrimaryFire.Update();
        }

        public void UseEnergy(float amount)
        {
            this.CurrentEnergy -= amount;
        }

        public bool EquipWeapon(Weapon weapon, HardpointPosition hardpointPosition, int row, int column)
        {
            Hardpoint hardpoint = null;
            switch (hardpointPosition)
            {
                case HardpointPosition.LeftOuterWing:
                    hardpoint = AllHardpoints[0];
                    break;

                case HardpointPosition.LeftWing:
                    hardpoint = AllHardpoints[1];
                    break;

                case HardpointPosition.Center:
                    hardpoint = AllHardpoints[2];
                    break;

                case HardpointPosition.RightWing:
                    hardpoint = AllHardpoints[3];
                    break;

                case HardpointPosition.RightOuterWing:
                    hardpoint = AllHardpoints[4];
                    break;
            }

            bool isEquippable = hardpoint.IsEquippable(weapon, row, column);
            if (isEquippable == false)
            {
                return false;
            }

            hardpoint.Equip(weapon, row, column);
            this.PrimaryFire.LinkWeapon(weapon);

            return true;
        }

        protected override void DestroyEntity()
        {
            base.DestroyEntity();

            Debug.Log(string.Format("You have been destroyed."));
        }
    }
}