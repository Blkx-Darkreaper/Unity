
namespace Strikeforce
{
    public class KeyEvent
    {
        public ActionKey Key { get; protected set; }
        public Type EventType { get; protected set; }
        public float PressedTime { get; protected set; }
        public float? ReleasedTime { get; set; }
        public bool IsComplete { get; protected set; }
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
        public enum Type { Pressed, DoubleTapped, Held }

        public KeyEvent(ActionKey key, Type type, float pressedTime)
        {
            this.Key = key;
            this.EventType = type;
            this.PressedTime = pressedTime;
            this.IsComplete = false;
        }

        public override string ToString()
        {
            string output = string.Format("{0}", Key.ToString());
            return output;
        }

        public void Release(float releasedTime, float minHoldDuration)
        {
            this.ReleasedTime = releasedTime;
            this.IsComplete = true;

            if(HoldDuration < minHoldDuration)
            {
                return;
            }

            this.EventType = Type.Held;
        }

        public void DoubleTap(float previousPressedTime, float previousReleasedTime)
        {
            this.EventType = Type.DoubleTapped;
        }
    }
}
