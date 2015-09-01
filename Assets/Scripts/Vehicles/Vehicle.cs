using UnityEngine;

public abstract class Vehicle : MonoBehaviour
{
    public float CameraDistance;

    public Vector3 CameraOffset;

    public abstract void Initialize();

    public abstract void SetMove(float forward, float strafe);

    public abstract void SetPitchYaw(float pitch, float yaw);

    public abstract Vector2 GetPitchYaw();

    public abstract void SetAimAt(Vector3 position);

    public abstract void TriggerPrimaryWeapon();

    public abstract void ReleasePrimaryWeapon();
}