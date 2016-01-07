using UnityEngine;

public class Pod : Vehicle {
    [Header("Primary Weapon")]
    public VehicleGun PrimaryWeaponPrefab;
    public Transform[] PrimaryWeaponShootPoints;

    [Header("Spawner")]
    public PlayerSpawner TriggerSpawner;
    public Vehicle VehiclePrefab;
    public VehicleGun WeaponPrefab;

    private bool hasSpawned;
    private VehicleGun primaryWeapon;

    public override void LiveUpdate()
    {
        if (!hasSpawned)
        {
            HeadsUpDisplay.Current.HideCrosshair();
            var hitRay = new Ray(transform.position, Vector3.down);
            RaycastHit hit;
            if (Physics.Raycast(hitRay, out hit, 0.5f, LayerMask.GetMask("Terrain")))
            {
                transform.position = hit.point;
                TriggerSpawner.TriggerAndDestroy(VehiclePrefab, WeaponPrefab);
                hasSpawned = true;
                Targeting.RemoveTargetable(Team.Good, GetComponent<Killable>());
                Destroy(this);
                GetComponent<Rigidbody>().isKinematic = true;
            }
        }
    }

    public override void Initialize()
    {
        SetPrimaryWeapon(PrimaryWeaponPrefab);
    }

    public override void SetMove(float forward, float strafe)
    {
    }

    public override void SetPitchYaw(float pitch, float yaw)
    {
    }

    public override Vector2 GetPitchYaw()
    {
        return Vector2.zero;
    }

    public override void SetCrouch(bool value)
    {
    }

    public override void SetRun(bool value)
    {
    }

    public override bool IsRun()
    {
        return true;
    }

    public override void SetAimAt(Vector3 position)
    {
    }

    public override Vector3 GetPrimaryWeaponShootPoint()
    {
        return Vector3.zero;
    }

    public override Vector3 GetPrimaryWeaponAimDirection()
    {
        return Vector3.zero;
    }

    public override void SetPrimaryWeapon(VehicleGun value)
    {
        if (primaryWeapon != null)
            Destroy(primaryWeapon.gameObject);
        primaryWeapon = Instantiate(value).GetComponent<VehicleGun>();
        //primaryWeapon.SetVelocityReference(new VelocityReference { Value = velocity });
        primaryWeapon.InitGun(PrimaryWeaponShootPoints, gameObject);
        primaryWeapon.SetClipRemaining(100);
        //primaryWeapon.OnFinishReload += OnReloadPrimaryFinish;
        primaryWeapon.transform.parent = transform;
        primaryWeapon.transform.localPosition = Vector3.zero;

        if (HeadsUpDisplay.Current != null)
        {
            HeadsUpDisplay.Current.SetCrosshair(primaryWeapon.Crosshair);
        }
    }

    public override VehicleGun GetPrimaryWeapon()
    {
        return primaryWeapon;
    }

    public override void TriggerPrimaryWeapon()
    {
    }

    public override void ReloadPrimaryWeapon(int rounds)
    {
    }

    public override void ReleasePrimaryWeapon()
    {
    }

    public override void AlertNeighbours()
    {
    }

    public override void Die(GameObject attacker)
    {
        Destroy(primaryWeapon.gameObject);
        base.Die(attacker);
    }
}
