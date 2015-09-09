using UnityEngine;

public class NpcSoldierController : AutonomousAgent
{
    public Vehicle VehiclePrefab;
    public VehicleGun WeaponPrefab;

    [Header("Chase Behaviour")]
    public float TargetReconsiderRate;
    public float AttackDistance;
    public float AimAngleTolerance = 5f;

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

    // Chase / Attack
    private Killable target;
    private float targetReconsiderCooldown;

    // Death
    private Vector3 killPosition;
    private Vector3 killDirection;
    private float killPower;

    private void Awake()
    {
        steering = new SteeringBehaviour(this);
        InitVehicle(VehiclePrefab.gameObject);

        target = Targeting.FindNearest(Targeting.GetOpposingTeam(Team), vehicle.transform.position, 100f);
        state = NpcSoldierState.Wander;

        AssignWanderPosition();
    }

    private void InitVehicle(GameObject prefab)
    {
        vehicle = ((GameObject) Instantiate(prefab, transform.position, transform.rotation)).GetComponent<Vehicle>();
        vehicle.transform.parent = transform;
        vehicle.OnDamage += OnVehicleDamage;
        vehicle.OnDie += OnVehicleDie;
        vehicle.Initialize();

        if (WeaponPrefab != null)
            vehicle.SetPrimaryWeapon(WeaponPrefab);

        Targeting.AddTargetable(Team, vehicle);
        var neighrbourSensor = GetComponentInChildren<NeighbourSensor>();
        if (neighrbourSensor != null)
            neighrbourSensor.transform.parent = vehicle.transform;
        var soundSensor = GetComponentInChildren<SoundSensor>();
        if (soundSensor != null)
            soundSensor.transform.parent = vehicle.transform;
    }

    public override void SetTarget(Killable value)
    {
        target = value;
        if (value != null)
        {
            Debug.Log("SET TARGET TO: " + value.name);
            state = NpcSoldierState.Chase;
        }
    }

    public override bool HasTarget()
    {
        return target != null;
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
        else
        {
            path = new[] {vehicle.transform.position};
        }
        curPathIndex = 0;
    }

    private Vector3 GetWanderForce()
    {
        var groupForces = steering.CalculateGroupForces(GetNeighbours());

        var steerForce = Vector3.zero;

        steerForce += 1f*groupForces.SeparationForce;
        if (steerForce.sqrMagnitude > 1f)
            return steerForce.normalized;

        steerForce += 0.3f*groupForces.CohesiveForce;
        if (steerForce.sqrMagnitude > 1f)
            return steerForce.normalized;

        steerForce += 0.3f*groupForces.AlignmentForce;
        if (steerForce.sqrMagnitude > 1f)
            return steerForce.normalized;

        // Destination Force
        steerForce += 1f*steering.SeekForce(path[curPathIndex]);
        if (steerForce.sqrMagnitude > 1f)
            return steerForce.normalized;

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
        vehicle.SetAimAt(path[curPathIndex] + vehicle.PathAimHeight*Vector3.up + vehicle.transform.forward);
        vehicle.SetRun(false);
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
            //vehicle.SetMove(0f, 0f);
        }

        if (HasTarget())
        {
            state = NpcSoldierState.Chase;
        }
    }

    private void AssignChasePosition()
    {
        Vehicle targetVehicle = null;
        if (target != null)
            targetVehicle = target.GetComponent<Vehicle>();
        if (targetVehicle != null)
        {
            var targetPosition = targetVehicle.transform.position;
            var toTarget = targetPosition - vehicle.transform.position;
            var chaseTo = targetPosition - 10f*toTarget.normalized;

            var navPath = new NavMeshPath();
            if (NavMesh.CalculatePath(vehicle.transform.position, chaseTo, NavMesh.AllAreas, navPath))
            {
                path = navPath.corners;
                curPathIndex = 0;
            }
            else
            {
                path = new[] {vehicle.transform.position};
                curPathIndex = 0;
            }
        }
    }

    private Vector3 GetChaseForce()
    {
        var groupForces = steering.CalculateGroupForces(GetNeighbours());

        var steerForce = Vector3.zero;

        steerForce += 1f*groupForces.SeparationForce;
        if (steerForce.sqrMagnitude > 1f)
            return steerForce.normalized;

        steerForce += 0.3f*groupForces.CohesiveForce;
        if (steerForce.sqrMagnitude > 1f)
            return steerForce.normalized;

        steerForce += 0.3f*groupForces.AlignmentForce;
        if (steerForce.sqrMagnitude > 1f)
            return steerForce.normalized;

        // Destination Force
        steerForce += 1f*steering.SeekForce(path[curPathIndex]);
        if (steerForce.sqrMagnitude > 1f)
            return steerForce.normalized;
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
                    curPathIndex = 0;
                }
            }
        }

        if (reachedPathEnd)
        {
            reachedPathEnd = false;
        }

        var chaseForce = GetChaseForce();

        Vehicle targetVehicle = null;
        if (HasTarget())
            targetVehicle = target.GetComponent<Vehicle>();
        if (targetVehicle != null)
        {
            var pitchYaw = GetSteerToPoint(targetVehicle.transform.position);
            vehicle.SetPitchYaw(0f, pitchYaw.y);

            var toTarget = targetVehicle.transform.position - GetVehicle().transform.position;
            if (toTarget.sqrMagnitude < AttackDistance*AttackDistance)
            {
                var aimAt = targetVehicle.transform.position + targetVehicle.PathAimHeight*Vector3.up + GetAimRadius(toTarget.sqrMagnitude)*Random.insideUnitSphere;
                var shootFrom = vehicle.GetPrimaryWeaponShootPoint();
                var aimRay = new Ray(shootFrom, aimAt - shootFrom);

                vehicle.SetAimAt(aimAt);

                var angleToTarget = Vector3.Angle(aimRay.direction, vehicle.GetPrimaryWeaponAimDirection());
                if (Mathf.Abs(angleToTarget) < AimAngleTolerance)
                {
                    // Avoid shooting friends
                    RaycastHit aimHit;
                    var dontShoot = false;
                    if (Physics.Raycast(aimRay, out aimHit, AttackDistance, ~LayerMask.GetMask("Sensors")))
                    {
                        var npc = aimHit.collider.GetComponentInParent<NpcSoldierController>();
                        if (npc != null)
                        {
                            if (npc.Team == Team)
                                dontShoot = true;
                        }
                    }

                    if (!dontShoot)
                    {
                        vehicle.TriggerPrimaryWeapon();
                    }
                    else
                    {
                        vehicle.ReleasePrimaryWeapon();
                    }
                }
                else
                {
                    vehicle.ReleasePrimaryWeapon();
                }
                vehicle.SetRun(false);
            }
            else
            {
                vehicle.SetAimAt(path[curPathIndex] + vehicle.PathAimHeight*Vector3.up + vehicle.transform.forward);
                vehicle.ReleasePrimaryWeapon();
                vehicle.SetRun(true);
            }
        }
        else
        {
            vehicle.ReleasePrimaryWeapon();
        }

        var forward = Vector3.Dot(chaseForce, vehicle.transform.forward);
        var strafe = Vector3.Dot(chaseForce, vehicle.transform.right);
        vehicle.SetMove(forward, strafe);

        // Reconsider target
        if (targetReconsiderCooldown > 0f)
        {
            targetReconsiderCooldown -= Time.deltaTime;
            if (targetReconsiderCooldown < 0f)
            {
                target = Targeting.FindNearest(Targeting.GetOpposingTeam(Team), vehicle.transform.position, 100f);
                targetReconsiderCooldown = TargetReconsiderRate;
            }
        }

        if (!HasTarget())
        {
            path = null;
            state = NpcSoldierState.Wander;
        }
    }

    private float GetAimRadius(float distanceSquared)
    {
        if (distanceSquared < 5f*5f)
            return 0;
        if (distanceSquared < 30f*30f)
            return 1*distanceSquared/(30f*30f);
        return 1f;
    }

    private void OnVehicleDamage(Collider hitCollider, Vector3 position, Vector3 direction, float power, float damage, GameObject attacker)
    {
        if (attacker != null)
        {
            var targetCandidate = attacker.GetComponentInParent<ActorAgent>();
            if (targetCandidate != null)
            {
                if (targetCandidate.Team == Targeting.GetOpposingTeam(Team))
                {
                    target = targetCandidate.GetVehicle();
                    SetTarget(target);
                    AlertNeighbours(target);
                }
            }
        }
    }

    private void OnVehicleDie(GameObject attacker)
    {
        Targeting.RemoveTargetable(Team, vehicle);
        Destroy(gameObject);
    }

    private void AlertNeighbours(Killable attacker)
    {
        foreach (var neighbour in GetNeighbours())
        {
            if (neighbour != null)
            {
                var autoAgent = neighbour.GetComponent<AutonomousAgent>();
                if (autoAgent != null)
                {
                    if (!autoAgent.HasTarget())
                        autoAgent.SetTarget(attacker);
                }
            }
        }
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