using UnityEngine;
using System.Drawing;

namespace Strikeforce
{
    public class Vehicle : Destructible
    {
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
            Level currentLevel = GameManager.Singleton.CurrentLevel;

            currentLevel.KeepInBounds(currentPosition.x, currentPosition.z, ref x, ref z);

            transform.Translate(x, 0, z);
        }
    }
}