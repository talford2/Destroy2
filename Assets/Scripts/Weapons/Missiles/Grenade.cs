using UnityEngine;

public class Grenade : Missile
{
    public float LaunchForce = 10000f;
    public float ExplosionPower;
    public float ExplosionRadius;
    public float ExplosionUpwardsModifier;
    public float ExplodeTime = 3f;

    private MeshRenderer meshRenderer;
    private Rigidbody rBody;

    private float lifeCooldown;

    private void Awake()
    {
        meshRenderer = GetComponentInChildren<MeshRenderer>();
        rBody = GetComponent<Rigidbody>();
        Stop();
    }

    public override void Shoot(Vector3 fromPosition, Vector3 direction, Vector3 initVelocity)
    {
        //transform.position = fromPosition;
        rBody.rotation = Quaternion.LookRotation(direction);
        rBody.position = fromPosition;
        rBody.isKinematic = false;
        rBody.AddForce(direction*LaunchForce);
        meshRenderer.enabled = true;
        lifeCooldown = ExplodeTime;
        base.Shoot(fromPosition, direction, initVelocity);
    }

    public override void LiveUpate()
    {
        if (lifeCooldown >= 0f)
        {
            lifeCooldown -= Time.deltaTime;
            if (lifeCooldown < 0f)
            {
                Explode(rBody.position);
            }
        }
    }

    private void Explode(Vector3 position)
    {
        var splashHitColliders = Physics.OverlapSphere(position, ExplosionRadius, ~LayerMask.GetMask("Sensors", "MissileSensors"));

        foreach (var splashHitCollider in splashHitColliders)
        {
            var hitKillable = splashHitCollider.GetComponentInParent<Killable>();
            if (hitKillable != null)
            {
                var splashDamage = GetSplashDamage(position, splashHitCollider.transform.position);
                Debug.Log("SPLASH DAMAGE: " + splashDamage);
                hitKillable.Damage(splashHitCollider, position, splashHitCollider.transform.position - position, ExplosionPower, splashDamage, GetOwner());
            }
        }

        splashHitColliders = Physics.OverlapSphere(position, ExplosionRadius, ~LayerMask.GetMask("Sensors", "MissileSensors"));
        foreach (var splashHitCollider in splashHitColliders)
        {
            var hitCorpse = splashHitCollider.GetComponentInParent<Corpse>();
            if (hitCorpse != null)
            {
                splashHitCollider.attachedRigidbody.AddExplosionForce(ExplosionPower, position, ExplosionRadius, ExplosionUpwardsModifier, ForceMode.Force);
            }
            var hitEquipWeapon = splashHitCollider.GetComponentInParent<EquipWeapon>();
            if (hitEquipWeapon != null)
            {
                splashHitCollider.attachedRigidbody.AddExplosionForce(ExplosionPower, position, ExplosionRadius, ExplosionUpwardsModifier, ForceMode.Force);
            }
            var hitShootable = splashHitCollider.GetComponentInParent<Shootable>();
            if (hitShootable != null)
            {
                splashHitCollider.attachedRigidbody.AddExplosionForce(ExplosionPower, position, ExplosionRadius, ExplosionUpwardsModifier, ForceMode.Force);
            }
        }

        // Hit
        if (HitEffect != null)
        {
            var missileHitEffect = (GameObject)Instantiate(HitEffect, position, Quaternion.identity);
        }
        Stop();
    }

    private float GetSplashDamage(Vector3 explosionCentre, Vector3 atPosition)
    {
        var fromCentre = atPosition - explosionCentre;
        return Damage * Mathf.Clamp((ExplosionRadius * ExplosionRadius - fromCentre.sqrMagnitude) / (ExplosionRadius * ExplosionRadius), 0f, 1f);
    }

    public override void AlwaysUpdate()
    {
    }

    public override void Stop()
    {
        meshRenderer.enabled = false;
        rBody.isKinematic = true;
        base.Stop();
    }
}
