using UnityEngine;
using System.Collections;

namespace Strikeforce
{
    public class AttackBehaviour : MonoBehaviour
    {
        protected Entity slave { get; set; }
        public float WeaponRange = 10f;
        private const float DEFAULT_WEAPON_RANGE = 10f;
        public float WeaponAimSpeed = 1f;
        protected const float DEFAULT_WEAPON_AIM_SPEED = 1f;
        public float WeaponCooldown = 1f;
        protected const float DEFAULT_WEAPON_COOLDOWN = 1f;
        protected float currentCooldownRemaining { get; set; }
        protected bool isAttacking { get; set; }
        protected bool isAdvancing { get; set; }
        protected bool isAiming { get; set; }
        protected Entity attackTarget { get; set; }
        protected int attackTargetId { get; set; }
        protected struct BehaviourProperties
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

            AttackTarget(attackTarget);
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

        protected void SetDefaultWeaponRange()
        {
            if (WeaponRange > 0)
            {
                return;
            }

            WeaponRange = DEFAULT_WEAPON_RANGE;
        }

        protected void SetDefaultWeaponAimSpeed()
        {
            if (WeaponAimSpeed > 0)
            {
                return;
            }

            WeaponAimSpeed = DEFAULT_WEAPON_AIM_SPEED;
        }

        protected void SetDefaultWeaponCooldown()
        {
            if (WeaponCooldown > 0)
            {
                return;
            }

            WeaponCooldown = DEFAULT_WEAPON_COOLDOWN;
        }

        protected virtual void SetAttackTarget(Entity target)
        {
            attackTarget = target;

            Vector3 targetPosition = attackTarget.transform.position;
            bool targetInRange = IsTargetInRange(targetPosition);
            if (targetInRange == true)
            {
                isAttacking = true;
                AttackTarget(attackTarget);
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

        protected bool IsTargetInRange(Vector3 targetPosition)
        {

            Vector3 bearingToTarget = targetPosition - transform.position;
            float weaponsRangeSquared = WeaponRange * WeaponRange;
            if (bearingToTarget.sqrMagnitude < weaponsRangeSquared)
            {
                return true;
            }

            return false;
        }

        protected void AttackTarget(Entity target)
        {
            if (target == null)
            {
                isAttacking = false;
                return;
            }

            Vector3 targetPosition = target.transform.position;
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

            bool readyToEngage = IsReadyToAttack();
            if (readyToEngage == false)
            {
                return;
            }

            FireAtTarget(target);
        }

        protected bool IsTargetInSights(Vector3 targetPosition)
        {
            Vector3 bearingToTarget = targetPosition - transform.position;
            if (bearingToTarget.normalized == transform.forward.normalized)
            {
                return true;
            }

            return false;
        }

        protected bool IsReadyToAttack()
        {
            if (currentCooldownRemaining <= 0)
            {
                return true;
            }

            return false;
        }

        protected virtual void FireAtTarget(Entity target)
        {
            currentCooldownRemaining = WeaponCooldown;

            string ownersName = "Neutral";
            string attackTargetsOwnersName = ownersName;
            Selectable unit = slave as Selectable;
            if (unit.currentOwner != null)
            {
                ownersName = unit.currentOwner.playerId.ToString();
            }

            Selectable attackTarget = target as Selectable;
            if (attackTarget == null)
            {
                Debug.Log(string.Format("{0} {1} fired at {2} {3}", ownersName, name, attackTargetsOwnersName, attackTarget.name));
                return;
            }

            if (attackTarget.currentOwner != null)
            {
                attackTargetsOwnersName = attackTarget.currentOwner.playerId.ToString();
            }

            Debug.Log(string.Format("{0} {1} fired at {2} {3}", ownersName, name, attackTargetsOwnersName, attackTarget.name));
        }

        protected virtual void GetBearingToTarget(Vector3 targetPosition)
        {
            isAiming = true;
        }

        protected void AdvanceTowardsTarget(Vector3 targetPosition)
        {
            Selectable unit = this.slave as Selectable;
            if (unit == null)
            {
                isAttacking = false;
                return;
            }

            isAdvancing = true;
            Vector3 attackPosition = GetNearestAttackPosition(targetPosition);

            _Targeting targeting = unit.gameObject.GetComponent<_Targeting>();
            if (targeting == null)
            {
                Debug.Log(string.Format("No targeting on {0} {1}. Unable to attack", unit.name, unit.entityId));
                return;
            }

            targeting.SetWaypoint(attackPosition);
            isAttacking = true;
        }

        protected Vector3 GetNearestAttackPosition(Vector3 targetPosition)
        {
            Vector3 bearingToTarget = targetPosition - transform.position;
            float distanceToTarget = bearingToTarget.magnitude;
            float distanceToTravel = distanceToTarget - (0.9f * WeaponRange); // Move in slightly closer than weapon's range
            Vector3 attackPosition = Vector3.Lerp(transform.position, targetPosition, distanceToTravel / distanceToTarget);
            return attackPosition;
        }
    }
}