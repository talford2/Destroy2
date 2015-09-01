using UnityEngine;

public abstract class Vehicle : MonoBehaviour
{
    public abstract void Initialize();

    public abstract void TriggerPrimaryWeapon();

    public abstract void ReleasePrimaryWeapon();
}