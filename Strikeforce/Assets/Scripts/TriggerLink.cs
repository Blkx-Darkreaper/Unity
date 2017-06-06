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
        public enum Type { Primary, Secondary, Special };

        public TriggerLink(Type type)
        {
            this.TriggerType = type;
            this.allLinkedWeapons = new LinkedList<Weapon>();
            this.cycleRemaining = this.CyclePeriod;
            this.currentWeaponToFire = null;
            this.IsFiring = false;
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

        public void ReadyWeapons(LinkedList<Weapon> sortedWeapons)
        {
            if (allLinkedWeapons.Count == 0)
            {
                return;
            }

            // Weapons are now sorted into firing order based on priority values
            this.allLinkedWeapons = sortedWeapons;

            // Set first weapon to fire
            this.currentWeaponToFire = allLinkedWeapons.First;
        }

        public void Update(string dominantWeaponType, int angledSpread, int horizontalSpread, int groupingBonus)
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

            FireCurrentWeapon(dominantWeaponType, angledSpread, horizontalSpread, groupingBonus);
        }

        protected void FireCurrentWeapon(string dominantWeaponType, int angledSpread, int horizontalSpread, int groupingBonus)
        {
            Weapon weapon = currentWeaponToFire.Value;
            string weaponType = weapon.Type;
            if (weaponType.Equals(dominantWeaponType) == true)
            {
                weapon.Fire();
                return;
            }

            // Apply Synergy effects
            weapon.Fire(angledSpread, horizontalSpread, groupingBonus);
        }
    }
}