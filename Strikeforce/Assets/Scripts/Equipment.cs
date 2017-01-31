using UnityEngine;
using System.Drawing;
using System.Collections;

namespace Strikeforce
{
    public class Equipment : MonoBehaviour
    {
		public Size SlotSize { get; set; }
        public bool IsRemovable;
        public bool IsWeapon = false;
        public float Cost;
        public int Level;
        public float EnergyCost;
        public float Cooldown;
		
		public Equipment(int width, int height) {
			this.SlotSize = new Size(width, height);
		}
    }
}