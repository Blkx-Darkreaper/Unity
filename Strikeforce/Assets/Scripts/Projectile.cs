using UnityEngine;

namespace Strikeforce
{
    public class Projectile : Destructible
    {
        public int Damage { get; set; }
        public float CurrentRangeToTarget { get; set; }
        public Entity Target { get; set; }

        protected override void Update() { }

        protected bool HasHitTarget(Entity target) {
            return false;
        }

        private void DealDamageToTarget(Entity target) { }
    }
}