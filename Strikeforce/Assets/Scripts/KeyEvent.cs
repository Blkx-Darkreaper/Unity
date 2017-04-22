
namespace Strikeforce
{
    public class KeyEvent
    {
        public ActionKey Key { get; protected set; }
        public Type EventType { get; protected set; }
        public float PressedTime { get; protected set; }
        public float? ReleasedTime { get; set; }
        public float HoldDuration
        {
            get
            {
                if (ReleasedTime == null)
                {
                    return 0;
                }

                float duration = (float)ReleasedTime - PressedTime;
                return duration;
            }
        }
        public enum Type { Pressed, DoubleTapped, Held, Released }

        public KeyEvent(ActionKey key, Type type, float pressedTime)
        {
            this.Key = key;
            this.EventType = type;
            this.PressedTime = pressedTime;
        }

        public override string ToString()
        {
            string output = string.Format("{0}", Key.ToString());
            return output;
        }

        public void Release(float releasedTime)
        {
            this.ReleasedTime = releasedTime;
            this.EventType = Type.Released;
        }

        public void DoubleTap(float secondPressedTime, float? secondReleasedTime)
        {
            this.EventType = Type.DoubleTapped;
        }
    }
}
