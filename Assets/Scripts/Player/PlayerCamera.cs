using UnityEngine;

public class PlayerCamera : MonoBehaviour
{
    public Transform FocusTransform;
    public float Distance = 5f;
    public Vector3 Offset = new Vector3(0f, 2f, 0);
    public float CatchupSpeed = 25f;

    private float targetPitch;
    private float targetYaw;

    private static PlayerCamera current;

    public static PlayerCamera Current
    {
        get { return current; }
    }

    private void Awake()
    {
        current = this;
    }

    public void SetTargetPitchYaw(float pitch, float yaw)
    {
        targetPitch = pitch;
        targetYaw = yaw;
    }

    private void LateUpdate()
    {
        var targetPosition = FocusTransform.position + Quaternion.Euler(targetPitch, targetYaw, 0)*Vector3.forward*-Distance;
        targetPosition = new Vector3(targetPosition.x, Mathf.Clamp(targetPosition.y, 1f, 100f), targetPosition.z);

        transform.position = Vector3.Lerp(transform.position, targetPosition, CatchupSpeed*Time.deltaTime);
        transform.LookAt(FocusTransform.position + Offset);
    }
}
