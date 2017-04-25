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
        public struct Type
        {

        }

        protected override void Update() { }

        protected virtual bool HasHitTarget(Entity target)
        {
            return false;
        }

        public override void TakeDamage(int amount, RaycastHit hit)
        {
            if (IsIndestructible == true)
            {
                return;
            }

            base.TakeDamage(amount, hit);
        }

        protected virtual void DealDamageToTarget(Destructible target, RaycastHit hit)
        {
            bool anti = false;

            if (target.IsAirborne == false)
            {
                anti = IsAntiGround;
            }
            else
            {
                anti = IsAntiAir;
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

            target.TakeDamage(Damage, hit);
        }

        protected virtual void OnCollisionEnter(Collision collision)
        {
            GameObject hitObject = collision.gameObject;
            Destructible hitDestructible = hitObject.GetComponent<Destructible>();
            if (hitDestructible == null)
            {
                return;
            }

            RaycastHit hit = new RaycastHit();
            hit.transform.position = collision.transform.position;

            this.TakeDamage(Damage, hit);
            hitDestructible.TakeDamage(Damage, hit);
        }
    }
}