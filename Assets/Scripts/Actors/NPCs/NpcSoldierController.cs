using UnityEngine;

public class NpcSoldierController : MonoBehaviour
{
    public Vehicle VehiclePrefab;

    private Vehicle vehicle;
    private NpcSoldierState lastState;
    private NpcSoldierState state;
    private Vector3 killPosition;
    private Vector3 killDirection;
    private float killPower;

    private void Awake()
    {
        InitVehicle(VehiclePrefab.gameObject);
        state = NpcSoldierState.Idle;
    }

    private void InitVehicle(GameObject prefab)
    {
        vehicle = ((GameObject)Instantiate(prefab, transform.position, transform.rotation)).GetComponent<Vehicle>();
        vehicle.transform.parent = transform;
        //vehicle.gameObject.layer = LayerMask.NameToLayer("Player");
        vehicle.Initialize();
    }

    private void Update()
    {
        lastState = state;
    }

    public enum NpcSoldierState
    {
        Idle,
        Wander
    }
}
