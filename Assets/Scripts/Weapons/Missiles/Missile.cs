using UnityEngine;

public abstract class Missile : MonoBehaviour
{
    protected GameObject Owner;
    protected float Damage;
    protected float Power;
    protected float Radius;
    protected float DeactivateDistance = 10000f;

    [Header("Missile")]
    public GameObject HitEffect;
    public bool CanLock;

    protected bool isLive;

    protected Transform ShootPoint;
    //protected Killable LockOnTarget;

    public virtual void Initialize(GameObject owner, float damage, float power, float radius)
    {
        Owner = owner;
        Damage = damage;
        Power = power;
        Radius = 0f;
    }

    public virtual void SetShootPoint(Transform shootPoint)
    {
        ShootPoint = shootPoint;
    }

    public virtual void Shoot(Vector3 fromPosition, Vector3 direction)
    {
        isLive = true;
    }

    public virtual void Shoot(Vector3 fromPosition, Vector3 direction, Vector3 initVelocity)
    {
        isLive = true;
    }

    public virtual void Stop()
    {
        isLive = false;
    }

    public virtual void Hit(RaycastHit hit)
    {
        if (HitEffect != null)
        {
            var hitEffect = (GameObject) Instantiate(HitEffect, hit.point, transform.rotation);
            hitEffect.transform.parent = hit.collider.transform;
        }
        Stop();
    }

    public abstract void LiveUpate();

    public abstract void AlwaysUpdate();

    private void Update()
    {
        if (isLive)
            LiveUpate();
        AlwaysUpdate();
    }

    public GameObject GetOwner()
    {
        return Owner;
    }

    public void HandleCollision(RaycastHit hit, Vector3 shootDirection)
    {
        if (hit.collider != null)
        {
            if (hit.collider.gameObject != null)
            {
                Hit(hit);
            }
        }
    }
}
