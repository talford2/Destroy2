using UnityEngine;

public class PlayerCamera : MonoBehaviour
{
	public Transform PivotTransform;
	public float Distance = 5f;
	public Vector3 Offset = new Vector3(0f, 2f, 0);
	public float CatchupSpeed = 25f;

	public Camera Cam;


	private float distance;
	private Vector3 focusPosition;

	private Vector3 lookAtPosition;

	private CameraMode mode;

	private static PlayerCamera current;

	public static PlayerCamera Current
	{
		get { return current; }
	}

	private void Awake()
	{
		current = this;
		distance = Distance;
		mode = CameraMode.Chase;
	}

	public void SetLookAt(Vector3 position)
	{
		lookAtPosition = position;
	}

	private void LateUpdate()
	{
		switch (mode)
		{
			case CameraMode.Chase:
				Chase(Time.deltaTime);
				break;
			case CameraMode.Aim:
				Aim(Time.deltaTime);
				break;
		}
	}

	public void SetMode(CameraMode value)
	{
		mode = value;
	}

	private void Chase(float deltaTime)
	{
		if (PivotTransform != null)
			focusPosition = PivotTransform.position + Offset;

		distance = Mathf.Lerp(distance, Distance, deltaTime);

		var camMinY = 1f;
		RaycastHit camDownHit;
		if (Physics.Raycast(new Ray(transform.position, Vector3.down), out camDownHit, 100f, ~LayerMask.GetMask("Player", "Sensors")))
			camMinY = camDownHit.point.y + 0.5f;

		var lookAngle = Quaternion.LookRotation(lookAtPosition - (focusPosition + Offset));
		var targetPitch = lookAngle.eulerAngles.x;
		var targetYaw = lookAngle.eulerAngles.y;

		var targetPosition = focusPosition + Quaternion.Euler(targetPitch, targetYaw, 0) * Vector3.forward * -distance;
		targetPosition = new Vector3(targetPosition.x, Mathf.Clamp(targetPosition.y, camMinY, 100f), targetPosition.z);

		transform.position = Vector3.Lerp(transform.position, targetPosition, CatchupSpeed * deltaTime);
	    var targetRotation = Quaternion.LookRotation(lookAtPosition - transform.position);
	    transform.rotation = Quaternion.Lerp(transform.rotation, targetRotation, CatchupSpeed*deltaTime);
		transform.LookAt(lookAtPosition);
	}

	private void Aim(float deltaTime)
	{
		if (PivotTransform != null)
			focusPosition = PivotTransform.position;

		var targetPosition = focusPosition;

		transform.position = Vector3.Lerp(transform.position, targetPosition, CatchupSpeed * deltaTime);
		transform.LookAt(lookAtPosition);
	}

	private float shakeCooldown = 0f;
	private float shakeAmplitude = 0f;
	private void Update()
	{
		if (shakeCooldown > 0)
		{
			Cam.transform.localPosition = shakeAmplitude * Random.insideUnitSphere;
			shakeCooldown -= Time.deltaTime;
		}
	}

	public void Shake(float time, float amplitude)
	{
		shakeCooldown = time;
		shakeAmplitude = amplitude;
	}

	public enum CameraMode
	{
		Chase,
		Aim
	}
}
