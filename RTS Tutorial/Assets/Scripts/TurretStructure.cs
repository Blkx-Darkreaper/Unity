﻿using UnityEngine;
using System.Collections;
using RTS;

public class TurretStructure : StructureController {

    protected Quaternion targetBearing;
    public string projectileName;

    protected override void Update()
    {
        base.Update();

        AimAtTarget();
    }

    protected void AimAtTarget()
    {
        if (isAiming == false)
        {
            return;
        }

        transform.rotation = Quaternion.RotateTowards(transform.rotation, targetBearing, weaponAimSpeed);
        Quaternion inverseTargetRotation = new Quaternion(-targetBearing.w, -targetBearing.x, -targetBearing.y, -targetBearing.z);
        UpdateBounds();

        if (transform.rotation != targetBearing)
        {
            if (transform.rotation != inverseTargetRotation)
            {
                return;
            }
        }

        isAiming = false;
    }

    public override bool IsAbleToAttack()
    {
        if (isConstructionComplete == false)
        {
            return false;
        }
        if (currentHitPoints == 0)
        {
            return false;
        }

        return true;
    }

    protected override void FireWeaponAtTarget(EntityController attackTarget)
    {
        base.FireWeaponAtTarget(attackTarget);

        Vector3 projectileSpawnPoint = transform.position;
        projectileSpawnPoint.x += (2.6f * transform.forward.x);
        projectileSpawnPoint.y += 1f;
        projectileSpawnPoint.z += (2.6f * transform.forward.z);

        GameObject gameObject = (GameObject)Instantiate(GameManager.activeInstance.GetEntityPrefab(projectileName),
            projectileSpawnPoint, transform.rotation);

        ProjectileController projectile = gameObject.GetComponent<ProjectileController>();
        projectile.currentRangeToTarget = 0.9f * weaponRange;
        projectile.target = attackTarget;
    }

    protected override void GetBearingToTarget(Vector3 targetPosition)
    {
        base.GetBearingToTarget(targetPosition);

        targetBearing = Quaternion.LookRotation(targetPosition - transform.position);
    }
}