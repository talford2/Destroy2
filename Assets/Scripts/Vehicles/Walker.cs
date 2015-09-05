using UnityEngine;

[RequireComponent(typeof(CharacterController))]
[RequireComponent(typeof(Animator))]
public class Walker : Vehicle
{
    [Header("Movement")]
    public float ForwardSpeed = 5f;
    public float StrafeSpeed = 5f;

	public AudioSource StepSound;

    public Transform HeadBone;

    [Header("Primary Weapon")]
    public VehicleGun PrimaryWeaponPrefab;
    public Transform[] PrimaryWeaponShootPoints;

    private CharacterController controller;
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
    private bool isRunning;
    private float headYaw;

    public override void Initialize()
    {
        controller = GetComponent<CharacterController>();
        meshAnimator = GetComponent<Animator>();

        primaryWeapon = Instantiate(PrimaryWeaponPrefab).GetComponent<VehicleGun>();
        primaryWeapon.SetVelocityReference(new VelocityReference { Value = velocity });
        primaryWeapon.InitGun(PrimaryWeaponShootPoints, gameObject);
        primaryWeapon.SetClipRemaining(100);
        primaryWeapon.OnFinishReload += OnReloadPrimaryFinish;
        primaryWeapon.transform.parent = transform;
        primaryWeapon.transform.localPosition = Vector3.zero;
    }

    public override void LiveUpdate()
    {
        fallSpeed += -9.81f*Time.deltaTime;
        controller.Move(velocity*Time.deltaTime);
        
        // Locomotion
        var speed = Mathf.Clamp(Mathf.Abs(move.z) + Mathf.Abs(move.x), 0f, 1f);
        var turnSpeed = 2f*speed;
        var strafeAngle = Mathf.Atan2(move.x, move.z) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.Lerp(transform.rotation, Quaternion.Euler(0f, yawTarget + strafeAngle, 0f), turnSpeed*Time.deltaTime);
        headYaw = Mathf.LerpAngle(headYaw, yawTarget - transform.eulerAngles.y, 5f*Time.deltaTime);
        meshAnimator.SetFloat("Speed", speed);
    }

    private void LateUpdate()
    {
        //yaw += 25f*Time.deltaTime;
        HeadBone.transform.rotation *= Quaternion.Euler(0f, 0f, headYaw);
    }

    public override void SetMove(float forward, float strafe)
    {
        move = new Vector3(strafe, 0f, forward).normalized;
        //velocity = Vector3.up*fallSpeed;
        // Note: HeadBone.transform orientation is weird.
        velocity = HeadBone.transform.right * move.x * StrafeSpeed + Vector3.up * fallSpeed + HeadBone.transform.up * -move.z * ForwardSpeed;
    }

    public override void SetRun(bool value)
    {
        isRunning = value;
    }

    public override bool IsRun()
    {
        return isRunning;
    }

    public override void SetAimAt(Vector3 position)
    {
        aimAt = position;
    }

    public override Vector3 GetPrimaryWeaponShootPoint()
    {
        return primaryWeapon.GetShootPointsCentre();
    }

    public override void SetPitchYaw(float pitch, float yaw)
    {
        pitchTarget -= pitch;
        yawTarget += yaw;
    }

    public override Vector2 GetPitchYaw()
    {
        return new Vector2(pitchTarget, yawTarget);
    }

    public override void TriggerPrimaryWeapon()
    {
        primaryWeapon.TriggerShoot(aimAt, 0f);
        if (primaryWeapon.GetClipRemaining() == 0)
        {
            primaryWeapon.ReleaseTriggerShoot();
            primaryWeapon.TriggerReload(primaryWeapon.ClipCapacity);
        }
    }

    public override void ReleasePrimaryWeapon()
    {
        primaryWeapon.ReleaseTriggerShoot();
    }

    private void OnReloadPrimaryFinish()
    {
        primaryWeapon.SetClipRemaining(primaryWeapon.ClipCapacity);
    }

    private void LeftFootStomp()
    {
		if (StepSound != null)
		{
			StepSound.Play();
		}
        Debug.Log("LEFT");
    }

    private void RightFootStomp()
    {
		if (StepSound != null)
		{
			StepSound.Play();
		}
		Debug.Log("RIGHT");
    }

    public override void Die(GameObject attacker)
    {
        Destroy(primaryWeapon.gameObject);
        var colliders = GetComponentsInChildren<Collider>();
        foreach (var curCollider in colliders)
        {
            curCollider.enabled = false;
        }
        base.Die(attacker);
        Destroy(gameObject);
    }
}
