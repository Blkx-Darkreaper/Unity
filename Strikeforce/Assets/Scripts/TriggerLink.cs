using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

namespace Strikeforce
{
    public class TriggerLink : MonoBehaviour
    {
        public Type TriggerType { get; protected set; }
        protected LinkedList<Weapon> allLinkedWeapons { get; set; }
        public float CyclePeriod = 0.5f;
        protected float cycleRemaining { get; set; }
        protected LinkedListNode<Weapon> currentWeaponToFire { get; set; }
        protected string DominantWeaponType { get;  set; }
        protected int AngledSpread { get;  set; }
        protected int HorizontalSpread { get;  set; }
        protected int GroupingBonus { get;  set; }
        public bool IsFiring { get; set; }
        public enum Type { Primary, Secondary, Special };

        protected void Awake()
        {
            this.allLinkedWeapons = new LinkedList<Weapon>();
            this.cycleRemaining = this.CyclePeriod;
            this.currentWeaponToFire = null;
            this.IsFiring = false;
        }

        public void Init(Transform parent, Type type)
        {
            transform.parent = parent;
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

            // Weapons are now sorted into firing order based on priority values
            this.allLinkedWeapons = sortedWeapons;

            // Set first weapon to fire
            this.currentWeaponToFire = allLinkedWeapons.First;

            this.DominantWeaponType = dominantWeaponType;
            this.AngledSpread = angledSpread;
            this.HorizontalSpread = horizontalSpread;
            this.GroupingBonus = groupingBonus;
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
                weapon.Fire();
                return;
            }

            // Apply Synergy effects
            weapon.Fire(AngledSpread, HorizontalSpread, GroupingBonus);
        }
    }
}