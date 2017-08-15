using UnityEngine;
using UnityEngine.Networking;

namespace Strikeforce
{
    public class Armour : Equipment
    {
        public int MaxHitPoints;
        [HideInInspector] [SyncVar] public int CurrentHitPoints;

        public virtual void TakeDamage(int damage)
        {
            if (isServer == false)
            {
                return;
            }

            CurrentHitPoints -= damage;

            Debug.Log(string.Format("{0} has taken {1} damage", name, damage));
        }

        public virtual void DestroyArmour()
        {
            GameManager.Singleton.RemoveEntity(this);

            Debug.Log(string.Format("{0} has been destroyed", name));
        }
    }
}