using UnityEngine;

public class PlayerController : ActorAgent
{
    public Vehicle VehiclePrefab;

    private static PlayerController current;

    public static PlayerController Current
    {
        get { return current; }
    }

    // Vehicle
    private Vehicle vehicle;

    // Aiming
    private float maxAimDistance;
    private Vector3 pivotPoint;
    private Vector2 pitchYaw;
    private Vector3 aimAt;

    private void Awake()
    {
        InitVehicle(VehiclePrefab.gameObject);

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        current = this;
    }

    private void InitVehicle(GameObject prefab)
    {
        vehicle = ((GameObject)Instantiate(prefab, transform.position, transform.rotation)).GetComponent<Vehicle>();
        vehicle.transform.parent = transform;
        Utility.SetLayerRecursively(vehicle.transform, LayerMask.NameToLayer("Player"));
        vehicle.OnVehicleDestroyed += OnDie;
        vehicle.Initialize();
        Targeting.AddTargetable(Team, vehicle);
    }

    private void SetUpCamera()
    {
        if (vehicle != null)
        {
            PlayerCamera.Current.SetPivot(vehicle.transform, vehicle.CameraOffset, vehicle.CameraDistance);
        }
    }

    private void Start()
    {
        SetUpCamera();
        HeadsUpDisplay.Current.SetCrosshair(vehicle.GetPrimaryWeapon().Crosshair);
    }

    private void Update()
    {
        // Movement
        if (vehicle != null)
        {
            vehicle.SetPitchYaw(Input.GetAxis("Mouse Y"), Input.GetAxis("Mouse X"));
            var isZoomed = Input.GetButton("Fire2");
            vehicle.SetRun(Input.GetButton("Fire3"));
            vehicle.SetMove(Input.GetAxis("Vertical"), Input.GetAxis("Horizontal"));

            // Aiming
            maxAimDistance = vehicle.GetPrimaryWeapon().MaxAimDistance;
            pivotPoint = vehicle.transform.position + vehicle.CameraOffset;
            pitchYaw = vehicle.GetPitchYaw();

            aimAt = pivotPoint + Quaternion.Euler(pitchYaw.x, pitchYaw.y, 0f)*Vector3.forward*maxAimDistance;
            var aimRay = new Ray(pivotPoint, aimAt - pivotPoint);
            RaycastHit aimHit;
            var isTargetInSight = false;

            if (Physics.SphereCast(aimRay, 0.5f, out aimHit, maxAimDistance, ~LayerMask.GetMask("Player", "Sensors")))
            {
                aimAt = aimRay.GetPoint(aimHit.distance);
                var aimKillable = aimHit.collider.GetComponentInParent<Killable>();
                if (aimKillable != null)
                {
                    isTargetInSight = true;
                }
            }

            // This is incorrect?
            vehicle.SetAimAt(aimAt);

            HeadsUpDisplay.Current.SetTargetInSight(isTargetInSight);

            if (isZoomed)
            {
                PlayerCamera.Current.SetMode(PlayerCamera.CameraMode.Aim);
                PlayerCamera.Current.SetPivot(vehicle.ZoomPoint, Vector3.zero, 0f);
            }
            else
            {
                PlayerCamera.Current.SetMode(PlayerCamera.CameraMode.Chase);
                SetUpCamera();
            }

            // Shooting
            if (Input.GetButton("Fire1") && !vehicle.IsRun())
            {
                vehicle.TriggerPrimaryWeapon();
            }
            else
            {
                vehicle.ReleasePrimaryWeapon();
            }
            Heading = vehicle.transform.forward;

            if (Input.GetKeyUp(KeyCode.Q))
                vehicle.Damage(null, Vector3.zero, Vector3.zero, 0f, 1000f, null);
        }
        else
        {
            PlayerCamera.Current.SetMode(PlayerCamera.CameraMode.Chase);
            pitchYaw += new Vector2(-Input.GetAxis("Mouse Y"), Input.GetAxis("Mouse X"));
            aimAt = pivotPoint + Quaternion.Euler(Mathf.DeltaAngle(-pitchYaw.x, 0f), Mathf.DeltaAngle(-pitchYaw.y, 0f), 0f) * Vector3.forward * maxAimDistance;
        }

        PlayerCamera.Current.AddPitchYaw(Input.GetAxis("Mouse Y"), Input.GetAxis("Mouse X"));

        if (Input.GetKeyUp(KeyCode.LeftControl))
            Debug.Break();
    }

    public override Vehicle GetVehicle()
    {
        return vehicle;
    }

    private void OnDie()
    {
        Debug.Log("YOU DIED.");
        Targeting.RemoveTargetable(Team, vehicle);
        HeadsUpDisplay.Current.FadeOutCrosshair(1f);
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawSphere(aimAt, 0.5f);
    }
}
