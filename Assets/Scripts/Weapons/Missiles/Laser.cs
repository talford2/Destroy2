using UnityEngine;

[RequireComponent(typeof(LineRenderer))]
public class Laser : Missile
{
    [Header("Laser")]
    public float Speed;
    public float TrailLength;

    private LineRenderer trail;

    private bool willHit;
    private RaycastHit hit;

    private Vector3 initialVelocity;
    private Vector3 shootDirection;
    private float trailLengthSquared;

    private Vector3 obserationPosition;
    private Vector3 shootPointPosition;

    private void Awake()
    {
        trail = GetComponent<LineRenderer>();
        trail.enabled = false;
        trailLengthSquared = TrailLength * TrailLength;
        Stop();
    }

    public override void Shoot(Vector3 fromPosition, Vector3 direction, Vector3 initVelocity)
    {
        transform.position = fromPosition;
        shootDirection = direction.normalized;
        trail.enabled = true;
        trail.SetPosition(0, fromPosition);
        trail.SetPosition(1, fromPosition);
        initialVelocity = initVelocity;

        CheckForHit(transform.position, shootDirection);

        var velocity = initialVelocity + shootDirection * Speed;
        transform.Translate(velocity * Time.deltaTime);

        base.Shoot(fromPosition, direction, initVelocity);
    }

    public override void LiveUpate()
    {
        var velocity = initialVelocity + shootDirection * Speed;

        if (ShootPoint != null)
            shootPointPosition = ShootPoint.position;

        var fromInitPos = shootPointPosition - transform.position;

        if (fromInitPos.sqrMagnitude <= trailLengthSquared + 0.1f)
        {
            trail.SetPosition(0, transform.position);
            trail.SetPosition(1, shootPointPosition);
        }
        else
        {
            trail.SetPosition(0, transform.position);
            trail.SetPosition(1, transform.position - shootDirection * TrailLength);
        }

        CheckForHit(transform.position, transform.TransformDirection(velocity * Time.deltaTime));

        transform.Translate(velocity * Time.deltaTime);
    }

    private void CheckForHit(Vector3 from, Vector3 direction)
    {
        if (!willHit)
        {
            var hitRay = new Ray(from, direction);
            if (Physics.SphereCast(hitRay, Radius, out hit, Speed * Time.deltaTime)) //, ~LayerMask.GetMask("Sensors", "Items", "Corpses", "Effects")))
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
        trail.enabled = false;
        base.Stop();
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawSphere(transform.position, 0.5f);
    }
}
