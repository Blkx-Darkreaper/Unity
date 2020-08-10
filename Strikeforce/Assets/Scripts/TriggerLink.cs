using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

namespace Strikeforce
{
    public class TriggerLink : ScriptableObject
    {
        public Type TriggerType { get; protected set; }
        protected LinkedList<Weapon> allLinkedWeapons { get; set; }
        public float CyclePeriod = 0.5f;
        protected float cycleRemaining { get; set; }
        protected LinkedListNode<Weapon> currentWeaponToFire { get; set; }
        public bool IsFiring { get; set; }
        protected Dictionary<string, int> allWeaponTypes { get; set; }
        public string DominantWeaponType { get; protected set; }
        public int AngledSpread { get; protected set; }
        public int HorizontalSpread { get; protected set; }
        public int GroupingBonus { get; protected set; }
        public enum Type { Primary, Secondary, Special };

        protected void Awake()
        {
            this.allLinkedWeapons = new LinkedList<Weapon>();
            this.cycleRemaining = this.CyclePeriod;
            this.currentWeaponToFire = null;
            this.IsFiring = false;
        }

        public void Init(Type type)
        {
            this.TriggerType = type;
        }

        public void LinkWeapon(Weapon weapon, Vector3 firingPoint)
        {
            if (TriggerType == Type.Special)
            {
                // Only allow Ordnance weapons
                if (weapon.IsOrdnanceWeapon == false)
                {
                    return;
                }
            }

            this.allLinkedWeapons.AddFirst(weapon);

            weapon.SetFiringPointOffset(firingPoint);
        }

        public void UnlinkWeapon(Weapon weapon)
        {
            weapon.SetFiringPointOffset(Vector3.zero);

            this.allLinkedWeapons.Remove(weapon);
            this.currentWeaponToFire = allLinkedWeapons.First;
        }

        public void ReadyWeapons(LinkedList<Weapon> sortedWeapons, string dominantWeaponType, int angledSpread, int horizontalSpread, int groupingBonus)
        {
            if (allLinkedWeapons.Count == 0)
            {
                return;
            }

            SetFiringOrder();

            SetAngledSpread();
            SetHorizontalSpread();
            SetGroupingBonus();
        }

        protected void SetFiringOrder()
        {
            SortedList<int, string> firingOrder = new SortedList<int, string>();

            foreach (string type in allWeaponTypes.Keys)
            {
                GameObject prefab = GlobalAssets.GetWeaponPrefab(type);
                Weapon weapon = prefab.GetComponent<Weapon>();
                int priority = weapon.Priority;
                int quantity = this.allWeaponTypes[type];

                int triples = quantity / 3;
                int pairs = (quantity - 3 * triples) / 2;
                int value = 100 * triples + 10 * pairs + priority;

                firingOrder.Add(value, type);
            }

            // SortedList is ranked lowest to highest
            this.DominantWeaponType = firingOrder[firingOrder.Keys[firingOrder.Count - 1]];

            LinkedList<Weapon> sortedWeapons = new LinkedList<Weapon>();
            foreach (int key in firingOrder.Keys)
            {
                string type = firingOrder[key];

                foreach (Weapon weapon in allLinkedWeapons)
                {
                    string typeToCheck = weapon.Type;
                    if (typeToCheck.Equals(type) == false)
                    {
                        continue;
                    }

                    sortedWeapons.AddFirst(weapon); // Reverse order, SortedList ranks from lowest to highest
                }
            }

            // Weapons are now sorted into firing order based on priority values
            this.allLinkedWeapons = sortedWeapons;

            // Set first weapon to fire
            this.currentWeaponToFire = allLinkedWeapons.First;
        }

        protected void SetAngledSpread()
        {
            if (DominantWeaponType.Equals(Weapon.Types.BOLT) == true)
            {
                return;
            }

            if (allWeaponTypes.ContainsKey(Weapon.Types.BOLT) == false)
            {
                return;
            }

            int quantity = allWeaponTypes[Weapon.Types.BOLT];
            this.AngledSpread = quantity * 5;
        }

        protected void SetHorizontalSpread()
        {
            if (DominantWeaponType.Equals(Weapon.Types.FLAMEBURST) == true)
            {
                return;
            }

            if (allWeaponTypes.ContainsKey(Weapon.Types.FLAMEBURST) == false)
            {
                return;
            }

            int quantity = allWeaponTypes[Weapon.Types.FLAMEBURST];
            this.HorizontalSpread = quantity * 10;
        }

        protected void SetGroupingBonus()
        {
            if (DominantWeaponType.Equals(Weapon.Types.WAVE) == true)
            {
                return;
            }

            if (allWeaponTypes.ContainsKey(Weapon.Types.WAVE) == false)
            {
                return;
            }

            int quantity = allWeaponTypes[Weapon.Types.WAVE];
            this.GroupingBonus = quantity;
        }

        public void Update()
        {
            if (allLinkedWeapons.Count == 0)
            {
                return;
            }

            if (IsFiring == false)
            {
                if (cycleRemaining >= CyclePeriod)
                {
                    return;
                }
            }

            float timeElapsed = Time.deltaTime;
            cycleRemaining -= timeElapsed;
            if (cycleRemaining <= 0)
            {
                // Reset the cycle, and switch to the next firing group
                cycleRemaining += CyclePeriod;

                if (allLinkedWeapons.Count > 0)
                {
                    this.currentWeaponToFire = currentWeaponToFire.Next;
                    if (currentWeaponToFire == null)
                    {
                        this.currentWeaponToFire = allLinkedWeapons.First;
                    }
                }

                //IsFiring = false;
            }

            if (IsFiring == false)
            {
                return;
            }

            FireCurrentWeapon();
        }

        protected void FireCurrentWeapon()
        {
            Weapon weapon = currentWeaponToFire.Value;
            string weaponType = weapon.Type;
            if (weaponType.Equals(DominantWeaponType) == true)
            {
                weapon.CmdFireSimple();
                return;
            }

            // Apply Synergy effects
            weapon.CmdFire(AngledSpread, HorizontalSpread, GroupingBonus);
        }
    }
}