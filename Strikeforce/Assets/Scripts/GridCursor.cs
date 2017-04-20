using UnityEngine;

namespace Strikeforce
{
    public class GridCursor : Entity
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

            int signX = Mathf.Sign(x);
            //x = signX * Level.TileLength;
            x = signX;

            int signZ = Mathf.Sign(z);
            //z = signZ * Level.TileLength;
            z = signZ;

            CurrentLevel.KeepInBounds(currentPosition.x, currentPosition.z, ref x, ref z);

            transform.Translate(x, 0, z);
        }
    }
}