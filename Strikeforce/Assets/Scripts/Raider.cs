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
        public Dictionary<HardpointPosition, Hardpoint[]> AllHardpoints { get; protected set; }
        public LinkedList<Armour> allArmour { get; protected set; }
        protected LinkedListNode<Armour> nextArmourNode { get; set; }
        public TriggerLink PrimaryFire { get; protected set; }
        public TriggerLink SecondaryFire { get; protected set; }
        public TriggerLink SpecialFire { get; protected set; } // Only Ordnance weapons can be linked
        public Equipment EquippedItem { get; protected set; }

        protected override void Awake()
        {
            base.Awake();

            this.CurrentEnergy = MaxEnergy;

            this.AllHardpoints = new Dictionary<HardpointPosition, Hardpoint[]>();

            this.allArmour = new LinkedList<Armour>();
            this.nextArmourNode = allArmour.First;

            this.PrimaryFire = new TriggerLink();
            this.SecondaryFire = new TriggerLink();
            this.SpecialFire = new TriggerLink();
            this.EquippedItem = null;
        }

        public void FirePrimary()  // Testing
        {
            // create the bullet object from the bullet prefab
            GameObject bullet = (GameObject)Instantiate(
                NetworkManager.singleton.spawnPrefabs[0],
                transform.position + transform.forward,
                Quaternion.identity);

			bullet.transform.parent = gameObject.transform;

            // make the bullet move away in front of the player
            bullet.GetComponentInChildren<Rigidbody>().velocity = transform.forward * 4;

            // spawn the bullet on the clients
            NetworkServer.Spawn(bullet);

            // make bullet disappear after 2 seconds
            Destroy(bullet, 2.0f);
            //PrimaryFire.IsFiring = true;
        }

        protected override void Update()
        {
            PrimaryFire.Update();
        }

        public void UpdateEnergy(float amount)
        {
            this.CurrentEnergy += amount;
        }

        public override void TakeDamage(int damage)
        {
            damage = SoakDamage(damage);
            
            base.TakeDamage(damage);
        }

        private int SoakDamage(int damage)
        {
            int distribution = allArmour.Count;
            if (distribution == 0)
            {
                return damage;
            }

            for (int i = 0; i < damage; i++)
            {
                if (allArmour.Count == 0)
                {
                    break;
                }

                LinkedListNode<Armour> currentNode = nextArmourNode;

                // Get next node
                this.nextArmourNode = nextArmourNode.Next;
                if (this.nextArmourNode == null)
                {
                    // Loop round to the beginning of the list
                    this.nextArmourNode = this.allArmour.First;
                }

                Armour currentArmour = currentNode.Value;
                currentArmour.TakeDamage(1);

                // If armour is destroyed
                if (currentArmour.CurrentHitPoints == 0)
                {
                    DestroyEntity();
                    allArmour.Remove(currentNode);
                }

                damage--;
            }

            return damage;
        }

        public bool EquipItem(Equipment item, HardpointPosition hardpointPosition, int index, int row, int column)
        {
            Hardpoint hardpoint = AllHardpoints[hardpointPosition][index];

            bool isEquippable = hardpoint.IsEquippable(item, row, column);
            if (isEquippable == false)
            {
                return false;
            }

            hardpoint.Equip(item, row, column);
            this.EquippedItem = item;

            return true;
        }

        public bool EquipWeapon(Weapon weapon, HardpointPosition hardpointPosition, int index, int row, int column, TriggerLink trigger)
        {
            Hardpoint hardpoint = AllHardpoints[hardpointPosition][index];

            bool isEquippable = hardpoint.IsEquippable(weapon, row, column);
            if (isEquippable == false)
            {
                return false;
            }

            hardpoint.Equip(weapon, row, column);
            trigger.LinkWeapon(weapon);

            return true;
        }

        public bool EquipArmour(Armour armour, HardpointPosition hardpointPosition, int index, int row, int column)
        {
            Hardpoint hardpoint = AllHardpoints[hardpointPosition][index];

            bool isEquippable = hardpoint.IsEquippable(armour, row, column);
            if (isEquippable == false)
            {
                return false;
            }

            hardpoint.Equip(armour, row, column);
            allArmour.AddLast(armour);

            return true;
        }

        protected override void DestroyEntity()
        {
            base.DestroyEntity();

            Debug.Log(string.Format("You have been destroyed."));
        }
    }
}