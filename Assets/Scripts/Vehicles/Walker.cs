using UnityEngine;

[RequireComponent(typeof(CharacterController))]
[RequireComponent(typeof(Animator))]
public class Walker : Vehicle
{
	[Header("Movement")]
	public float ForwardSpeed = 5f;
	public float StrafeSpeed = 5f;

	public AudioSource StepSound;
	public GameObject StepEffect;

	public Transform LeftFoot;
	public Transform RightFoot;

	

	[Header("Primary Weapon")]
	public VehicleGun PrimaryWeaponPrefab;
	public Transform[] PrimaryWeaponShootPoints;

	[Header("Parts")]
	public GameObject CorpsePrefab;

    public Transform HeadTransform;
    public Transform TurretTransform;

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
	private float headAimYaw;
    private Quaternion turretAimAngle;

	// Death
	private Vector3 killPosition;
	private Vector3 killDirection;
	private float killPower;

	public override void Initialize()
	{
		controller = GetComponent<CharacterController>();
		meshAnimator = GetComponent<Animator>();

		SetPrimaryWeapon(PrimaryWeaponPrefab);
	}

	public override void LiveUpdate()
	{
        var lookAngle = Quaternion.LookRotation(aimAt - primaryWeapon.GetShootPointsCentre());
        pitchTarget = lookAngle.eulerAngles.x;
        yawTarget = lookAngle.eulerAngles.y;

		fallSpeed += -9.81f * Time.deltaTime;
		controller.Move(velocity * Time.deltaTime);

		// Locomotion
		var speed = Mathf.Clamp(Mathf.Abs(move.z) + Mathf.Abs(move.x), 0f, 1f);
		var turnSpeed = 2f * speed;
		var strafeAngle = Mathf.Atan2(move.x, move.z) * Mathf.Rad2Deg;
		transform.rotation = Quaternion.Lerp(transform.rotation, Quaternion.Euler(0f, yawTarget + strafeAngle, 0f), turnSpeed * Time.deltaTime);

		headAimYaw = Mathf.LerpAngle(headAimYaw, yawTarget - transform.eulerAngles.y, 5f * Time.deltaTime);

        var targetTurretPitch = Quaternion.LookRotation(aimAt - TurretTransform.position);
        turretAimAngle = Quaternion.Lerp(turretAimAngle, targetTurretPitch, 5f * Time.deltaTime);

		meshAnimator.SetFloat("Speed", speed);
	}

    private void LateUpdate()
    {
        HeadTransform.rotation *= Quaternion.Euler(0f, 0f, headAimYaw);
        TurretTransform.rotation = turretAimAngle*Quaternion.Euler(270f, 0, 0);
    }

    public override void SetMove(float forward, float strafe)
	{
		move = new Vector3(strafe, 0f, forward).normalized;
		//velocity = Vector3.up*fallSpeed;
		// Note: HeadBone.transform orientation is weird.
		if (HeadTransform != null)
			velocity = HeadTransform.transform.right * move.x * StrafeSpeed + Vector3.up * fallSpeed + HeadTransform.transform.up * -move.z * ForwardSpeed;
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

    public override void SetPrimaryWeapon(VehicleGun value)
    {
        if (primaryWeapon != null)
            Destroy(primaryWeapon.gameObject);
        primaryWeapon = Instantiate(value).GetComponent<VehicleGun>();
        primaryWeapon.SetVelocityReference(new VelocityReference {Value = velocity});
        primaryWeapon.InitGun(PrimaryWeaponShootPoints, gameObject);
        primaryWeapon.SetClipRemaining(100);
        primaryWeapon.OnFinishReload += OnReloadPrimaryFinish;
        primaryWeapon.transform.parent = transform;
        primaryWeapon.transform.localPosition = Vector3.zero;
    }

    public override VehicleGun GetPrimaryWeapon()
	{
		return primaryWeapon;
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
        if (StepEffect != null)
        {
            var step = Instantiate(StepEffect, LeftFoot.position, Quaternion.identity);
        }
        PlayerCamera.Current.Shake(LeftFoot.position, 20f, 100f, 0.2f, 0.4f, 0.05f);
        Debug.Log("LEFT");
    }

    private void RightFootStomp()
    {
        if (StepSound != null)
        {
            StepSound.Play();
        }
        if (StepEffect != null)
        {
            var step = Instantiate(StepEffect, RightFoot.position, Quaternion.identity);
        }

        PlayerCamera.Current.Shake(RightFoot.position, 20f, 100f, 0.2f, 0.4f, 0.05f);
        Debug.Log("RIGHT");
    }

    public override void Damage(Collider hitCollider, Vector3 position, Vector3 direction, float power, float damage, GameObject attacker)
	{
		killPosition = position;
		killDirection = direction;
		killPower = power;
        OnDamage(hitCollider, position, direction, power, damage, attacker);
		base.Damage(hitCollider, position, direction, power, damage, attacker);
	}

	public override void Damage(Collider hitCollider, Vector3 position, Vector3 direction, Missile missile)
	{
		killPosition = position;
		killDirection = direction;
		killPower = missile.GetPower();
        OnDamage(hitCollider, position, direction, missile.GetPower(), missile.GetDamage(), missile.GetOwner());
		base.Damage(hitCollider, position, direction, missile);
	}

	public override void Die(GameObject attacker)
	{
		Destroy(primaryWeapon.gameObject);
		var colliders = GetComponentsInChildren<Collider>();
		foreach (var curCollider in colliders)
		{
			curCollider.enabled = false;
		}
		var corpse = (GameObject)Instantiate(CorpsePrefab, transform.position, transform.rotation);
		corpse.transform.parent = SceneManager.CorpseContainer;
		var walkerCorpse = corpse.GetComponent<WalkerCorpse>();
		if (walkerCorpse != null)
		{
			var player = GetComponentInParent<PlayerController>();
			if (player != null)
			{
			    PlayerCamera.Current.DistanceCatupSpeed = 1f;
				PlayerCamera.Current.SetPivot(walkerCorpse.FocalPoint, Vector3.zero, 30f);
			}
		}

		var liveParts = transform.FindChild("Ground").GetComponentsInChildren<Transform>();
		var corpseColliders = corpse.GetComponentsInChildren<Collider>();

		var deadParts = corpse.transform.FindChild("Walker").GetComponentsInChildren<Transform>();
		foreach (var livePart in liveParts)
		{
			foreach (var deadPart in deadParts)
			{
				if (livePart.name == deadPart.name)
				{
					deadPart.transform.localPosition = livePart.transform.localPosition;
					deadPart.transform.localRotation = livePart.transform.localRotation;
				}
			}
		}

		foreach (var corpseCollider in corpseColliders)
		{
			var killRay = new Ray(killPosition - 5f * killDirection.normalized, killDirection);
			RaycastHit killHit;
			if (corpseCollider.Raycast(killRay, out killHit, 10f))
			{
				corpseCollider.attachedRigidbody.AddForceAtPosition(killDirection.normalized * killPower, killHit.point, ForceMode.Force);
			}
		}

		base.Die(attacker);
		Destroy(gameObject);
	}
}
