using UnityEngine;
using System.Drawing;
using System.Collections;

namespace Strikeforce
{
    public class Equipment : MonoBehaviour
    {
		public Size SlotSize { get; set; }
        public bool IsRemovable { get; protected set; }
		
		public Equipment(int width, int height) {
			this.SlotSize = new Size(width, height);
		}
    }
}