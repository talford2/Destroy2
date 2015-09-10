using System.Collections;
using UnityEngine;

public class PlayerController : ActorAgent
{
	public Vehicle VehiclePrefab;
	public VehicleGun WeaponPrefab;

	private static PlayerController current;

	public static PlayerController Current
	{
		get { return current; }
	}

	// Vehicle
	private Vehicle vehicle;

	// Aiming
	private float maxAimDistance;
	private Vector3 aimAt;

    // Collecting
    private bool isCollecting;

    // Death
    private Vector3 diedAtPosition;

	private void Awake()
	{
		InitVehicle(VehiclePrefab, WeaponPrefab, transform.position, transform.rotation);

		Cursor.lockState = CursorLockMode.Locked;
		Cursor.visible = false;

		current = this;
	}

	public override void InitVehicle(Vehicle vehiclePrefab, VehicleGun weaponPrefab, Vector3 position, Quaternion rotation)
	{
		vehicle = ((GameObject)Instantiate(vehiclePrefab.gameObject, position, rotation)).GetComponent<Vehicle>();
		vehicle.transform.parent = transform;
		Utility.SetLayerRecursively(vehicle.transform, LayerMask.NameToLayer("Player"));
        vehicle.OnDamage += OnVehicleDamage;
        vehicle.OnDie += OnVehicleDie;

		vehicle.Initialize();

        if (weaponPrefab != null)
            vehicle.SetPrimaryWeapon(weaponPrefab);
        
		Targeting.AddTargetable(Team, vehicle);
	}

    private void OnVehicleDamage(Collider hitCollider, Vector3 position, Vector3 direction, float power, float damage, GameObject attacker)
	{
		HeadsUpDisplay.Current.DamageCooldown = 1;
	}

	private void SetUpCamera()
	{
		if (vehicle != null)
		{
			PlayerCamera.Current.SetPivot(vehicle.DefaultPivot, vehicle.CameraOffset, vehicle.CameraDistance);
            PlayerCamera.Current.TargetZoom = 1f;
            PlayerCamera.Current.DistanceCatupSpeed = 15f;
		}
	}

	private void Start()
	{
		SetUpCamera();
		HeadsUpDisplay.Current.SetCrosshair(vehicle.GetPrimaryWeapon().Crosshair);
	}

	private void Update()
	{
		// Aim Camera
        PlayerCamera.Current.AddPitchYaw(Input.GetAxis("Mouse Y") / PlayerCamera.Current.TargetZoom, Input.GetAxis("Mouse X") / PlayerCamera.Current.TargetZoom);

		// Movement
		if (vehicle != null)
		{
			//vehicle.SetPitchYaw(Input.GetAxis("Mouse Y"), Input.GetAxis("Mouse X"));
			var isZoomed = Input.GetButton("Fire2");
			vehicle.SetRun(Input.GetButton("Fire3"));
			vehicle.SetMove(Input.GetAxis("Vertical"), Input.GetAxis("Horizontal"));
		    isCollecting = Input.GetButton("Collect");

			// Aiming
			maxAimDistance = vehicle.GetPrimaryWeapon().MaxAimDistance;
			var pivotPoint = vehicle.GetPrimaryWeaponShootPoint();

			aimAt = PlayerCamera.Current.GetLookAtPosition();
			var aimRay = new Ray(pivotPoint, aimAt - pivotPoint);
			RaycastHit aimHit;
			var isTargetInSight = false;

			if (Physics.SphereCast(aimRay, vehicle.GetPrimaryWeapon().MissileRadius, out aimHit, maxAimDistance, ~LayerMask.GetMask("Player", "Sensors")))
			{
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
				//PlayerCamera.Current.SetMode(PlayerCamera.CameraMode.Aim);
			    PlayerCamera.Current.SetPivot(vehicle.ZoomPivot, vehicle.CameraOffset, 1f);// vehicle.CameraDistance);
			    PlayerCamera.Current.TargetZoom = vehicle.GetPrimaryWeapon().Zoom;
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
				vehicle.Damage(null, Vector3.zero, Vector3.up, 0f, 1000f, null);
		}
		else
		{
			PlayerCamera.Current.SetMode(PlayerCamera.CameraMode.Chase);
		    PlayerCamera.Current.TargetZoom = 1f;
		}

		if (Input.GetKeyUp(KeyCode.LeftControl))
			Debug.Break();
	}

    public bool IsCollecting()
    {
        return isCollecting;
    }

	public override Vehicle GetVehicle()
	{
		return vehicle;
	}

	private void OnVehicleDie(GameObject attacker)
	{
	    diedAtPosition = vehicle.transform.position;
		Debug.Log("YOU DIED.");
		Targeting.RemoveTargetable(Team, vehicle);
		HeadsUpDisplay.Current.FadeOutCrosshair(1f);
        StartCoroutine(Respawn(5f));
	}

    public void Give(GameObject givePrefab)
    {
        var weapon = givePrefab.GetComponent<VehicleGun>();
        if (weapon != null)
        {
            vehicle.SetPrimaryWeapon(weapon);
            HeadsUpDisplay.Current.SetCrosshair(vehicle.GetPrimaryWeapon().Crosshair);
        }
    }

    private IEnumerator Respawn(float delay)
    {
        yield return new WaitForSeconds(delay);
        SceneManager.FindNearestSpawner(diedAtPosition).Trigger(VehiclePrefab, WeaponPrefab);
        HeadsUpDisplay.Current.ShowCrosshair();
    }

	private void OnDrawGizmos()
	{
		Gizmos.color = Color.red;
		Gizmos.DrawSphere(aimAt, 0.5f);
	}
}
