using System;
using UnityEngine;

public class Destructible : Impermanent
{
    public int hitPoints;
    public bool isAirbourne;
    [SerializeField]
    protected GameObject explosionPrefab;

    public virtual void TakeDamage(int damage)
    {
        this.hitPoints -= damage;

        if(hitPoints > 0)
        {
            return;
        }

        Explode();
        Disable();
    }

    public virtual bool Explode()
    {
        if(explosionPrefab == null)
        {
            return false;
        }

        Explosion explosion = ExplosionManager.singleton.GetExplosionByName(explosionPrefab.name);
        if (explosion == null)
        {
            return false;
        }

        explosion.gameObject.transform.position = transform.position;
        explosion.gameObject.transform.rotation = transform.rotation;
        explosion.gameObject.SetActive(true);

        return true;
    }

    public override void Disable()
    {
        gameObject.SetActive(false);
    }

    public override void Destroy()
    {
        Destroy(gameObject);
    }
}