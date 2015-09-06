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
    private Vector3 crosshairAt;
    private Vector3 screenCentre;
    private Vector3 aimAt;

    private void Awake()
    {
        InitVehicle(VehiclePrefab.gameObject);

        screenCentre = new Vector3(0.5f, 0.5f, 0f);

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
            PlayerCamera.Current.PivotTransform = vehicle.transform;
            PlayerCamera.Current.Distance = vehicle.CameraDistance;
            PlayerCamera.Current.Offset = vehicle.CameraOffset;
        }
    }

    private void Start()
    {
        SetUpCamera();
    }

    private void Update()
    {
        // Movement
        vehicle.SetPitchYaw(Input.GetAxis("Mouse Y"), Input.GetAxis("Mouse X"));
        var isZoomed = Input.GetButton("Fire2");
        vehicle.SetRun(Input.GetButton("Fire3"));
        vehicle.SetMove(Input.GetAxis("Vertical"), Input.GetAxis("Horizontal"));

        // Aiming
        var maxAimDistance = vehicle.GetPrimaryWeapon().MaxAimDistance;
        crosshairAt = vehicle.transform.position + vehicle.CameraOffset + Quaternion.Euler(Mathf.DeltaAngle(-vehicle.GetPitchYaw().x, 0f), Mathf.DeltaAngle(-vehicle.GetPitchYaw().y, 0f), 0f) * Vector3.forward * maxAimDistance;
        var aimRay = new Ray(vehicle.transform.position + vehicle.CameraOffset, crosshairAt - (vehicle.transform.position + vehicle.CameraOffset));
        RaycastHit aimHit;
        var isTargetInSight = false;

        if (Physics.Raycast(aimRay, out aimHit, maxAimDistance, ~LayerMask.GetMask("Player", "Sensors")))
        {
            crosshairAt = aimHit.point;
            var aimKillable = aimHit.collider.GetComponentInParent<Killable>();
            if (aimKillable != null)
            {
                isTargetInSight = true;
            }
        }

        PlayerCamera.Current.SetLookAt(crosshairAt);

        // This is incorrect!
        vehicle.SetAimAt(crosshairAt);

        HeadsUpDisplay.Current.SetTargetInSight(isTargetInSight);

        if (vehicle.IsLive)
        {
            if (isZoomed)
            {
                PlayerCamera.Current.SetMode(PlayerCamera.CameraMode.Aim);
                PlayerCamera.Current.PivotTransform = vehicle.ZoomPoint;
            }
            else
            {
                PlayerCamera.Current.SetMode(PlayerCamera.CameraMode.Chase);
                SetUpCamera();
            }
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

        if (Input.GetKeyUp(KeyCode.LeftControl))
        {
            Debug.Break();
        }
        if (vehicle != null)
            Heading = vehicle.transform.forward;
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
        Gizmos.color = Color.cyan;
        Gizmos.DrawSphere(crosshairAt, 0.5f);
    }
}
