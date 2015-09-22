using System.Collections;
using UnityEngine;

public class PlayerController : ActorAgent
{
	public Vehicle VehiclePrefab;
	public VehicleGun WeaponPrefab;
    public int KillCount { get; set; }

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
	    var reloadCrosshair = vehicle.GetPrimaryWeapon().ReloadCrosshair;
        if (reloadCrosshair!=null)
            HeadsUpDisplay.Current.SetReloadCrosshair(reloadCrosshair);
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
			var targetInSightType = HeadsUpDisplay.InSightType.None;

			if (Physics.SphereCast(aimRay, vehicle.GetPrimaryWeapon().MissileRadius, out aimHit, maxAimDistance, ~LayerMask.GetMask("Player", "Sensors", "MissileSensors")))
			{
				var aimKillable = aimHit.collider.GetComponentInParent<ActorAgent>();
				if (aimKillable != null)
				{
				    targetInSightType = aimKillable.Team == Team
                        ? HeadsUpDisplay.InSightType.Friendly
                        : HeadsUpDisplay.InSightType.Enemy;
				}
			}

			// This is incorrect?
			vehicle.SetAimAt(aimAt);

            HeadsUpDisplay.Current.SetTargetInSight(targetInSightType);

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
		    if (!vehicle.GetPrimaryWeapon().GetIsReloading())
		    {
		        if (Input.GetButton("Fire1") && !vehicle.IsRun())
		        {
		            vehicle.TriggerPrimaryWeapon();
		        }
		        else
		        {
		            vehicle.ReleasePrimaryWeapon();
		        }
		        if (Input.GetButton("Reload"))
		        {
		            vehicle.ReloadPrimaryWeapon(vehicle.GetPrimaryWeapon().ClipCapacity - vehicle.GetPrimaryWeapon().GetClipRemaining());
		        }
                HeadsUpDisplay.Current.ShowNormal();
		    }
		    else
		    {
		        if (vehicle.GetPrimaryWeapon().ReloadCrosshair != null)
		        {
		            HeadsUpDisplay.Current.ShowReload();
		            HeadsUpDisplay.Current.SetReloadProgress(vehicle.GetPrimaryWeapon().GetReloadProgress());
		        }
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

    private IEnumerator EnableDropped(EquipWeapon droppedWeapon, float delay)
    {
        yield return new WaitForSeconds(delay);
        droppedWeapon.EnableCollect();
    }

    public void Give(GameObject givePrefab)
    {
        var weapon = givePrefab.GetComponent<VehicleGun>();
        if (weapon != null)
        {
            // Drop current weapon
            var solderVehicle = vehicle.GetComponent<Soldier>();
            if (solderVehicle != null)
            {
                var equipped = solderVehicle.GetEquipped();
                var dropEquipped = ((GameObject)Instantiate(vehicle.GetPrimaryWeapon().EquipPrefab, equipped.transform.position, equipped.transform.rotation)).GetComponent<EquipWeapon>();
                dropEquipped.GetComponent<MeshCollider>().enabled = true;
                var droppedRigidBody = dropEquipped.GetComponent<Rigidbody>();
                droppedRigidBody.isKinematic = false;
                droppedRigidBody.AddForce(Vector3.up*5f + vehicle.transform.right*5f, ForceMode.Impulse);
                StartCoroutine(EnableDropped(dropEquipped, 1f));
            }
            vehicle.SetPrimaryWeapon(weapon);
            HeadsUpDisplay.Current.SetCrosshair(vehicle.GetPrimaryWeapon().Crosshair);
            HeadsUpDisplay.Current.SetReloadCrosshair(vehicle.GetPrimaryWeapon().ReloadCrosshair);
        }
    }

    private IEnumerator Respawn(float delay)
    {
        yield return new WaitForSeconds(delay);
        SceneManager.FindSafeSpawner(diedAtPosition).Trigger(VehiclePrefab, WeaponPrefab);
        //SceneManager.FindNearestSpawner(diedAtPosition).Trigger(VehiclePrefab, WeaponPrefab);
        HeadsUpDisplay.Current.SetCrosshair(vehicle.GetPrimaryWeapon().Crosshair);
        HeadsUpDisplay.Current.ShowCrosshair();
    }

	private void OnDrawGizmos()
	{
		Gizmos.color = Color.red;
		Gizmos.DrawSphere(aimAt, 0.5f);
	}

    private void OnGUI()
    {
        var ammoStyle = new GUIStyle
        {
            alignment = TextAnchor.MiddleRight,
            normal = {textColor = Color.white},
            fontSize = 20
        };
        GUI.Label(new Rect(Screen.width - 120f, Screen.height - 40f, 100f, 30f), string.Format("{0} / {1}", vehicle.GetPrimaryWeapon().GetClipRemaining(), vehicle.GetPrimaryWeapon().ClipCapacity), ammoStyle);
        GUI.Label(new Rect(Screen.width - 120f, 20f, 100f, 30f), string.Format("{0}", KillCount), ammoStyle);
    }
}
