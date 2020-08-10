using UnityEngine;

namespace Strikeforce
{
    public class Projectile : Destructible
    {
        public int damage;
        public bool isFriendly;
        public bool isAntiAir;
        public bool isAntiGround;
        public bool isIndestructible;
        public float currentRangeToTarget { get; set; }
        public Entity target { get; set; }
        public struct Type
        {

        }

        public bool CanHitTarget(Destructible target)
        {
            bool canHit = false;

            if (target.isAirborne == false)
            {
                canHit = isAntiGround;
            }
            else
            {
                canHit = isAntiAir;
            }

            if (canHit == false)
            {
                return false;
            }

            if (isFriendly == true)
            {
                Raider raider = target.transform.GetComponent<Raider>();
                if (raider != null)
                {
                    return false;
                }
            }

            return true;
        }

        protected virtual bool HasHitTarget(Entity target)
        {
            return false;
        }

        public override void TakeDamage(int amount, RaycastHit hit)
        {
            if (isIndestructible == true)
            {
                return;
            }

            base.TakeDamage(amount, hit);
        }

        protected virtual void DealDamageToTarget(Destructible target, RaycastHit hit)
        {
            bool anti = false;

            if (this.isAirborne == false)
            {
                anti = isAntiGround;
            }
            else
            {
                anti = isAntiAir;
            }

            if (anti == false)
            {
                return;
            }

            if (isFriendly == true)
            {
                Raider raider = target.transform.GetComponent<Raider>();
                if (raider != null)
                {
                    return;
                }
            }

            target.TakeDamage(damage, hit);
        }

        protected virtual void OnCollisionEnter(Collision collision)
        {
            GameObject hitObject = collision.gameObject;
            Destructible hitDestructible = hitObject.GetComponent<Destructible>();
            if (hitDestructible == null)
            {
                return;
            }

            bool canHit = CanHitTarget(hitDestructible);
            if (canHit == false)
            {
                return;
            }

            RaycastHit hit = new RaycastHit();
            hit.transform.position = collision.transform.position;

            this.TakeDamage(damage, hit);
            hitDestructible.TakeDamage(damage, hit);
        }
    }
}