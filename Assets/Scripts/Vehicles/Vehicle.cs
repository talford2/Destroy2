using UnityEngine;

public abstract class Vehicle : Killable
{
    [Header("Vehicle")]
    public float CameraDistance;

    public Vector3 CameraOffset;
    public Transform DefaultPivot;
    public Transform ZoomPivot;

    public float PathAimHeight;

    public abstract void Initialize();

    public abstract void SetMove(float forward, float strafe);

    public abstract void SetPitchYaw(float pitch, float yaw);

    public abstract Vector2 GetPitchYaw();

    public abstract void SetRun(bool value);

    public abstract bool IsRun();

    public abstract void SetAimAt(Vector3 position);

    public abstract Vector3 GetPrimaryWeaponShootPoint();

    public abstract Vector3 GetPrimaryWeaponAimDirection();

    public abstract void SetPrimaryWeapon(VehicleGun value);

    public abstract VehicleGun GetPrimaryWeapon();

    public abstract void TriggerPrimaryWeapon();

    public abstract void ReloadPrimaryWeapon(int rounds);

    public abstract void ReleasePrimaryWeapon();
}