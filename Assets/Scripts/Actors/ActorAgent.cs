using UnityEngine;

public abstract class ActorAgent : MonoBehaviour {
    public Team Team;

    public Vector3 Heading;

    public abstract void InitVehicle(Vehicle vehiclePrefab, VehicleGun weaponPrefab, Vector3 position, Quaternion rotation);

    public abstract Vehicle GetVehicle();
}
