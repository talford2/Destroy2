using UnityEngine;
using UnityEngineInternal;

[RequireComponent(typeof (CharacterController))]
public class PlayerController : MonoBehaviour
{
    public float StrafeSpeed = 5f;
    public float ForwardSpeed = 5f;
    public VehicleGun PrimaryWeapon;
    public Transform[] ShootPoints;
    public float MaxAimDistance = 1000f;

    private static PlayerController current;
    public static PlayerController Current {get{ return current;}}

    private CharacterController controller;
    private Animator meshAnimator;

    private VehicleGun gun;

    private float fallSpeed;
    private float yawTarget;
    private float pitchTarget;

    private Vector3 velocity;

    // Aiming
    private Vector3 screenCentre;
    private Vector3 aimAt;

    private void Awake()
    {
        controller = GetComponent<CharacterController>();
        meshAnimator = GetComponentInChildren<Animator>();

        var gunInstance = Instantiate(PrimaryWeapon);
        gun = gunInstance.GetComponent<VehicleGun>();
        gun.SetVelocityReference(new VelocityReference { Value = velocity });
        gun.InitGun(ShootPoints, gameObject);
        gun.SetClipRemaining(100);
        gun.OnFinishReload += OnReloadFinish;

        screenCentre = new Vector3(0.5f, 0.5f, 0f);

        current = this;
    }

    private void Update()
    {
        // Movement
        yawTarget += Input.GetAxis("Mouse X");
        pitchTarget -= Input.GetAxis("Mouse Y");
        var move = new Vector3(Input.GetAxis("Horizontal"), 0f, Input.GetAxis("Vertical"));

        fallSpeed += -9.81f*Time.deltaTime;
        velocity = transform.right*move.x*StrafeSpeed + Vector3.up*fallSpeed + transform.forward*move.z*ForwardSpeed;
        pitchTarget = Mathf.Clamp(pitchTarget, -40f, 80f);

        PlayerCamera.Current.SetTargetPitchYaw(pitchTarget, yawTarget);

        controller.Move(velocity*Time.deltaTime);
        transform.rotation = Quaternion.Lerp(transform.rotation, Quaternion.Euler(0f, yawTarget, 0f), 25f*Time.deltaTime);

        // Aiming
        var targetRay = PlayerCamera.Current.GetComponent<Camera>().ViewportPointToRay(screenCentre);
        RaycastHit targetHit;
        if (Physics.Raycast(targetRay, out targetHit, MaxAimDistance, ~LayerMask.GetMask("Player")))
        {
            aimAt = targetHit.point;
        }
        else
        {
            aimAt = targetRay.GetPoint(MaxAimDistance);
        }

        // Shooting
        if (Input.GetButton("Fire1"))
        {
            gun.TriggerShoot(aimAt, 0f);
            if (gun.GetClipRemaining() == 0)
                gun.TriggerReload(gun.ClipCapacity);
        }
        else
        {
            gun.ReleaseTriggerShoot();
        }

        // Locomotion
        meshAnimator.SetFloat("Speed", move.z);
    }

    private void OnReloadFinish()
    {
        gun.SetClipRemaining(gun.ClipCapacity);
        Debug.Log("RELOAD FINISHED!");
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawSphere(aimAt, 0.5f);
    }
}
