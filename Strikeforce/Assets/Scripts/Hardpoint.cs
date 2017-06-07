using UnityEngine;
using System.Drawing;

namespace Strikeforce
{
    public enum HardpointPosition { LeftOuterWing, LeftWing, Center, RightWing, RightOuterWing }

    public class Hardpoint
    {
        public Vector2 Location;
        public HardpointPosition Position { get; protected set; }
        public static Size PixelsPerSlot = new Size(42, 42);
        public int Width { get { return EquippedItems.GetLength(1); } }
        public int Height { get { return EquippedItems.GetLength(0); } }
        public Equipment[,] EquippedItems { get; protected set; }
        protected TriggerLink primaryFire;
        protected TriggerLink secondaryFire;
        protected TriggerLink specialFire;

        public Hardpoint(int relativeToCenterX, int relativeToCenterY, int width, int height, HardpointPosition position)
        {
            this.Location = new Vector2(relativeToCenterX, relativeToCenterY);
            this.Position = position;
            this.EquippedItems = new Equipment[width, height];
            this.primaryFire = new TriggerLink(TriggerLink.Type.Primary);
            this.secondaryFire = new TriggerLink(TriggerLink.Type.Secondary);
            this.specialFire = new TriggerLink(TriggerLink.Type.Special);
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

            // Set Parent
            item.Parent = parent;
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
            primaryFire.ReadyWeapons();
            secondaryFire.ReadyWeapons();
            specialFire.ReadyWeapons();
        }

        public void Update()
        {
            primaryFire.Update();
            secondaryFire.Update();
            specialFire.Update();
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