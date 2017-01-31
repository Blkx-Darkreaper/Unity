using UnityEngine;
using System.Collections.Generic;

namespace Strikeforce
{
    public class Raider : Aircraft
    {
        public Hardpoint Center { get; protected set; }
        public Hardpoint LeftWing { get; protected set; }
        public Hardpoint RightWing { get; protected set; }
        public Hardpoint LeftOuterWing { get; protected set; }
        public Hardpoint RightOuterWing { get; protected set; }
        public enum HardpointPosition { LeftOuterWing, LeftWing, Center, RightWing, RightOuterWing }
        public TriggerLink PrimaryFire { get; protected set; }
        public TriggerLink SecondaryFire { get; protected set; }
        public TriggerLink SpecialFire { get; protected set; }  // Only Ordnance weapons can be linked
        public Equipment EquippedItem { get; protected set; }

        public Raider(Hardpoint[] hardpoints) : base()
        {
            // Hardpoints Left 0 1 2 3 4 Right
            this.Center = hardpoints[2];
            this.LeftWing = hardpoints[1];
            this.RightWing = hardpoints[3];
            this.LeftOuterWing = hardpoints[0];
            this.RightOuterWing = hardpoints[4];

            this.PrimaryFire = new TriggerLink();
            this.SecondaryFire = new TriggerLink();
            this.SpecialFire = new TriggerLink();
            this.EquippedItem = null;
        }

        public bool EquipWeapon(Weapon weapon, HardpointPosition hardpointPosition, int row, int column)
        {
            Hardpoint hardpoint = null;
            switch (hardpointPosition)
            {
                case HardpointPosition.LeftOuterWing:
                    hardpoint = LeftOuterWing;
                    break;

                case HardpointPosition.LeftWing:
                    hardpoint = LeftWing;
                    break;

                case HardpointPosition.Center:
                    hardpoint = Center;
                    break;

                case HardpointPosition.RightWing:
                    hardpoint = RightWing;
                    break;

                case HardpointPosition.RightOuterWing:
                    hardpoint = RightOuterWing;
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

        protected class TriggerLink
        {
            public Weapon Dominant { get; protected set; }
            public float AngledSpread { get; protected set; }
            public float HorizontalSpread { get; protected set; }
            public int FiringGroups { get; protected set; }
            public int GroupingSize { get; protected set; }
            protected LinkedList<Weapon> allLinkedWeapons { get; protected set; }
            protected List<FiringGroup> allFiringGroups { get; protected set; }
            protected Dictionary<string, int> allWeaponTypes { get; protected set; }

            public TriggerLink()
            {
                this.Dominant = null;
                this.AngledSpread = 0f;
                this.HorizontalSpread = 0f;
                this.GroupingSize = 1;
                this.allLinkedWeapons = new LinkedList<Weapon>();
                this.allFiringGroups = new List<FiringGroup>();
                this.allWeaponTypes = new Dictionary<string, int>();
            }

            public void LinkWeapon(Weapon weapon)
            {
                this.allLinkedWeapons.AddFirst(weapon);
                
                bool hasType = this.allWeaponTypes.ContainsKey(weapon.Type);
                if (hasType == false)
                {
                    this.allWeaponTypes.Add(weapon.Type, 1);
                }
                else
                {
                    this.allWeaponTypes[weapon.Type]++;
                }
            }

            protected void SetDominant()
            {

            }
        }

        protected class FiringGroup {
            public LinkedList<Weapon> AllWeapons { get; protected set; }

            public FiringGroup()
            {
                this.AllWeapons = new LinkedList<Weapon>();
            }
        }

        protected override void DestroyEntity()
        {
            base.DestroyEntity();

            Debug.Log(string.Format("You have been destroyed."));
        }
    }
}