using UnityEngine;

public class PlayerSpawner : MonoBehaviour
{
    public SafeSensor SafeSensor;

    public bool IsSafe
    {
        get { return SafeSensor.GetIsSafe(); }
    }

    public int EnemyCount
    {
        get { return SafeSensor.GetEnemyAgentCount(); }
    }

    private void Awake()
    {
        SafeSensor.SetTeam(Team.Good);
    }

    public void Trigger(Vehicle vehiclePrefab, VehicleGun weaponPrefab)
    {
        PlayerController.Current.InitVehicle(vehiclePrefab, weaponPrefab, transform.position, transform.rotation);
    }

    public void TriggerAndDestroy(Vehicle vehiclePrefab, VehicleGun weaponPrefab)
    {
        PlayerController.Current.InitVehicle(vehiclePrefab, weaponPrefab, transform.position, transform.rotation);
        Destroy(gameObject);
    }
}
