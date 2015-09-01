using System;
using UnityEngine;

public class Soldier : Vehicle
{
    [Header("Movement")]
    public float ForwardSpeed = 5f;
    public float StrafeSpeed = 5f;

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

    public override void Initialize()
    {
        controller = GetComponent<CharacterController>();
        meshAnimator = GetComponent<Animator>();

        primaryWeapon = Instantiate(PrimaryWeaponPrefab).GetComponent<VehicleGun>();
        primaryWeapon.SetVelocityReference(new VelocityReference { Value = velocity });
        primaryWeapon.InitGun(PrimaryWeaponShootPoints, gameObject);
        primaryWeapon.SetClipRemaining(100);
        primaryWeapon.OnFinishReload += OnReloadPrimaryFinish;
    }

    private void Update()
    {
        fallSpeed += -9.81f * Time.deltaTime;
        controller.Move(velocity * Time.deltaTime);
        transform.rotation = Quaternion.Lerp(transform.rotation, Quaternion.Euler(0f, yawTarget, 0f), 25f * Time.deltaTime);
        // Locomotion
        meshAnimator.SetBool("IsAim", true);
        meshAnimator.SetFloat("Speed", move.z);
        meshAnimator.SetFloat("VerticalSpeed", move.z);
        meshAnimator.SetFloat("HorizontalSpeed", move.x);
    }

    public override void SetMove(float forward, float strafe)
    {
        move = new Vector3(strafe, 0f, forward);
        velocity = Vector3.up*fallSpeed;
        //velocity = transform.right * move.x * StrafeSpeed + Vector3.up * fallSpeed + transform.forward * move.z * ForwardSpeed;
    }

    public override void SetAimAt(Vector3 position)
    {
        aimAt = position;
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
}