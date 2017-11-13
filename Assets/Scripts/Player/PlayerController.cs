﻿using System.Collections;
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
		vehicle = Instantiate(vehiclePrefab.gameObject, position, rotation).GetComponent<Vehicle>();
		vehicle.transform.parent = transform;
		Utility.SetLayerRecursively(vehicle.transform, LayerMask.NameToLayer("Player"));
        vehicle.OnDamage += OnVehicleDamage;
        vehicle.OnDie += OnVehicleDie;

		vehicle.Initialize();

        if (weaponPrefab != null)
            vehicle.SetPrimaryWeapon(weaponPrefab);
	    vehicle.MaxHealth *= 2f;
        vehicle.Health = vehicle.MaxHealth;
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

    public void OnTargetHit()
    {
        HeadsUpDisplay.Current.ShowHit(0.2f);
    }

    private void Update()
    {
        // Aim Camera
        PlayerCamera.Current.AddPitchYaw(Input.GetAxis("Mouse Y")/PlayerCamera.Current.TargetZoom, Input.GetAxis("Mouse X")/PlayerCamera.Current.TargetZoom);

        // Movement
        if (vehicle != null)
        {
            var isRun = Input.GetButton("Fire3");
            //vehicle.SetPitchYaw(Input.GetAxis("Mouse Y"), Input.GetAxis("Mouse X"));
            var isZoomed = Input.GetButton("Fire2");
            var move = new Vector2(Input.GetAxis("Horizontal"), Input.GetAxis("Vertical"));
            vehicle.SetRun(isRun);
            vehicle.SetMove(move.y, move.x);
            vehicle.SetCrouch(Input.GetButton("Crouch"));
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

            if (isRun && move.sqrMagnitude > 0.1f)
            {
                HeadsUpDisplay.Current.FadeOutCrosshair(0.2f);
            }
            else
            {
                HeadsUpDisplay.Current.FadeInCrosshair(0.2f);
                //HeadsUpDisplay.Current.ShowCrosshair();
            }

            if (!isRun && isZoomed)
            {
                //PlayerCamera.Current.SetMode(PlayerCamera.CameraMode.Aim);
                PlayerCamera.Current.SetPivot(vehicle.ZoomPivot, vehicle.CameraOffset, 1f); // vehicle.CameraDistance);
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
                if (!isRun)
                {
                    HeadsUpDisplay.Current.ShowNormal();
                }
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

            if (isDead && Input.GetButton("Fire1"))
            {
                Debug.LogFormat("TIMEDIFF: {0:f2}", (Time.time - timeOfDeath));
                if ((Time.time - timeOfDeath) > 3f)
                {
                    isDead = false;
                    StartCoroutine(Respawn(0f));
                }
            }
        }

        if (Input.GetKeyUp(KeyCode.LeftControl))
            Debug.Break();

        if (Input.GetKeyUp(KeyCode.Escape))
            Application.Quit();
    }

    public bool IsCollecting()
    {
        return isCollecting;
    }

	public override Vehicle GetVehicle()
	{
		return vehicle;
	}

    private float timeOfDeath;
    private bool isDead;

	private void OnVehicleDie(GameObject attacker)
	{
	    diedAtPosition = vehicle.transform.position;
		Debug.Log("YOU DIED.");
		Targeting.RemoveTargetable(Team, vehicle);
		HeadsUpDisplay.Current.FadeOutCrosshair(0.5f);
        timeOfDeath = Time.time;
        isDead = true;
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
                droppedRigidBody.AddRelativeTorque(Vector3.up * 5f - Vector3.right * 25f);
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
}
