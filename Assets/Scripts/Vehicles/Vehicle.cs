using UnityEngine;

public abstract class Vehicle : Killable
{
    [Header("Vehicle")]
    public float CameraDistance;

    public Vector3 CameraOffset;
    public Transform DefaultPivot;
    public Transform ZoomPivot;

    public delegate void OnVehicleDamageEvent(Collider hitCollider, Vector3 position, Vector3 direction, float power, float damage, GameObject attacker);
    public event OnVehicleDamageEvent OnVehicleDamage;

    public delegate void OnVehicleDestroyedEvent();
    public event OnVehicleDestroyedEvent OnVehicleDestroyed;

    public abstract void Initialize();

    public abstract void SetMove(float forward, float strafe);

    public abstract void SetPitchYaw(float pitch, float yaw);

    public abstract Vector2 GetPitchYaw();

    public abstract void SetRun(bool value);

    public abstract bool IsRun();

    public abstract void SetAimAt(Vector3 position);

    public abstract Vector3 GetPrimaryWeaponShootPoint();

    public abstract void SetPrimaryWeapon(VehicleGun value);

    public abstract VehicleGun GetPrimaryWeapon();

    public abstract void TriggerPrimaryWeapon();

    public abstract void ReleasePrimaryWeapon();

    public void OnDamage(Collider hitCollider, Vector3 position, Vector3 direction, float power, float damage, GameObject attacker)
    {
        if (OnVehicleDamage != null)
        {
            OnVehicleDamage(hitCollider, position, direction, power, damage, attacker);
        }
    }

    private void OnDestroy()
    {
        if (OnVehicleDestroyed != null)
        {
            OnVehicleDestroyed();
        }
    }
}