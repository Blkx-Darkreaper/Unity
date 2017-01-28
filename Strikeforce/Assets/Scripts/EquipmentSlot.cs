using UnityEngine;
using System.Drawing;

namespace Strikeforce
{
    public class EquipmentSlot : MonoBehaviour
    {
        public Vector2 Location { get; protected set; }
        public int Width { get { return EquippedItems.GetLength(1); } }
        public int Height { get { return EquippedItems.GetLength(0); } }
		public Equipment[,] EquippedItems { get; protected set; }
		
		public EquipmentSlot(int x, int y, int width, int height) {
			this.Location = new Vector2(x, y);
			this.EquippedItems = new Equipment[width, height];
		}

		public bool IsEquippable(Equipment item, int row, int column) {
			Size slotSize = item.SlotSize;
			int itemWidth = slotSize.Width;
            if (itemWidth > this.Width)
            {
                return false;
            }

			int itemHeight = slotSize.Height;
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

        public void Equip(Equipment item, int row, int column)
        {
            Size slotSize = item.SlotSize;
			int itemWidth = slotSize.Width;
            int itemHeight = slotSize.Height;

            for(int x = column; x < itemWidth; x++) {
                for (int y = row; y < itemHeight; y++)
                {
                    EquippedItems[x, y] = item;
                }
            }
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
    }
}