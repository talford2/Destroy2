using UnityEngine;

public class Rocket : Missile
{
    [Header("Rocket")]
    public float Speed;
    public float ExplosionPower;
    public float ExplosionRadius;
    public float ExplosionUpwardsModifier;
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
            RaycastHit missileSensorHit;
            if (Physics.SphereCast(hitRay, Radius, out missileSensorHit, Speed * Time.deltaTime, LayerMask.GetMask("MissileSensors")))
            {
                var sensor = missileSensorHit.collider.GetComponentInParent<MissileSensor>();
                sensor.Trigger(this);
            }
            if (Physics.SphereCast(hitRay, Radius, out shootHit, Speed * Time.deltaTime, ~LayerMask.GetMask("Sensors", "MissileSensors")))
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
        if (PlayerController.Current.GetVehicle() != null)
            obserationPosition = PlayerController.Current.GetVehicle().transform.position;

        var fromObservationPosition = transform.position - obserationPosition;
        if (fromObservationPosition.sqrMagnitude > DeactivateDistance * DeactivateDistance)
        {
            Stop();
        }
        if (Owner == null)
        {
            if (!isLive)
                Destroy(gameObject, 10f);
        }
    }

    private float GetSplashDamage(Vector3 explosionCentre, Vector3 atPosition)
    {
        var fromCentre = atPosition - explosionCentre;
        return Damage*Mathf.Clamp((ExplosionRadius * ExplosionRadius - fromCentre.sqrMagnitude) / (ExplosionRadius * ExplosionRadius), 0f, 1f);
    }

    public override void HandleCollision(RaycastHit hit, Vector3 direction)
    {
        if (hit.collider != null)
        {
            Hit(hit);
            var splashHitColliders = Physics.OverlapSphere(hit.point, ExplosionRadius, ~LayerMask.GetMask("Sensors", "MissileSensors"));

            foreach (var splashHitCollider in splashHitColliders)
            {
                var hitKillable = splashHitCollider.GetComponentInParent<Killable>();
                if (hitKillable != null)
                {
                    var splashDamage = GetSplashDamage(hit.point, splashHitCollider.transform.position);
                    Debug.Log("SPLASH DAMAGE: " + splashDamage);
                    hitKillable.Damage(splashHitCollider, hit.point, splashHitCollider.transform.position - hit.point, ExplosionPower, splashDamage, this);
                }
            }

            splashHitColliders = Physics.OverlapSphere(hit.point, ExplosionRadius, ~LayerMask.GetMask("Sensors", "MissileSensors"));
            foreach (var splashHitCollider in splashHitColliders)
            {
                var hitCorpse = splashHitCollider.GetComponentInParent<Corpse>();
                if (hitCorpse != null)
                {
                    splashHitCollider.attachedRigidbody.AddExplosionForce(ExplosionPower, hit.point, ExplosionRadius, ExplosionUpwardsModifier, ForceMode.Force);
                }
                var hitEquipWeapon = splashHitCollider.GetComponentInParent<EquipWeapon>();
                if (hitEquipWeapon != null)
                {
                    splashHitCollider.attachedRigidbody.AddExplosionForce(ExplosionPower, hit.point, ExplosionRadius, ExplosionUpwardsModifier, ForceMode.Force);
                }
                var hitShootable = hit.collider.GetComponentInParent<Shootable>();
                if (hitShootable != null)
                {
                    hit.collider.attachedRigidbody.AddForceAtPosition(direction.normalized * Power, hit.point, ForceMode.Force);
                }
            }

            PlayerCamera.Current.Shake(hit.point, 10f, 80f, 0.3f, 2f, 0f);
        }
    }

    public override void Stop()
    {
        meshRenderer.enabled = false;
        if (smokeInstance != null)
            smokeInstance.GetComponentInChildren<ParticleSystem>().Stop();
        base.Stop();
    }
}
