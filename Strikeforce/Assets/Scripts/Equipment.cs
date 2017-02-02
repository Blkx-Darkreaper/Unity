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
        public string CurrentStatus { get; protected set; }
        public static struct EquipmentStatus
        {
            public const string READY = "Ready";
            public const string RECHARGING = "Recharging";
            public const string DISABLED = "Disabled";
        }

        public Equipment(int id, Raider parent, int width, int height) : this(id, width, height)
        {
            this.parent = parent;
        }
		
		public Equipment(int id, int width, int height) {
            this.Id = id;
			this.SlotSize = new Size(width, height);
            this.CurrentStatus = EquipmentStatus.READY;
		}

        public void Equip(Raider parent, HardpointPosition hardpoint)
        {
            this.EquippedHardpoint = hardpoint;
        }
    }
}