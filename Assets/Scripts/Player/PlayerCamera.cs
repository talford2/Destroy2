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
	private float frequency = 0.5f;

	private float shakeInterval = 0;

	private Vector3 prevShakePos = Vector3.zero;
	private Vector3 curShakePos = Vector3.zero;

	private void Update()
	{
		var frac = Mathf.Clamp(1 - (shakeInterval / frequency), 0, 1);

		if (shakeCooldown > 0)
		{
			shakeCooldown -= Time.deltaTime;

			shakeInterval -= Time.deltaTime;
			if (shakeInterval < 0)
			{
				prevShakePos = curShakePos;
				curShakePos = shakeAmplitude * Random.insideUnitSphere;
				//Cam.transform.localPosition = shakeAmplitude * Random.insideUnitSphere;
				shakeInterval = frequency;
			}
		}
		else
		{
			curShakePos = Vector3.zero;
		}

		Cam.transform.localPosition = Vector3.Lerp(prevShakePos, curShakePos, frac);
	}

	public void Shake(float time, float amplitude)
	{
		shakeCooldown = time;
		shakeAmplitude = amplitude;

		curShakePos = amplitude * Random.insideUnitSphere;
		//prevShakePos = Vector3.zero;
	}

	public enum CameraMode
	{
		Chase,
		Aim
	}
}
