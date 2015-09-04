using UnityEngine;

public class NpcSoldierController : AutonomousAgent
{
    public Vehicle VehiclePrefab;

    // Navigation
    private SteeringBehaviour steering;
    private bool reachedPathEnd;
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
        state = NpcSoldierState.Chase;

        //AssignWanderPosition();
    }

    private void InitVehicle(GameObject prefab)
    {
        vehicle = ((GameObject) Instantiate(prefab, transform.position, transform.rotation)).GetComponent<Vehicle>();
        vehicle.transform.parent = transform;
        //vehicle.gameObject.layer = LayerMask.NameToLayer("Player");
        vehicle.OnVehicleDestroyed += OnVehicleDestroyed;
        vehicle.Initialize();

        var neighrbourSensor = GetComponentInChildren<NeighbourSensor>();
        if (neighrbourSensor != null)
        {
            neighrbourSensor.transform.parent = vehicle.transform;
        }
    }

    private Vector2 GetSteerToPoint(Vector3 point)
    {
        var yawDiff = Vector3.Dot(point - vehicle.transform.position, vehicle.transform.right);
        var pitchDiff = Vector3.Dot(point - vehicle.transform.position, vehicle.transform.up);

        var yawTolerance = 2f;
        var pitchTolerance = 2f;

        var yawAmount = 0f;
        var pitchAmount = 0f;

        if (yawDiff < -yawTolerance)
        {
            yawAmount = Mathf.Clamp(Mathf.Abs(yawDiff)/yawTolerance, 0f, 1f);
        }
        else if (yawDiff > yawTolerance)
        {
            yawAmount = -Mathf.Clamp(Mathf.Abs(yawDiff)/yawTolerance, 0f, 1f);
        }

        if (pitchDiff < -pitchTolerance)
        {
            pitchAmount = Mathf.Clamp(Mathf.Abs(pitchDiff)/pitchTolerance, 0f, 1f);
        }
        else if (pitchDiff > pitchTolerance)
        {
            pitchAmount = -Mathf.Clamp(Mathf.Abs(pitchDiff)/pitchTolerance, 0f, 1f);
        }

        return new Vector2(-pitchAmount, -yawAmount);
    }

    private void Update()
    {
        Heading = transform.forward;
        switch (state)
        {
            case NpcSoldierState.Idle:
                break;
            case NpcSoldierState.Wander:
                Wander();
                break;
            case NpcSoldierState.Chase:
                Chase();
                break;
        }
        lastState = state;
    }

    private void AssignWanderPosition()
    {
        var randomSpherePoint = Random.insideUnitSphere;
        var wanderTo = vehicle.transform.position + vehicle.transform.forward*10f + 10f*new Vector3(randomSpherePoint.x, 0f, randomSpherePoint.z);

        var navPath = new NavMeshPath();
        if (NavMesh.CalculatePath(vehicle.transform.position, wanderTo, NavMesh.AllAreas, navPath))
        {
            path = navPath.corners;
        }
    }

    private Vector3 GetWanderForce()
    {
        var groupForces = steering.CalculateGroupForces(GetNeighbours());

        var steerForce = Vector3.zero;

        steerForce += 0.3f*groupForces.SeparationForce;
        if (steerForce.sqrMagnitude > 1f)
            return steerForce;

        steerForce += 0.3f*groupForces.CohesiveForce;
        if (steerForce.sqrMagnitude > 1f)
            return steerForce;

        steerForce += 0.3f*groupForces.AlignmentForce;
        if (steerForce.sqrMagnitude > 1f)
            return steerForce;

        // Destination Force
        steerForce += 1f*steering.SeekForce(path[curPathIndex]);
        if (steerForce.sqrMagnitude > 1f)
            return steerForce;

        return steerForce.normalized;
    }

    private void Wander()
    {
        if (path == null)
            AssignWanderPosition();

        var wanderForce = GetWanderForce();

        var pitchYaw = GetSteerToPoint(path[curPathIndex]);
        vehicle.SetPitchYaw(0f, pitchYaw.y);
        var forward = Vector3.Dot(wanderForce, vehicle.transform.forward);
        var strafe = Vector3.Dot(wanderForce, vehicle.transform.right);
        vehicle.SetMove(forward, strafe);

        if (!reachedPathEnd)
        {
            var toPathDestination = path[curPathIndex] - vehicle.transform.position;
            if (toPathDestination.sqrMagnitude <= 1f)
            {
                curPathIndex++;
                if (curPathIndex == path.Length)
                {
                    reachedPathEnd = true;
                }
            }
        }

        if (reachedPathEnd)
        {
            AssignWanderPosition();
            reachedPathEnd = false;
            curPathIndex = 0;
            vehicle.SetMove(0f, 0f);
        }
    }

    private void AssignChasePosition()
    {
        var chaseTo = PlayerController.Current.GetVehicle().transform.position;

        var navPath = new NavMeshPath();
        if (NavMesh.CalculatePath(vehicle.transform.position, chaseTo, NavMesh.AllAreas, navPath))
        {
            path = navPath.corners;
        }
        else
        {
            path = new[] {vehicle.transform.position};
            curPathIndex = 0;
        }
    }

    private Vector3 GetChaseForce()
    {
        Debug.Log("NEIGHBOURS: " + GetNeighbours().Count);
        var groupForces = steering.CalculateGroupForces(GetNeighbours());

        var steerForce = Vector3.zero;

        steerForce += 1f*groupForces.SeparationForce;
        if (steerForce.sqrMagnitude > 1f)
            return steerForce;

        steerForce += 0.3f*groupForces.CohesiveForce;
        if (steerForce.sqrMagnitude > 1f)
            return steerForce;

        steerForce += 0.3f*groupForces.AlignmentForce;
        if (steerForce.sqrMagnitude > 1f)
            return steerForce;

        Debug.Log("PATH INDEX: " + curPathIndex);

        // Destination Force
        steerForce += 1f*steering.SeekForce(path[curPathIndex]);
        if (steerForce.sqrMagnitude > 1f)
            return steerForce;
        return steerForce.normalized;
    }

    private void Chase()
    {
        AssignChasePosition();
        if (!reachedPathEnd)
        {
            var toPathDestination = path[curPathIndex] - vehicle.transform.position;
            if (toPathDestination.sqrMagnitude <= 1f)
            {
                curPathIndex++;
                if (curPathIndex == path.Length)
                {
                    reachedPathEnd = true;
                }
            }
        }

        if (reachedPathEnd)
        {
            reachedPathEnd = false;
            curPathIndex = 0;
        }
        
        var chaseForce = GetChaseForce();

        var pitchYaw = GetSteerToPoint(path[curPathIndex]);
        vehicle.SetPitchYaw(0f, pitchYaw.y);
        var forward = Vector3.Dot(chaseForce, vehicle.transform.forward);
        var strafe = Vector3.Dot(chaseForce, vehicle.transform.right);
        vehicle.SetMove(forward, strafe);
    }

    private void OnVehicleDestroyed()
    {
        Destroy(gameObject);
    }

    private void OnDrawGizmos()
    {
        if (path != null)
        {
            for (var i = 0; i < path.Length; i++)
            {
                Gizmos.color = i == 0 ? Color.yellow : Color.magenta;
                Gizmos.DrawSphere(path[i], 0.5f);
            }
            if (curPathIndex < path.Length)
                Gizmos.DrawLine(vehicle.transform.position, path[curPathIndex]);
        }
        Gizmos.color = Color.white;
        foreach (var neighbour in GetNeighbours())
        {
            Gizmos.DrawLine(vehicle.transform.position + Vector3.up, neighbour.GetVehicle().transform.position + Vector3.up);
        }
    }

    public enum NpcSoldierState
    {
        Idle,
        Wander,
        Chase
    }

    public override Vehicle GetVehicle()
    {
        return vehicle;
    }
}
