using UnityEngine;

public class NpcSoldierController : AutonomousAgent
{
    public Vehicle VehiclePrefab;
    
    // Navigation
    private SteeringBehaviour steering;
    private Vector3[] path;
    private int curPathIndex;

    // Vehicle
    private Vehicle vehicle;

    // State
    private NpcSoldierState lastState;
    private NpcSoldierState state;

    // Death
    private Vector3 killPosition;
    private Vector3 killDirection;
    private float killPower;

    private void Awake()
    {
        steering = new SteeringBehaviour(this);
        InitVehicle(VehiclePrefab.gameObject);
        state = NpcSoldierState.Idle;

        AssignWanderPosition();
    }

    private void InitVehicle(GameObject prefab)
    {
        vehicle = ((GameObject)Instantiate(prefab, transform.position, transform.rotation)).GetComponent<Vehicle>();
        vehicle.transform.parent = transform;
        //vehicle.gameObject.layer = LayerMask.NameToLayer("Player");
        vehicle.OnVehicleDestroyed += OnVehicleDestroyed;
        vehicle.Initialize();
    }

    private void AssignWanderPosition()
    {
        var randomSpherePoint = Random.insideUnitSphere;
        var wanderTo = transform.position + transform.forward*10f + new Vector3(randomSpherePoint.x, 0f, randomSpherePoint.z);

        var navPath = new NavMeshPath();
        if (NavMesh.CalculatePath(transform.position, wanderTo, NavMesh.AllAreas, navPath))
        {
            path = navPath.corners;
        }
    }

    private void Update()
    {
        lastState = state;
        Heading = transform.forward;
    }

    private void OnVehicleDestroyed()
    {
        Destroy(gameObject);
    }

    private void OnDrawGizmos()
    {
        if (path != null)
        {
            Gizmos.color = Color.magenta;
            for (var i = 0; i < path.Length; i++)
            {
                Gizmos.DrawSphere(path[i], 0.5f);
            }
        }
    }

    public enum NpcSoldierState
    {
        Idle,
        Wander
    }
}
