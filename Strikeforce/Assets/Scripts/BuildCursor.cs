using UnityEngine;

namespace Strikeforce
{
    public class BuildCursor : Entity
    {
        protected virtual void Awake()
        {
            base.Awake();
        }

        public virtual void Move(float x, float z)
        {
            if (x == 0f)
            {
                if (z == 0f)
                {
                    return;
                }
            }

            Vector3 currentPosition = transform.position;

            CurrentLevel.KeepInBounds(currentPosition.x, currentPosition.z, ref x, ref z);

            transform.Translate(x, 0, z);
        }
    }
}