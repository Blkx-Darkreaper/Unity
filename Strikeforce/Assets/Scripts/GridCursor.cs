using UnityEngine;

namespace Strikeforce
{
    public class GridCursor : Entity
    {
        protected override void Awake()
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

            int signX = (int)Mathf.Sign(x);
            x = signX;

            int signZ = (int)Mathf.Sign(z);
            z = signZ;

            CurrentLevel.KeepInBounds(currentPosition.x, currentPosition.z, ref x, ref z);

            transform.Translate(x, 0, z);
        }

        public virtual void DefaultSize()
        {
            transform.localScale = new Vector3(1, 1, 1);
        }

        public virtual void SetSize(Vector2 size)
        {
            float height = transform.localScale.y;
            transform.localScale = new Vector3(size.x, height, size.y);
        }
    }
}