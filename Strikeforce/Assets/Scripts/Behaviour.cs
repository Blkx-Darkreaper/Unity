using UnityEngine;
using System.Collections;

namespace Strikeforce
{
    public class Behaviour : MonoBehaviour
    {
        protected Entity slave { get; set; }
        public float WeaponRange = 10f;
        private const float DEFAULT_WEAPON_RANGE = 10f;
        public float WeaponAimSpeed = 1f;
        private const float DEFAULT_WEAPON_AIM_SPEED = 1f;
        public float WeaponCooldown = 1f;
        private const float DEFAULT_WEAPON_COOLDOWN = 1f;
        protected float currentCooldownRemaining { get; set; }
        protected bool isAttacking { get; set; }
        protected bool isAdvancing { get; set; }
        protected bool isAiming { get; set; }
        protected Entity actionTarget { get; set; }
        protected int actionTargetId { get; set; }
        protected struct SelectableProperties
        {
            public const string IS_ATTACKING = "IsAttacking";
            public const string IS_ADVANCING = "IsAdvancing";
            public const string IS_AIMING = "IsAiming";
            public const string COOLDOWN = "CurrentCooldown";
            public const string TARGET_ID = "TargetId";
        }

        protected virtual void Start()
        {
            SetWeaponDefaults();
        }

        protected virtual void Update()
        {
            currentCooldownRemaining -= Time.deltaTime;
            currentCooldownRemaining = Mathf.Clamp(currentCooldownRemaining, 0f, float.MaxValue);

            if (isAttacking == false)
            {
                return;
            }
            if (isAdvancing == true)
            {
                return;
            }
            if (isAiming == true)
            {
                return;
            }

            AttackTarget(actionTarget);
        }

        protected virtual void SetWeaponDefaults()
        {
            bool readyToAttack = IsAbleToAttack();
            if (readyToAttack == false)
            {
                return;
            }

            SetDefaultWeaponRange();
            SetDefaultWeaponAimSpeed();
            SetDefaultWeaponCooldown();
        }

        private void SetDefaultWeaponRange()
        {
            if (WeaponRange > 0)
            {
                return;
            }

            WeaponRange = DEFAULT_WEAPON_RANGE;
        }

        private void SetDefaultWeaponAimSpeed()
        {
            if (WeaponAimSpeed > 0)
            {
                return;
            }

            WeaponAimSpeed = DEFAULT_WEAPON_AIM_SPEED;
        }

        private void SetDefaultWeaponCooldown()
        {
            if (WeaponCooldown > 0)
            {
                return;
            }

            WeaponCooldown = DEFAULT_WEAPON_COOLDOWN;
        }

        protected virtual void SetAttackTarget(Entity target)
        {
            actionTarget = target;

            Vector3 targetPosition = actionTarget.transform.position;
            bool targetInRange = IsTargetInRange(targetPosition);
            if (targetInRange == true)
            {
                isAttacking = true;
                AttackTarget(actionTarget);
            }
            else
            {
                AdvanceTowardsTarget(targetPosition);
            }
        }

        public virtual bool IsAbleToAttack()
        {
            return false;
        }

        private bool IsTargetInRange(Vector3 targetPosition)
        {

            Vector3 bearingToTarget = targetPosition - transform.position;
            float weaponsRangeSquared = WeaponRange * WeaponRange;
            if (bearingToTarget.sqrMagnitude < weaponsRangeSquared)
            {
                return true;
            }

            return false;
        }

        private void AttackTarget(Entity attackTarget)
        {
            if (attackTarget == null)
            {
                isAttacking = false;
                return;
            }

            Vector3 targetPosition = attackTarget.transform.position;
            bool targetInRange = IsTargetInRange(targetPosition);
            if (targetInRange == false)
            {
                AdvanceTowardsTarget(targetPosition);
                return;
            }

            bool targetInSights = IsTargetInSights(targetPosition);
            if (targetInSights == false)
            {
                GetBearingToTarget(targetPosition);
                return;
            }

            bool readyToFire = IsReadyToFire();
            if (readyToFire == false)
            {
                return;
            }

            FireWeaponAtTarget(attackTarget);
        }

        private bool IsTargetInSights(Vector3 targetPosition)
        {
            Vector3 bearingToTarget = targetPosition - transform.position;
            if (bearingToTarget.normalized == transform.forward.normalized)
            {
                return true;
            }

            return false;
        }

        private bool IsReadyToFire()
        {
            if (currentCooldownRemaining <= 0)
            {
                return true;
            }

            return false;
        }

        protected virtual void FireWeaponAtTarget(Entity target)
        {
            currentCooldownRemaining = WeaponCooldown;

            string ownersName = "Neutral";
            string attackTargetsOwnersName = ownersName;
            Selectable unit = slave as Selectable;
            if (unit.Owner != null)
            {
                ownersName = unit.Owner.PlayerId.ToString();
            }

            Selectable attackTarget = target as Selectable;
            if (attackTarget == null)
            {
                Debug.Log(string.Format("{0} {1} fired at {2} {3}", ownersName, name, attackTargetsOwnersName, attackTarget.name));
                return;
            }

            if (attackTarget.Owner != null)
            {
                attackTargetsOwnersName = attackTarget.Owner.PlayerId.ToString();
            }

            Debug.Log(string.Format("{0} {1} fired at {2} {3}", ownersName, name, attackTargetsOwnersName, attackTarget.name));
        }

        protected virtual void GetBearingToTarget(Vector3 targetPosition)
        {
            isAiming = true;
        }

        private void AdvanceTowardsTarget(Vector3 targetPosition)
        {
            Selectable unit = this.slave as Selectable;
            if (unit == null)
            {
                isAttacking = false;
                return;
            }

            isAdvancing = true;
            Vector3 attackPosition = GetNearestAttackPosition(targetPosition);
            unit.SetWaypoint(attackPosition);
            isAttacking = true;
        }

        private Vector3 GetNearestAttackPosition(Vector3 targetPosition)
        {
            Vector3 bearingToTarget = targetPosition - transform.position;
            float distanceToTarget = bearingToTarget.magnitude;
            float distanceToTravel = distanceToTarget - (0.9f * WeaponRange); // Move in slightly closer than weapon's range
            Vector3 attackPosition = Vector3.Lerp(transform.position, targetPosition, distanceToTravel / distanceToTarget);
            return attackPosition;
        }
    }
}