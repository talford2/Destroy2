using UnityEngine;

public class PlayerSpawner : MonoBehaviour
{
    public Vehicle VehiclePrefab;

    public void Trigger()
    {
        PlayerController.Current.InitVehicle(VehiclePrefab.gameObject, transform.position, transform.rotation);
    }
}
