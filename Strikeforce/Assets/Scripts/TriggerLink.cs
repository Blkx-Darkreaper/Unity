using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

namespace Strikeforce
{
    public class TriggerLink
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

        public TriggerLink(Type type)
        {
            this.TriggerType = type;
            this.allLinkedWeapons = new LinkedList<Weapon>();
            this.cycleRemaining = this.CyclePeriod;
            this.currentWeaponToFire = null;
            this.IsFiring = false;
            this.allWeaponTypes = new Dictionary<string, int>();
            this.DominantWeaponType = string.Empty;
            this.AngledSpread = 0;
            this.HorizontalSpread = 0;
            this.GroupingBonus = 0;
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

        public void UnlinkWeapon(Weapon weapon)
        {
            weapon.SetFiringPointOffset(Vector3.zero);

            this.allLinkedWeapons.Remove(weapon);
            this.currentWeaponToFire = allLinkedWeapons.First;

            bool hasType = this.allWeaponTypes.ContainsKey(weapon.Type);
            if (hasType == false)
            {
                return;
            }

            this.allWeaponTypes[weapon.Type]--;
        }

        public void ReadyWeapons()
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
                GameObject prefab = GlobalAssets.WeaponPrefabs[type];
                Weapon weapon = prefab.GetComponent<Weapon>();
                int priority = weapon.Priority;
                int quantity = this.allWeaponTypes[type];

                int triples = quantity / 3;
                int pairs = (quantity - 3 * triples) / 2;
                int value = 100 * triples + 10 * pairs + priority;

                firingOrder.Add(value, type);
            }

            // Set the dominant weapon type
            this.DominantWeaponType = firingOrder[firingOrder.Keys[0]];

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

                    sortedWeapons.AddLast(weapon);
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
            if (weaponType.Equals(this.DominantWeaponType) == true)
            {
                weapon.Fire();
                return;
            }

            // Apply Synergy effects
            weapon.Fire(AngledSpread, HorizontalSpread, GroupingBonus);
        }
    }
}