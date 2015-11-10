using UnityEngine;
using System.Collections;

public class ProjectileController : EntityController {

    public float velocity;
    public int damage;
    public float currentRangeToTarget { get; set; }
    public EntityController target { get; set; }

    protected override void Update()
    {
        bool hitTarget = HasHitTarget(target);
        if (hitTarget == true)
        {
            DealDamageToTarget(target);
            Destroy(gameObject);
            return;
        }

        if (currentRangeToTarget <= 0)
        {
            Destroy(gameObject);
            return;
        }

        float distanceTravelled = Time.deltaTime * velocity;
        currentRangeToTarget -= distanceTravelled;
        transform.position += (distanceTravelled * transform.forward);
    }

    protected bool HasHitTarget(EntityController target)
    {
        if (target == null)
        {
            return false;
        }

        bool hitTarget = target.selectionBounds.Contains(transform.position);
        return hitTarget;
    }

    private void DealDamageToTarget(EntityController target)
    {
        if (target == null)
        {
            return;
        }

        target.TakeDamage(damage);
    }
}