using UnityEngine;

public class PlayerController : ActorAgent
{
    public Vehicle VehiclePrefab;

    public float MaxAimDistance = 1000f;

    private static PlayerController current;

    public static PlayerController Current
    {
        get { return current; }
    }

    // Vehicle
    private Vehicle vehicle;

    // Aiming
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
        vehicle.gameObject.layer = LayerMask.NameToLayer("Player");
        vehicle.OnVehicleDestroyed += OnDie;
        vehicle.Initialize();
        Targeting.AddTargetable(Team, vehicle);
    }

    private void SetUpCamera()
    {
        if (vehicle != null)
        {
            PlayerCamera.Current.FocusTransform = vehicle.transform;
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

        var pitchYawTarget = vehicle.GetPitchYaw();
        var pitchTarget = Mathf.Clamp(pitchYawTarget.x, -40f, 80f);
        PlayerCamera.Current.SetTargetPitchYaw(pitchTarget, pitchYawTarget.y);

        // Aiming
        var targetRay = PlayerCamera.Current.GetComponent<Camera>().ViewportPointToRay(screenCentre);
        RaycastHit targetHit;
        var isTargetInSight = false;
        if (Physics.Raycast(targetRay, out targetHit, MaxAimDistance, ~LayerMask.GetMask("Player", "Sensors")))
        {
            aimAt = targetHit.point;
            var aimKillable = targetHit.collider.GetComponentInParent<Killable>();
            if (aimKillable != null)
            {
                isTargetInSight = true;
            }
        }
        else
        {
            aimAt = targetRay.GetPoint(MaxAimDistance);
        }

        vehicle.SetAimAt(aimAt);

        HeadsUpDisplay.Current.SetTargetInSight(isTargetInSight);
        if (vehicle.IsLive)
        {
            if (isZoomed)
            {
                PlayerCamera.Current.SetMode(PlayerCamera.CameraMode.Aim);
                PlayerCamera.Current.FocusTransform = vehicle.ZoomPoint;
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
