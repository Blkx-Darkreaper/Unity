using UnityEngine;
using System.Drawing;
using System.Collections;

namespace Strikeforce
{
    public class Equipment : MonoBehaviour
    {
        public int Id { get; protected set; }
		public Size SlotSize { get; set; }
        public bool IsRemovable;
        public bool IsWeapon = false;
        protected Raider parent { get; set; }
        public HardpointPosition EquippedHardpoint { get; protected set; }
        public float Cost;
        public int Level;
        public float EnergyCost;
        public float Cooldown;

        public Equipment(int id, Raider parent, int width, int height) : this(id, width, height)
        {
            this.parent = parent;
        }
		
		public Equipment(int id, int width, int height) {
            this.Id = id;
			this.SlotSize = new Size(width, height);
		}

        public void Equip(Raider parent, HardpointPosition hardpoint)
        {
            this.EquippedHardpoint = hardpoint;
        }
    }
}