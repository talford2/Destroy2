using UnityEngine;

public class PlayerCamera : MonoBehaviour
{
    public Transform FocusTransform;
    public float Distance = 5f;
    public Vector3 Offset = new Vector3(0f, 2f, 0);
    public float CatchupSpeed = 25f;

    private float distance;
    private float targetPitch;
    private float targetYaw;
    private Vector3 focusPosition;

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

    public void SetTargetPitchYaw(float pitch, float yaw)
    {
        targetPitch = pitch;
        targetYaw = yaw;
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
        Debug.Log("CHASE");
        if (FocusTransform != null)
            focusPosition = FocusTransform.position;

        distance = Mathf.Lerp(distance, Distance, deltaTime);

        var targetPosition = focusPosition + Quaternion.Euler(targetPitch, targetYaw, 0) * Vector3.forward * -distance;
        targetPosition = new Vector3(targetPosition.x, Mathf.Clamp(targetPosition.y, 1f, 100f), targetPosition.z);

        transform.position = Vector3.Lerp(transform.position, targetPosition, CatchupSpeed * deltaTime);
        transform.LookAt(focusPosition + Offset);
    }

    private void Aim(float deltaTime)
    {
        Debug.Log("AIM!");
        if (FocusTransform != null)
            focusPosition = FocusTransform.position;

        var targetPosition = focusPosition;
        targetPosition = new Vector3(targetPosition.x, Mathf.Clamp(targetPosition.y, 1f, 100f), targetPosition.z);

        transform.position =Vector3.Lerp(transform.position, targetPosition, CatchupSpeed * deltaTime);
        var lookAt = Quaternion.Euler(targetPitch, targetYaw, 0);
        transform.rotation = Quaternion.Lerp(transform.rotation, lookAt, 5f*deltaTime);
    }

    public enum CameraMode
    {
        Chase,
        Aim
    }
}
