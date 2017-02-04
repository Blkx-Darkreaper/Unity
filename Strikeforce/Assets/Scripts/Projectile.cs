using UnityEngine;

namespace Strikeforce
{
    public class Projectile : Destructible
    {
        public int Damage;
        public bool IsFriendly;
        public bool IsAntiAir;
        public bool IsAntiGround;
        public bool IsIndestructible;
        public float CurrentRangeToTarget { get; set; }
        public Entity Target { get; set; }

        protected override void Update() { }

        protected virtual bool HasHitTarget(Entity target) {
            return false;
        }

        public override void TakeDamage(int damage)
        {
            if (IsIndestructible == true)
            {
                return;
            }

            base.TakeDamage(damage);
        }

        protected virtual void DealDamageToTarget(Destructible target) {
            bool anti = false;

            switch (target.CurrentOrbit)
            {
                case Orbit.Ground:
                    anti = IsAntiGround;
                    break;

                case Orbit.Air:
                    anti = IsAntiAir;
                    break;
            }

            if (anti == false)
            {
                return;
            }

            if (IsFriendly == true)
            {
                Raider raider = target.transform.GetComponent<Raider>();
                if (raider != null)
                {
                    return;
                }
            }

            target.TakeDamage(Damage);
        }

        protected virtual void OnCollisionEnter(Collision collision)
        {
            GameObject hitObject = collision.gameObject;
            Destructible hitDestructible = hitObject.GetComponent<Destructible>();
            if (hitDestructible == null)
            {
                return;
            }

            this.TakeDamage(Damage);
            hitDestructible.TakeDamage(Damage);
        }
    }
}