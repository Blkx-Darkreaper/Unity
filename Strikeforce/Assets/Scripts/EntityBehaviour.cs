using UnityEngine;
using System.Collections;

namespace Strikeforce
{
    public class EntityBehaviour : MonoBehaviour
    {
        protected Entity slave { get; set; }
        public float weaponRange = 10f;
        private const float DEFAULT_WEAPON_RANGE = 10f;
        public float weaponAimSpeed = 1f;
        protected const float DEFAULT_WEAPON_AIM_SPEED = 1f;
        public float weaponCooldown = 1f;
        protected const float DEFAULT_WEAPON_COOLDOWN = 1f;
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

        protected void SetDefaultWeaponRange()
        {
            if (weaponRange > 0)
            {
                return;
            }

            weaponRange = DEFAULT_WEAPON_RANGE;
        }

        protected void SetDefaultWeaponAimSpeed()
        {
            if (weaponAimSpeed > 0)
            {
                return;
            }

            weaponAimSpeed = DEFAULT_WEAPON_AIM_SPEED;
        }

        protected void SetDefaultWeaponCooldown()
        {
            if (weaponCooldown > 0)
            {
                return;
            }

            weaponCooldown = DEFAULT_WEAPON_COOLDOWN;
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

        protected bool IsTargetInRange(Vector3 targetPosition)
        {

            Vector3 bearingToTarget = targetPosition - transform.position;
            float weaponsRangeSquared = weaponRange * weaponRange;
            if (bearingToTarget.sqrMagnitude < weaponsRangeSquared)
            {
                return true;
            }

            return false;
        }

        protected void AttackTarget(Entity attackTarget)
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

        protected bool IsTargetInSights(Vector3 targetPosition)
        {
            Vector3 bearingToTarget = targetPosition - transform.position;
            if (bearingToTarget.normalized == transform.forward.normalized)
            {
                return true;
            }

            return false;
        }

        protected bool IsReadyToFire()
        {
            if (currentCooldownRemaining <= 0)
            {
                return true;
            }

            return false;
        }

        protected virtual void FireWeaponAtTarget(Entity target)
        {
            currentCooldownRemaining = weaponCooldown;

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
                this.isAttacking = false;
                return;
            }

            this.isAdvancing = true;

            _Targeting targeting = gameObject.GetComponent<_Targeting>();
            if(targeting == null)
            {
                Debug.Log(string.Format("No targeting on {0} {1}. Unable to attack", slave.name, slave.entityId));
                return;
            }

            Vector3 attackPosition = GetNearestAttackPosition(targetPosition);
            targeting.SetWaypoint(attackPosition);
            this.isAttacking = true;
        }

        protected Vector3 GetNearestAttackPosition(Vector3 targetPosition)
        {
            Vector3 bearingToTarget = targetPosition - transform.position;
            float distanceToTarget = bearingToTarget.magnitude;
            float distanceToTravel = distanceToTarget - (0.9f * weaponRange); // Move in slightly closer than weapon's range
            Vector3 attackPosition = Vector3.Lerp(transform.position, targetPosition, distanceToTravel / distanceToTarget);
            return attackPosition;
        }

        protected void ReturnToBase()
        {
            Vehicle vehicle = (Vehicle)slave;
            if(vehicle == null)
            {
                return;
            }

            float distanceTravelled = vehicle.DistanceTravelled;
            float range = vehicle.Range;
            if(distanceTravelled < range)
            {
                return;
            }

            float fuel = vehicle.FuelRemaining;
            if(fuel > 0)
            {
                return;
            }

            vehicle.AddWaypoint(vehicle.HomeBase.rallyPoint);
        }
    }
}