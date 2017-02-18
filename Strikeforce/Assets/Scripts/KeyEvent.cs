
namespace Strikeforce
{
    public class KeyEvent
    {
        public ActionKey Key { get; protected set; }
        public Type EventType { get; protected set; }
        public float HoldDuration { get; protected set; }
        public enum Type { Pressed, Held, Released }

        public KeyEvent(ActionKey key, Type type)
        {
            this.Key = key;
            this.EventType = type;
        }

        public KeyEvent(ActionKey key, float duration)
            : this(key, Type.Released)
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
