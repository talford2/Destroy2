using UnityEngine;

public class PlayerSpawner : MonoBehaviour
{
    public void Trigger(Vehicle vehiclePrefab, VehicleGun weaponPrefab)
    {
        PlayerController.Current.InitVehicle(vehiclePrefab, weaponPrefab, transform.position, transform.rotation);
    }
}
