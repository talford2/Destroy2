﻿using System.Collections.Generic;
using UnityEngine;

public class Pod : Vehicle {
    [Header("Primary Weapon")]
    public VehicleGun PrimaryWeaponPrefab;
    public Transform[] PrimaryWeaponShootPoints;

    [Header("Movement")]
    public float Speed = 80f;

    [Header("Spawner")]
    public PlayerSpawner TriggerSpawner;
    public Vehicle VehiclePrefab;
    public VehicleGun WeaponPrefab;

    [Header("Landing")]
    public GameObject LandEffectPrefab;
    public List<AudioClip> LandSounds;
    public float LandingSplashDamage = 150f;
    public float LandHitForceRadius = 5f;
    public float LandHitUpwardModifier = 1f;
    public float LandHitForce = 10000f;

    //private bool hasSpawned;
    private VehicleGun primaryWeapon;

    private Vector3 aimAt;
    //private float pitchTarget;
    private float yawTarget;
    private float acceleration = 100f;
    private void Awake()
    {
        //transform.position += Vector3.up * 300f;
    }

    public override void LiveUpdate()
    {
        // steering facing direction
        var lookAngle = Quaternion.LookRotation(aimAt - primaryWeapon.GetShootPointsCentre());
        yawTarget = Mathf.LerpAngle(yawTarget, lookAngle.eulerAngles.y, 5f * Time.deltaTime);
        transform.rotation = Quaternion.Lerp(transform.rotation, Quaternion.Euler(0f, yawTarget, 0f), 25f * Time.deltaTime);

        Speed += acceleration * Time.deltaTime;

        transform.position += Speed * Vector3.down * Time.deltaTime;

        HeadsUpDisplay.Current.HideCrosshair();
        var hitRay = new Ray(transform.position, Vector3.down);
        RaycastHit hit;

        if (Physics.Raycast(hitRay, out hit, 20f, LayerMask.GetMask("Player")))
        {
            var inviisbleDestruct = hit.collider.GetComponentInParent<InvisibleDestruct>();
            if (inviisbleDestruct != null)
                Destroy(inviisbleDestruct.gameObject);
        }

        if (Physics.Raycast(hitRay, out hit, Mathf.Abs(Speed * Time.deltaTime + 0.5f), LayerMask.GetMask("Terrain", "Player")))
        {
            transform.position = hit.point;
            WorldSounds.PlayClipAt(transform.position, LandSounds[Random.Range(0, LandSounds.Count)]);
            Land();
            Instantiate(LandEffectPrefab, transform.position, Quaternion.identity);
            TriggerSpawner.TriggerAndDestroy(VehiclePrefab, WeaponPrefab, 0.3f);
            Targeting.RemoveTargetable(Team.Good, GetComponent<Killable>());
            Destroy(this);
        }
    }

    private void Land()
    {
        var splashHitColliders = Physics.OverlapSphere(transform.position, LandHitForceRadius, ~LayerMask.GetMask("Sensors", "MissileSensors", "Player"));

        foreach (var splashHitCollider in splashHitColliders)
        {
            var hitKillable = splashHitCollider.GetComponentInParent<Killable>();
            if (hitKillable != null)
            {
                hitKillable.Damage(splashHitCollider, transform.position, splashHitCollider.transform.position - transform.position, LandHitForce, LandingSplashDamage, null);
            }
        }

        splashHitColliders = Physics.OverlapSphere(transform.position, LandHitForceRadius, ~LayerMask.GetMask("Sensors", "MissileSensors"));
        foreach (var splashHitCollider in splashHitColliders)
        {
            var hitCorpse = splashHitCollider.GetComponentInParent<Corpse>();
            if (hitCorpse != null)
            {
                splashHitCollider.attachedRigidbody.AddExplosionForce(LandHitForce, transform.position, LandHitForceRadius, LandHitUpwardModifier, ForceMode.Force);
            }
            var hitEquipWeapon = splashHitCollider.GetComponentInParent<EquipWeapon>();
            if (hitEquipWeapon != null)
            {
                splashHitCollider.attachedRigidbody.AddExplosionForce(LandHitForce, transform.position, LandHitForceRadius, LandHitUpwardModifier, ForceMode.Force);
            }
            var hitShootable = splashHitCollider.GetComponentInParent<Shootable>();
            if (hitShootable != null)
            {
                splashHitCollider.attachedRigidbody.AddExplosionForce(LandHitForce, transform.position, LandHitForceRadius, LandHitUpwardModifier, ForceMode.Force);
            }
        }

        PlayerCamera.Current.Shake(transform.position, 100f, 1000f, 0.3f, 5f, 0f);
    }

    public override void Initialize()
    {
        SetPrimaryWeapon(PrimaryWeaponPrefab);

        var lookYaw = Quaternion.LookRotation(aimAt - primaryWeapon.GetShootPointsCentre()).eulerAngles.y;
        yawTarget = lookYaw;
        transform.rotation = Quaternion.Euler(transform.eulerAngles.x, lookYaw, transform.eulerAngles.z);
    }

    public override void SetMove(float forward, float strafe)
    {
    }

    public override void SetPitchYaw(float pitch, float yaw)
    {
        //pitchTarget -= pitch;
        yawTarget += yaw;
    }

    public override Vector2 GetPitchYaw()
    {
        return Vector2.zero;
    }

    public override void SetCrouch(bool value)
    {
    }

    public override void SetRun(bool value)
    {
    }

    public override bool IsRun()
    {
        return true;
    }

    public override void SetAimAt(Vector3 position)
    {
        aimAt = position;
    }

    public override Vector3 GetPrimaryWeaponShootPoint()
    {
        return Vector3.zero;
    }

    public override Vector3 GetPrimaryWeaponAimDirection()
    {
        return Vector3.zero;
    }

    public override void SetPrimaryWeapon(VehicleGun value)
    {
        if (primaryWeapon != null)
            Destroy(primaryWeapon.gameObject);
        primaryWeapon = Instantiate(value).GetComponent<VehicleGun>();
        //primaryWeapon.SetVelocityReference(new VelocityReference { Value = velocity });
        primaryWeapon.InitGun(PrimaryWeaponShootPoints, gameObject);
        primaryWeapon.SetClipRemaining(100);
        //primaryWeapon.OnFinishReload += OnReloadPrimaryFinish;
        primaryWeapon.transform.parent = transform;
        primaryWeapon.transform.localPosition = Vector3.zero;

        if (PlayerController.Current != null && PlayerController.Current.GetVehicle() == this)
        {
            if (HeadsUpDisplay.Current != null)
            {
                HeadsUpDisplay.Current.SetCrosshair(primaryWeapon.Crosshair);
            }
        }
    }

    public override VehicleGun GetPrimaryWeapon()
    {
        return primaryWeapon;
    }

    public override void TriggerPrimaryWeapon()
    {
    }

    public override void ReloadPrimaryWeapon(int rounds)
    {
    }

    public override void ReleasePrimaryWeapon()
    {
    }

    public override void AlertNeighbours()
    {
    }

    public override void Die(GameObject attacker)
    {
        Destroy(primaryWeapon.gameObject);
        base.Die(attacker);
    }
}
