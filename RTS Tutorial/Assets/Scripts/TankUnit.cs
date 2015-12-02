using UnityEngine;
using System.Collections;
using RTS;
using Newtonsoft.Json;

public class TankUnit : UnitController {

    private Quaternion targetBearing;
    public string projectileName;
    protected struct TankProperties
    {
        public const string TARGET_BEARING = "TargetBearing";
    }

    protected override void Update()
    {
        base.Update();

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
        return true;
    }

    protected override void AimTowardsTarget(Vector3 targetPosition)
    {
        base.AimTowardsTarget(targetPosition);

        Vector3 bearingToTarget = targetPosition - transform.position;
        targetBearing = Quaternion.LookRotation(bearingToTarget);
    }

    protected override void FireWeaponAtTarget(EntityController attackTarget)
    {
        base.FireWeaponAtTarget(attackTarget);

        Vector3 projectileSpawnPoint = transform.position;
        projectileSpawnPoint.x += (2.1f * transform.forward.x);
        projectileSpawnPoint.y += 1.4f;
        projectileSpawnPoint.z += (2.1f * transform.forward.z);

        GameObject gameObject = (GameObject)Instantiate(GameManager.activeInstance.GetEntityPrefab(projectileName),
            projectileSpawnPoint, transform.rotation);

        ProjectileController projectile = gameObject.GetComponent<ProjectileController>();
        projectile.currentRangeToTarget = 0.9f * weaponRange;
        projectile.target = attackTarget;
    }

    protected override void SaveDetails(JsonWriter writer)
    {
        base.SaveDetails(writer);

        SaveManager.SaveQuaternion(writer, TankProperties.TARGET_BEARING, targetBearing);
    }

    protected override bool LoadDetails(JsonReader reader, string propertyName)
    {
        base.LoadDetails(reader, propertyName);

        bool loadComplete = false;

        switch (propertyName)
        {
            case TankProperties.TARGET_BEARING:
                targetBearing = LoadManager.LoadQuaternion(reader);
                loadComplete = true;
                break;
        }

        return loadComplete;
    }
}