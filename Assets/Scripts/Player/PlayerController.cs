using UnityEngine;

public class PlayerController : MonoBehaviour
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
        vehicle.Initialize();
        PlayerCamera.Current.FocusTransform = vehicle.transform;
    }

    private void Update()
    {
        var walker = vehicle as Walker;
        if (walker != null)
        {
            // Movement
            walker.SetPitchYaw(Input.GetAxis("Mouse Y"), Input.GetAxis("Mouse X"));
            walker.SetMove(Input.GetAxis("Vertical"), Input.GetAxis("Horizontal"));

            var pitchTarget = Mathf.Clamp(walker.GetPitchTarget(), -40f, 80f);
            PlayerCamera.Current.SetTargetPitchYaw(pitchTarget, walker.GetYawTarget());

            //controller.Move(velocity*Time.deltaTime);
            //transform.rotation = Quaternion.Lerp(transform.rotation, Quaternion.Euler(0f, yawTarget, 0f), 25f*Time.deltaTime);

            // Aiming
            var targetRay = PlayerCamera.Current.GetComponent<Camera>().ViewportPointToRay(screenCentre);
            RaycastHit targetHit;
            var isTargetInSight = false;
            if (Physics.Raycast(targetRay, out targetHit, MaxAimDistance, ~LayerMask.GetMask("Player")))
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

            walker.SetAimAt(aimAt);

            HeadsUpDisplay.Current.SetTargetInSight(isTargetInSight);

            // Shooting
            if (Input.GetButton("Fire1"))
            {
                walker.TriggerPrimaryWeapon();
            }
            else
            {
                walker.ReleasePrimaryWeapon();
            }
        }
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawSphere(aimAt, 0.5f);
    }
}
