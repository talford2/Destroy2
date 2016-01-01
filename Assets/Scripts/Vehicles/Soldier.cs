using UnityEngine;

public class Soldier : Vehicle
{
    [Header("Movement")]
    public float ForwardSpeed = 5f;
    public float StrafeSpeed = 5f;

    [Header("Primary Weapon")]
    public VehicleGun PrimaryWeaponPrefab;
    public Transform[] PrimaryWeaponShootPoints;

    [Header("Parts")]
    public Collider HeadCollider;
    public Transform WeaponPlaceholder;
    public GameObject CorpsePrefab;

    public Transform MidTorsoTransform;
    public Transform UpperTorsoTransform;
    public Transform HeadTransform;

    public AudioClip HeadShotSound;

    private CharacterController controller;
    private VehicleGun primaryWeapon;
    private GameObject equippedPrimaryWeapon;

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
    private bool isCrouching;
    private bool isRunning;

    private Quaternion midTorsoAngle;
    private Quaternion upperTorsoAngle;
    private Quaternion headAimAngle;
    private Quaternion gunAimAngle;

    // Death
    private Vector3 killPosition;
    private Vector3 killDirection;
    private float killPower;

    public override void Initialize()
    {
        controller = GetComponent<CharacterController>();
        meshAnimator = GetComponent<Animator>();

        SetPrimaryWeapon(PrimaryWeaponPrefab);

        yawTarget = transform.eulerAngles.y;
    }

    public override void LiveUpdate()
    {
        var lookAngle = Quaternion.LookRotation(aimAt - primaryWeapon.GetShootPointsCentre());
        pitchTarget = Mathf.LerpAngle(pitchTarget, lookAngle.eulerAngles.x, 5f*Time.deltaTime);
        yawTarget = Mathf.LerpAngle(yawTarget, lookAngle.eulerAngles.y, 5f * Time.deltaTime);

        fallSpeed += -9.81f * Time.deltaTime;
        velocity = Vector3.up * fallSpeed;

        if (controller.isGrounded)
            fallSpeed = 0f;

        controller.Move(velocity * Time.deltaTime);
        transform.rotation = Quaternion.Lerp(transform.rotation, Quaternion.Euler(0f, yawTarget, 0f), 25f * Time.deltaTime);
        // Locomotion
        meshAnimator.SetBool("IsAim", !isRunning);
        meshAnimator.SetFloat("Speed", move.z);
        meshAnimator.SetFloat("VerticalSpeed", move.z);
        meshAnimator.SetFloat("HorizontalSpeed", move.x);
        meshAnimator.SetBool("IsCrouched", isCrouching);

        if (isCrouching)
        {
            controller.height = 1.2f;
            controller.center = new Vector3(0, 0.6f, 0);
        }
        else
        {
            controller.height = 1.5f;
            controller.center = new Vector3(0, 0.85f, 0);
        }
    }

    public void LateUpdate()
    {
        if (!isRunning && !isCrouching)
        {
            var midTorsoTargetAngle = Quaternion.LookRotation((aimAt - MidTorsoTransform.position)*0.1f);
            midTorsoAngle = Quaternion.Lerp(midTorsoAngle, midTorsoTargetAngle, 5f*Time.deltaTime);
            MidTorsoTransform.rotation = midTorsoAngle*Quaternion.Euler(274.5f, 42f, 0);

            var upperTorsoTargetAngle = Quaternion.LookRotation((aimAt - UpperTorsoTransform.position)*0.2f);
            upperTorsoAngle = Quaternion.Lerp(upperTorsoAngle, upperTorsoTargetAngle, 4f*Time.deltaTime);
            UpperTorsoTransform.rotation = upperTorsoAngle*Quaternion.Euler(279.3f, 47f, 0);
            var headAimTargetAngle = Quaternion.LookRotation(aimAt - HeadTransform.position);
            headAimAngle = Quaternion.Lerp(headAimAngle, headAimTargetAngle, 5f*Time.deltaTime);
            HeadTransform.rotation = headAimAngle*Quaternion.Euler(270f, 47f, 0);

            var gunAimTargetAngle = Quaternion.LookRotation(aimAt - WeaponPlaceholder.position);
            gunAimAngle = Quaternion.Lerp(gunAimAngle, gunAimTargetAngle, 5f*Time.deltaTime);
            WeaponPlaceholder.rotation = gunAimAngle*Quaternion.Euler(270f, 0, 0);
        }
    }

    public override void SetMove(float forward, float strafe)
    {
        move = new Vector3(strafe, 0f, forward);
        //velocity = Vector3.up*fallSpeed;
        //velocity = transform.right * move.x * StrafeSpeed + Vector3.up * fallSpeed + transform.forward * move.z * ForwardSpeed;
    }

    public override void SetCrouch(bool value)
    {
        isCrouching = value;
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

    public override Vector3 GetPrimaryWeaponAimDirection()
    {
        return primaryWeapon.GetShootPointsDirection();
    }

    public override void SetPrimaryWeapon(VehicleGun value)
    {
        if (primaryWeapon!=null)
            Destroy(primaryWeapon.gameObject);
        primaryWeapon = Instantiate(value).GetComponent<VehicleGun>();

        if (PrimaryWeaponPrefab.EquipPrefab != null)
        {
            if (equippedPrimaryWeapon!=null)
                Destroy(equippedPrimaryWeapon);
            equippedPrimaryWeapon = Instantiate(value.EquipPrefab);
            equippedPrimaryWeapon.transform.parent = WeaponPlaceholder;
            equippedPrimaryWeapon.transform.localPosition = Vector3.zero;
            equippedPrimaryWeapon.transform.localRotation = Quaternion.identity;
            PrimaryWeaponShootPoints = equippedPrimaryWeapon.GetComponent<EquipWeapon>().ShootPoints;
        }

        primaryWeapon.SetVelocityReference(new VelocityReference { Value = Vector3.zero });
        //primaryWeapon.SetVelocityReference(new VelocityReference { Value = velocity });
        primaryWeapon.InitGun(PrimaryWeaponShootPoints, gameObject);
        primaryWeapon.SetClipRemaining(100);
        primaryWeapon.OnShoot += OnShootPrimaryWeapon;
        primaryWeapon.OnClipEmpty += OnEmptyPrimaryWeapon;
        primaryWeapon.OnFinishReload += OnReloadPrimaryFinish;
        primaryWeapon.transform.parent = transform;
        primaryWeapon.transform.localPosition = Vector3.zero;
    }

    public override VehicleGun GetPrimaryWeapon()
    {
        return primaryWeapon;
    }

    public GameObject GetEquipped()
    {
        return equippedPrimaryWeapon;
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

    public override void ReloadPrimaryWeapon(int rounds)
    {
        primaryWeapon.TriggerReload(rounds);
    }

    public override void ReleasePrimaryWeapon()
    {
        primaryWeapon.ReleaseTriggerShoot();
        var player = GetComponentInParent<PlayerController>();
        if (player != null)
        {
            PlayerCamera.Current.RestorePitchYaw();
        }
    }

    public override void AlertNeighbours()
    {
        var sounds = GetComponent<SoldierSounds>();
        sounds.AlertCall();
    }

    private void OnShootPrimaryWeapon()
    {
        // TODO: Make kick more gradual by multiplying by amount that increases with each shot until trigger is released.
        meshAnimator.SetTrigger("Shoot");
        var player = GetComponentInParent<PlayerController>();
        if (player != null)
        {
            var pitch = primaryWeapon.NonRandomPitch
                ? primaryWeapon.KickPitchYaw.x
                : Random.Range(0, primaryWeapon.KickPitchYaw.x);
            PlayerCamera.Current.AddTemporaryPitchYaw(pitch, Random.Range(-primaryWeapon.KickPitchYaw.y / 2f, primaryWeapon.KickPitchYaw.y / 2f));
        }
    }

    private void OnEmptyPrimaryWeapon()
    {
        Debug.Log("CLIP EMPTY");
        var player = GetComponentInParent<PlayerController>();
        if (player != null)
        {
            PlayerCamera.Current.RestorePitchYaw();
        }
    }

    private void OnReloadPrimaryFinish()
    {
        primaryWeapon.SetClipRemaining(primaryWeapon.ClipCapacity);
    }

    public override void Damage(Collider hitCollider, Vector3 position, Vector3 direction, float power, float damage, GameObject attacker)
    {
        killPosition = position;
        killDirection = direction;
        killPower = power;
        var damageAmount = damage;
        if (hitCollider == HeadCollider)
        {
            Debug.Log("HEAD DAMAGE!");
        }
        base.Damage(hitCollider, position, direction, power, damageAmount, attacker);
    }

    public override void Damage(Collider hitCollider, Vector3 position, Vector3 direction, Missile missile)
    {
        killPosition = position;
        killDirection = direction;
        killPower = missile.GetPower();
        var damageAmount = missile.GetDamage();
        if (hitCollider == HeadCollider)
        {
            Debug.Log("HEADSHOT!");
            damageAmount *= 10f;
            AudioSource.PlayClipAtPoint(HeadShotSound, position);
        }
        base.Damage(hitCollider, position, direction, killPower, damageAmount, missile.GetOwner());
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
        var soldierCorpse = corpse.GetComponent<SoldierCorpse>();
        if (soldierCorpse != null)
        {
            soldierCorpse.Equip(primaryWeapon.EquipPrefab);
            var player = GetComponentInParent<PlayerController>();
            if (player != null)
            {
                PlayerCamera.Current.DistanceCatupSpeed = 1f;
                PlayerCamera.Current.SetPivot(soldierCorpse.FocalPoint, Vector3.zero, 30f);
            }
        }

        var liveParts = transform.FindChild("Ground").GetComponentsInChildren<Transform>();
        var corpseColliders = corpse.GetComponentsInChildren<Collider>();

        var deadParts = corpse.transform.FindChild("Soldier").GetComponentsInChildren<Transform>();
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
                Debug.Log("KILLED!");
                corpseCollider.attachedRigidbody.AddForceAtPosition(killDirection.normalized * killPower, killHit.point, ForceMode.Force);
            }
        }

        base.Die(attacker);
        Destroy(gameObject);
    }
}