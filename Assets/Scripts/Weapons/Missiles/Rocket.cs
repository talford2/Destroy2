using UnityEngine;

public class Rocket : Missile
{
    [Header("Rocket")]
    public float Speed;

    private MeshRenderer meshRenderer;
    private Vector3 shootDirection;
    private Vector3 initialVelocity;
    private Vector3 obserationPosition;

    private bool willHit;
    private RaycastHit hit;

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

        initialVelocity = initVelocity;

        CheckForHit(transform.position, shootDirection);

        var velocity = initialVelocity + shootDirection * Speed;
        transform.Translate(velocity * Time.deltaTime);

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
            if (Physics.SphereCast(hitRay, Radius, out hit, Speed * Time.deltaTime, ~LayerMask.GetMask("Sensors"))) //, ~LayerMask.GetMask("Sensors", "Items", "Corpses", "Effects")))
            {
                if (Owner != null)
                {
                    var hitKillable = hit.collider.GetComponentInParent<Killable>();
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
            HandleCollision(hit, shootDirection);
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

    public override void Stop()
    {
        meshRenderer.enabled = false;
        base.Stop();
    }
}
