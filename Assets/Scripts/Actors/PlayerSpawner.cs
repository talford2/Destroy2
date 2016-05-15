using System.Collections;
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

    public void TriggerAndDestroy(Vehicle vehiclePrefab, VehicleGun weaponPrefab, float delay)
    {
        StartCoroutine(SpawnAndDestroy(vehiclePrefab, weaponPrefab, delay));
    }

    private IEnumerator SpawnAndDestroy(Vehicle vehiclePrefab, VehicleGun weaponPrefab, float delay)
    {
        yield return new WaitForSeconds(delay);
        PlayerController.Current.InitVehicle(vehiclePrefab, weaponPrefab, transform.position, transform.rotation);
        Destroy(gameObject);
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawLine(transform.position, new Vector3(transform.position.x, 0f, transform.position.z));
    }
}
