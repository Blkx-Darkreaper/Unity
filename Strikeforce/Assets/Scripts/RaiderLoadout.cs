using UnityEngine;
using System;
using System.Collections.Generic;

namespace Strikeforce
{
    public class RaiderLoadout : MonoBehaviour
    {
        public string RaiderType;
        public float StartingSpeed;
        public float MaxEnergy;
        public Vector3[] AllFiringPoints { get; protected set; }
        public Dictionary<HardpointPosition, Hardpoint[]> AllHardpoints { get; protected set; }
        public LinkedList<Armour> allArmour { get; protected set; }
        public LinkedListNode<Armour> nextArmourNode { get; set; }
        public Equipment EquippedItem { get; protected set; }
        public static HardpointPosition[] PositionOrder = new HardpointPosition[] {
                    HardpointPosition.Center,
                    HardpointPosition.LeftWing,
                    HardpointPosition.RightWing,
                    HardpointPosition.LeftOuterWing,
                    HardpointPosition.RightOuterWing
        };

        public void SetLayout(Vector3[] firingPoints, Hardpoint[] leftOuterWing, Hardpoint[] leftWing, Hardpoint[] center, Hardpoint[] rightWing, Hardpoint[] rightOuterWing)
        {
            this.AllFiringPoints = firingPoints;

            this.AllHardpoints.Add(HardpointPosition.LeftOuterWing, leftOuterWing);
            this.AllHardpoints.Add(HardpointPosition.LeftWing, leftWing);
            this.AllHardpoints.Add(HardpointPosition.Center, center);
            this.AllHardpoints.Add(HardpointPosition.RightWing, rightWing);
            this.AllHardpoints.Add(HardpointPosition.RightOuterWing, rightOuterWing);
        }

        public void ReadyWeapons()
        {
            foreach (HardpointPosition position in PositionOrder)
            {
                Hardpoint[] hardpoints = AllHardpoints[position];
                foreach (Hardpoint hardpoint in hardpoints)
                {
                    hardpoint.ReadyWeapons();
                }
            }
        }

        public bool EquipItem(Equipment item, HardpointPosition hardpointPosition, int index, int row, int column, Raider parent)
        {
            Hardpoint hardpoint = AllHardpoints[hardpointPosition][index];

            bool isEquippable = hardpoint.IsEquippable(item, row, column);
            if (isEquippable == false)
            {
                return false;
            }

            hardpoint.Equip(item, row, column, parent);
            this.EquippedItem = item;

            return true;
        }

        public bool EquipWeapon(Weapon weapon, HardpointPosition hardpointPosition, int index, int row, int column, Raider parent)
        {
            Hardpoint hardpoint = AllHardpoints[hardpointPosition][index];

            bool isEquippable = hardpoint.IsEquippable(weapon, row, column);
            if (isEquippable == false)
            {
                return false;
            }

            hardpoint.Equip(weapon, row, column, parent);

            // Link to Primary by default
            return LinkWeapon(weapon, hardpointPosition, index, TriggerLink.Type.Primary);
        }

        public bool LinkWeapon(Weapon weapon, HardpointPosition hardpointPosition, int index, TriggerLink.Type type)
        {
            int positionIndex = (int)hardpointPosition;
            Vector3 firingPoint = AllFiringPoints[positionIndex];

            Hardpoint hardpoint = AllHardpoints[hardpointPosition][index];
            return hardpoint.LinkWeapon(weapon, firingPoint, type);
        }

        public void UnlinkWeapon(Weapon weapon, TriggerLink trigger)
        {
            trigger.UnlinkWeapon(weapon);
        }

        public bool EquipArmour(Armour armour, HardpointPosition hardpointPosition, int index, int row, int column, Raider parent)
        {
            Hardpoint hardpoint = AllHardpoints[hardpointPosition][index];

            bool isEquippable = hardpoint.IsEquippable(armour, row, column);
            if (isEquippable == false)
            {
                return false;
            }

            hardpoint.Equip(armour, row, column, parent);
            allArmour.AddLast(armour);

            return true;
        }
    }
}