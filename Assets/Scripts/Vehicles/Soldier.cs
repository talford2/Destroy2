﻿using UnityEngine;

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
    private bool isShooting;

    // Locomotion
    private Animator meshAnimator;
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
        controller.Move(velocity * Time.deltaTime);
        transform.rotation = Quaternion.Lerp(transform.rotation, Quaternion.Euler(0f, yawTarget, 0f), 25f * Time.deltaTime);
        // Locomotion
        meshAnimator.SetBool("IsAim", !isRunning);
        meshAnimator.SetBool("IsShooting", isShooting);
        meshAnimator.SetFloat("Speed", move.z);
        meshAnimator.SetFloat("VerticalSpeed", move.z);
        meshAnimator.SetFloat("HorizontalSpeed", move.x);
    }

    public void LateUpdate()
    {
        if (!isRunning)
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
        if (primaryWeapon!=null)
            Destroy(primaryWeapon.gameObject);
        primaryWeapon = Instantiate(value).GetComponent<VehicleGun>();

        if (PrimaryWeaponPrefab.EquipPrefab != null)
        {
            var equippedWeapon = Instantiate(value.EquipPrefab);
            equippedWeapon.transform.parent = WeaponPlaceholder;
            equippedWeapon.transform.localPosition = Vector3.zero;
            equippedWeapon.transform.localRotation = Quaternion.identity;
            PrimaryWeaponShootPoints = equippedWeapon.GetComponent<EquipWeapon>().ShootPoints;
        }

        primaryWeapon.SetVelocityReference(new VelocityReference { Value = velocity });
        primaryWeapon.InitGun(PrimaryWeaponShootPoints, gameObject);
        primaryWeapon.SetClipRemaining(100);
        primaryWeapon.OnShoot += OnShootPrimaryWeapon;
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

    private void OnShootPrimaryWeapon()
    {
        meshAnimator.SetTrigger("Shoot");
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
        OnDamage(hitCollider, position, direction, power, damage, attacker);
        if (hitCollider == HeadCollider)
        {
            Debug.Log("HEAD DAMAGE!");
            ApplyDamage(damage * 10f, attacker);
        }
        else
        {
            base.Damage(hitCollider, position, direction, power, damage, attacker);
        }
    }

    public override void Damage(Collider hitCollider, Vector3 position, Vector3 direction, Missile missile)
    {
        killPosition = position;
        killDirection = direction;
        killPower = missile.GetPower();
        OnDamage(hitCollider, position, direction, missile.GetPower(), missile.GetDamage(), missile.GetOwner());
        if (hitCollider == HeadCollider)
        {
            Debug.Log("HEADSHOT!");
            ApplyDamage(missile.GetDamage() * 10f, missile.GetOwner());
        }
        else
        {
            base.Damage(hitCollider, position, direction, missile);
        }
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