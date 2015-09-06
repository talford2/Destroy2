using System.Collections.Generic;
using UnityEngine;

public class Rocket : Missile
{
    [Header("Rocket")]
    public float Speed;
    public float ExplosionPower;
    public float ExplosionRadius;
    public GameObject SmokeEffect;

    private MeshRenderer meshRenderer;
    private GameObject smokeInstance;
    private Vector3 shootDirection;
    private Vector3 initialVelocity;
    private Vector3 obserationPosition;

    private bool willHit;
    private RaycastHit shootHit;

    private void Awake()
    {
        meshRenderer = GetComponentInChildren<MeshRenderer>();
        Stop();
    }

    public override void Shoot(Vector3 fromPosition, Vector3 direction, Vector3 initVelocity)
    {
        transform.position = fromPosition;
        shootDirection = direction.normalized;
        meshRenderer.enabled = true;

        smokeInstance = (GameObject)Instantiate(SmokeEffect, fromPosition, Quaternion.LookRotation(direction));
        smokeInstance.transform.parent = transform;

        initialVelocity = initVelocity;

        CheckForHit(transform.position, shootDirection);

        base.Shoot(fromPosition, direction, initVelocity);
    }

    public override void LiveUpate()
    {
        var velocity = initialVelocity + shootDirection * Speed;

        CheckForHit(transform.position, transform.TransformDirection(velocity * Time.deltaTime));
        transform.Translate(velocity * Time.deltaTime);
    }

    private void CheckForHit(Vector3 from, Vector3 direction)
    {
        if (!willHit)
        {
            var hitRay = new Ray(from, direction);
            if (Physics.SphereCast(hitRay, Radius, out shootHit, Speed * Time.deltaTime, ~LayerMask.GetMask("Sensors")))
            {
                if (Owner != null)
                {
                    var hitKillable = shootHit.collider.GetComponentInParent<Killable>();
                    if (hitKillable != null)
                    {
                        if (hitKillable != Owner.GetComponent<Killable>())
                            willHit = true;
                    }
                    else
                    {
                        willHit = true;
                    }
                }
            }
        }
        else
        {
            HandleCollision(shootHit, shootDirection);
            willHit = false;
        }
    }

    public override void AlwaysUpdate()
    {
        if (PlayerController.Current != null)
            obserationPosition = PlayerController.Current.transform.position;

        var fromObservationPosition = transform.position - obserationPosition;
        if (fromObservationPosition.sqrMagnitude > DeactivateDistance * DeactivateDistance)
        {
            Stop();
            if (Owner == null)
                Destroy(gameObject);
        }
    }

    public override void HandleCollision(RaycastHit hit, Vector3 direction)
    {
        if (hit.collider != null)
        {
            Hit(hit);
            var splashHitColliders = Physics.OverlapSphere(hit.point, ExplosionRadius, ~LayerMask.GetMask("Sensors"));

            foreach (var splashHitCollider in splashHitColliders)
            {
                var hitKillable = splashHitCollider.GetComponentInParent<Killable>();
                if (hitKillable != null)
                {
                    hitKillable.Damage(splashHitCollider, hit.point, splashHitCollider.transform.position - hit.point, this);
                }
            }

            splashHitColliders = Physics.OverlapSphere(hit.point, ExplosionRadius, ~LayerMask.GetMask("Sensors"));
            foreach (var splashHitCollider in splashHitColliders)
            {
                var hitCorpse = splashHitCollider.GetComponentInParent<Corpse>();
                if (hitCorpse != null)
                {
                    splashHitCollider.attachedRigidbody.AddExplosionForce(ExplosionPower, hit.point, ExplosionRadius, 1f, ForceMode.Force);
                }
                var hitEquipWeapon = splashHitCollider.GetComponentInParent<EquipWeapon>();
                if (hitEquipWeapon != null)
                {
                    splashHitCollider.attachedRigidbody.AddExplosionForce(ExplosionPower, hit.point, ExplosionRadius, 1f, ForceMode.Force);
                }
            }
        }
    }

    public override void Stop()
    {
        meshRenderer.enabled = false;
        base.Stop();
    }
}
