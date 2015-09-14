using UnityEngine;

public class PlayerSpawner : MonoBehaviour
{
    public SafeSensor SafeSensor;

    public bool IsSafe
    {
        get { return SafeSensor.GetIsSafe(); }
    }

    private void Awake()
    {
        SafeSensor.SetTeam(Team.Good);
    }

    public void Trigger(Vehicle vehiclePrefab, VehicleGun weaponPrefab)
    {
        PlayerController.Current.InitVehicle(vehiclePrefab, weaponPrefab, transform.position, transform.rotation);
    }
}
