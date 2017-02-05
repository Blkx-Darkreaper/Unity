using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Strikeforce
{
    public class KeyEvent
    {
        public ActionKey Key { get; protected set; }
        public bool IsBeingHeld { get; protected set; }
        public float HoldDuration { get; protected set; }

        public KeyEvent(ActionKey key, bool isBeingHeld)
        {
            this.Key = key;
            this.IsBeingHeld = isBeingHeld;
        }

        public KeyEvent(ActionKey key, float duration)
            : this(key, false)
        {
            this.HoldDuration = duration;
        }

        public override string ToString()
        {
            string output = string.Format("{0}", Key.ToString());
            return output;
        }
    }
}
