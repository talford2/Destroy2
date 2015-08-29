using UnityEngine;

public class PlayerCamera : MonoBehaviour
{
    public Transform FocusTransform;
    public Vector3 Offset;

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

    // Not sure if this will be used yet?
    public void SetTargetPitchYaw(float pitch, float yaw)
    {
        targetPitch = pitch;
        targetYaw = yaw;
    }

    private void LateUpdate()
    {
        var targetPosition = FocusTransform.position + Quaternion.Euler(targetPitch, targetYaw, 0)*Offset;
        transform.position = Vector3.Lerp(transform.position, targetPosition, 25f*Time.deltaTime);
        transform.LookAt(FocusTransform.position);
    }
}
