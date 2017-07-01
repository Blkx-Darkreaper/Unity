using UnityEngine;
using UnityEngine.Networking;
using System.Drawing;
using System.Collections.Generic;

namespace Strikeforce
{
    public enum HardpointPosition { LeftOuterWing, LeftWing, Center, RightWing, RightOuterWing }

    public class Hardpoint : NetworkBehaviour
    {
        [HideInInspector]
        public Vector2 Location;
        public HardpointPosition Position { get; protected set; }
        public static Size PixelsPerSlot = new Size(42, 42);
        public int Width { get { return EquippedItems.GetLength(1); } }
        public int Height { get { return EquippedItems.GetLength(0); } }
        public Equipment[,] EquippedItems { get; protected set; }
        protected Dictionary<string, LinkedList<Weapon>> allWeaponTypes { get; set; }
        public string DominantWeaponType { get; protected set; }
        public int AngledSpread { get; protected set; }
        public int HorizontalSpread { get; protected set; }
        public int GroupingBonus { get; protected set; }
        protected TriggerLink primaryFire;
        protected TriggerLink secondaryFire;
        protected TriggerLink specialFire;

        protected void Awake()
        {
            this.allWeaponTypes = new Dictionary<string, LinkedList<Weapon>>();
            this.DominantWeaponType = string.Empty;
            this.AngledSpread = 0;
            this.HorizontalSpread = 0;
            this.GroupingBonus = 0;
        }

        public void Init(int relativeToCenterX, int relativeToCenterY, int width, int height, HardpointPosition position)
        {
            this.Location = new Vector2(relativeToCenterX, relativeToCenterY);
            this.Position = position;
            this.EquippedItems = new Equipment[width, height];

            //TriggerLink[] allTriggers = GetComponentsInChildren<TriggerLink>();

            //this.primaryFire = allTriggers[0];
            this.primaryFire = ScriptableObject.CreateInstance<TriggerLink>();
            this.primaryFire.Init(TriggerLink.Type.Primary);

            //this.secondaryFire = allTriggers[1];
            this.secondaryFire = ScriptableObject.CreateInstance<TriggerLink>();
            this.secondaryFire.Init(TriggerLink.Type.Secondary);

            //this.specialFire = allTriggers[2];
            this.specialFire = ScriptableObject.CreateInstance<TriggerLink>();
            this.specialFire.Init(TriggerLink.Type.Special);
        }

        public void Init(Transform parent, int relativeToCenterX, int relativeToCenterY, int width, int height, HardpointPosition position)
        {
            this.transform.parent = parent;
            Init(relativeToCenterX, relativeToCenterY, width, height, position);
        }

        public bool Contains(Equipment item)
        {
            foreach (Equipment itemToCheck in EquippedItems)
            {
                if (itemToCheck != item)
                {
                    continue;
                }

                return true;
            }

            return false;
        }

        public bool IsEquippable(Equipment item, int row, int column)
        {
            int itemWidth = item.Width;
            if (itemWidth > this.Width)
            {
                return false;
            }

            int itemHeight = item.Height;
            if (itemHeight > this.Height)
            {
                return false;
            }

            for (int x = column; x < itemWidth; x++)
            {
                for (int y = row; y < itemHeight; y++)
                {
                    if (EquippedItems[x, y] != null)
                    {
                        return false;
                    }
                }
            }

            return true;
        }

        public void Equip(Equipment item, int row, int column, Raider parent)
        {
            int itemWidth = item.Width;
            int itemHeight = item.Height;

            for (int x = column; x < itemWidth; x++)
            {
                for (int y = row; y < itemHeight; y++)
                {
                    EquippedItems[x, y] = item;
                }
            }

            if (item.IsWeapon == false)
            {
                return;
            }

            // Set Parent of weapon
            Weapon weapon = (Weapon)item;
            weapon.Parent = parent;

            LinkedList<Weapon> weaponType;
            bool hasType = this.allWeaponTypes.ContainsKey(weapon.Type);
            if (hasType == false)
            {
                weaponType = new LinkedList<Weapon>();
                this.allWeaponTypes.Add(weapon.Type, weaponType);
            }
            else
            {
                weaponType = this.allWeaponTypes[weapon.Type];
            }

            weaponType.AddLast(weapon);
        }

        public Equipment Unequip(int row, int column)
        {
            Equipment item = EquippedItems[row, column];
            bool isRemovable = item.IsRemovable;
            if (isRemovable == false)
            {
                return null;
            }

            for (int x = 0; x < this.Width; x++)
            {
                for (int y = 0; y < this.Height; y++)
                {
                    Equipment itemToCheck = EquippedItems[x, y];
                    if (itemToCheck.Equals(item) == false)
                    {
                        continue;
                    }

                    EquippedItems[x, y] = null;
                }
            }

            if(item.IsWeapon == true)
            {
                Weapon weapon = (Weapon)item;

                bool hasType = this.allWeaponTypes.ContainsKey(weapon.Type);
                if (hasType == true)
                {
                    LinkedList<Weapon> weaponType = this.allWeaponTypes[weapon.Type];
                    weaponType.Remove(weapon);
                }
            }

            return item;
        }

        public bool LinkWeapon(Weapon weapon, Vector3 firingPoint, TriggerLink.Type type)
        {
            TriggerLink trigger = null;

            switch (type)
            {
                case TriggerLink.Type.Primary:
                    trigger = primaryFire;
                    break;

                case TriggerLink.Type.Secondary:
                    trigger = secondaryFire;
                    break;

                case TriggerLink.Type.Special:
                    trigger = specialFire;
                    break;

                default:
                    return false;
            }

            trigger.LinkWeapon(weapon, firingPoint);
            return true;
        }

        public void ReadyWeapons()
        {
            if(allWeaponTypes.Count == 0)
            {
                return;
            }

            LinkedList<Weapon> sortedWeapons = SetFiringOrder();
            SetAngledSpread();
            SetHorizontalSpread();
            SetGroupingBonus();

            primaryFire.ReadyWeapons(sortedWeapons, DominantWeaponType, AngledSpread, HorizontalSpread, GroupingBonus);
            secondaryFire.ReadyWeapons(sortedWeapons, DominantWeaponType, AngledSpread, HorizontalSpread, GroupingBonus);
            specialFire.ReadyWeapons(sortedWeapons, DominantWeaponType, AngledSpread, HorizontalSpread, GroupingBonus);
        }

        protected LinkedList<Weapon> SetFiringOrder()
        {
            SortedList<int, string> firingOrder = new SortedList<int, string>();

            foreach (string type in allWeaponTypes.Keys)
            {
                GameObject prefab = GlobalAssets.GetWeaponPrefab(type);
                Weapon weapon = prefab.GetComponent<Weapon>();
                int priority = weapon.Priority;
                int quantity = this.allWeaponTypes[type].Count;

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

                LinkedList<Weapon> weaponType = allWeaponTypes[type];
                foreach (Weapon weapon in weaponType)
                {
                    sortedWeapons.AddLast(weapon);
                }
            }

            return sortedWeapons;
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

            int quantity = allWeaponTypes[Weapon.Types.BOLT].Count;
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

            int quantity = allWeaponTypes[Weapon.Types.FLAMEBURST].Count;
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

            int quantity = allWeaponTypes[Weapon.Types.WAVE].Count;
            this.GroupingBonus = quantity;
        }

        public void SetPrimaryFire(bool isFiring)
        {
            primaryFire.IsFiring = isFiring;
        }

        public void SetSecondaryFire(bool isFiring)
        {
            secondaryFire.IsFiring = isFiring;
        }

        public void SetSpecialFire(bool isFiring)
        {
            specialFire.IsFiring = isFiring;
        }
    }
}