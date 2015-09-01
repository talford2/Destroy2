using UnityEngine;

[RequireComponent(typeof(Animator))]
public class Walker : Vehicle
{
    public float ForwardSpeed = 5f;
    public float StrafeSpeed = 5f;

    public VehicleGun PrimaryWeapon;
    public Transform[] PrimaryWeaponShootPoints;

    private VehicleGun primaryWeapon;

    // Movement
    private Vector3 move;
    private Vector3 velocity;
    private float fallSpeed;

    // Aiming
    private float pitchTarget;
    private float yawTarget;
    private Vector3 aimAt;

    // Locomotion
    private Animator meshAnimator;

    public override void Initialize()
    {
        meshAnimator = GetComponent<Animator>();

        primaryWeapon = Instantiate(PrimaryWeapon).GetComponent<VehicleGun>();
        primaryWeapon.SetVelocityReference(new VelocityReference { Value = velocity });
        primaryWeapon.InitGun(PrimaryWeaponShootPoints, gameObject);
        primaryWeapon.SetClipRemaining(100);
        primaryWeapon.OnFinishReload += OnReloadPrimaryFinish;
    }

    private void Update()
    {
        transform.rotation = Quaternion.Lerp(transform.rotation, Quaternion.Euler(0f, yawTarget, 0f), 25f * Time.deltaTime);
        // Locomotion
        meshAnimator.SetFloat("Speed", move.z);
    }

    public void SetMove(float forward, float strafe)
    {
        move = new Vector3(strafe, 0f, forward);
        fallSpeed += -9.81f * Time.deltaTime;
        velocity = transform.right * move.x * StrafeSpeed + Vector3.up * fallSpeed + transform.forward * move.z * ForwardSpeed;
    }

    public void SetAimAt(Vector3 position)
    {
        aimAt = position;
    }

    public void SetPitchYaw(float pitch, float yaw)
    {
        pitchTarget -= pitch;
        yawTarget += yaw;
    }

    public void TriggerPrimaryWeapon()
    {
        primaryWeapon.TriggerShoot(aimAt, 0f);
    }

    public void ReleasePrimaryWeapon()
    {
        primaryWeapon.ReleaseTriggerShoot();
    }

    private void OnReloadPrimaryFinish()
    {
        primaryWeapon.SetClipRemaining(primaryWeapon.ClipCapacity);
    }

    private void LeftFootStomp()
    {
        Debug.Log("LEFT");
    }

    private void RightFootStomp()
    {
        Debug.Log("RIGHT");
    }
}
