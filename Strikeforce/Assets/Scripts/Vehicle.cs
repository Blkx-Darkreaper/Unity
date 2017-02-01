using UnityEngine;

namespace Strikeforce
{
    public class Vehicle : Destructible
    {
        public void Move(float x, float z)
        {
            transform.Translate(x, 0, z);
        }
    }
}