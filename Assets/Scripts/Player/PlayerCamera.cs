using UnityEngine;

public class PlayerCamera : MonoBehaviour
{
    public float DistanceCatupSpeed = 15f;
    public Camera Cam;
    public float TargetZoom;

    private float zoomSpeed = 10f;
    private float defaultZoom = 60f;
    private Transform pivotTransform;
    private float targetDistance = 5f;
    private Vector3 offset = new Vector3(0f, 2f, 0);

    private float distance;
    private Vector3 pivotPosition;

    private Vector2 pitchYaw;

    // Weapon Kick
    private Vector2 targetTempPitchYaw;
    private Vector2 tempPitchYaw;

    private CameraMode mode;

    private static PlayerCamera current;

    // Shake
    private float shakeCooldown;
    private float shakeAmplitude;
    private float shakeFrequency = 0.5f;
    private float shakeTotalTime;
    private float shakeInterval;
    private Vector3 curShakePos;
    private Vector3 shakeAt;
    private float shakeMinDistance;
    private float shakeMaxDistance;

    public static PlayerCamera Current
    {
        get { return current; }
    }

    private void Awake()
    {
        current = this;
        distance = targetDistance;
        mode = CameraMode.Chase;
    }

    public void AddPitchYaw(float deltaPitch, float deltaYaw)
    {
        pitchYaw.x -= deltaPitch;
        pitchYaw.y += deltaYaw;
    }

    private bool restorePitchYaw;

    public void AddTemporaryPitchYaw(float deltaPitch, float deltaYaw)
    {
        targetTempPitchYaw.x -= deltaPitch;
        targetTempPitchYaw.y += deltaYaw;
        restorePitchYaw = false;
    }

    public void RestorePitchYaw()
    {
        restorePitchYaw = true;
    }

    public Vector3 GetLookAtPosition()
    {
        var lookRay = Cam.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0f));
        lookRay.origin += lookRay.direction * distance;
        RaycastHit lookHit;
        var lookAt = lookRay.GetPoint(1000f);
        if (Physics.Raycast(lookRay, out lookHit, 1000f, ~LayerMask.GetMask("Player", "Sensors", "MissileSensors")))
        {
            lookAt = lookHit.point;
        }
        return lookAt;
    }

    public void SetPivot(Transform pivot, Vector3 pivotOffset, float pivotDistance)
    {
        offset = pivotOffset;
        targetDistance = pivotDistance;
        if (pivot != pivotTransform)
        {
            var lookAt = GetLookAtPosition();
            var pivotFrom = pivot.position + offset + Quaternion.Euler(pitchYaw.x, pitchYaw.y, 0) * Vector3.forward * -distance;
            var angleTo = Quaternion.LookRotation(lookAt - pivotFrom).eulerAngles;
            pitchYaw = new Vector2(angleTo.x, angleTo.y);
            pivotTransform = pivot;
        }
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

    public CameraMode GetMode()
    {
        return mode;
    }

    private void Chase(float deltaTime)
    {

        tempPitchYaw = Vector2.Lerp(tempPitchYaw, targetTempPitchYaw, 10f * Time.deltaTime);
        var usePitchYaw = pitchYaw + tempPitchYaw;

        Cam.fieldOfView = Mathf.Lerp(Cam.fieldOfView, defaultZoom / TargetZoom, zoomSpeed * Time.deltaTime);
        var lookAngle = Quaternion.Euler(usePitchYaw.x, usePitchYaw.y, 0f);
        if (pivotTransform != null)
            pivotPosition = pivotTransform.position + lookAngle * offset;

        distance = Mathf.Lerp(distance, targetDistance, DistanceCatupSpeed * deltaTime);

        var chasePosition = pivotPosition + lookAngle * Vector3.forward * -distance;

        var camMinY = 0.5f;
        RaycastHit camDownHit;
        if (Physics.Raycast(new Ray(chasePosition + 100f * Vector3.up, Vector3.down), out camDownHit, 100f, ~LayerMask.GetMask("Player", "Sensors", "MissileSensors", "Terrain")))
            camMinY = camDownHit.point.y + 0.5f;

        transform.position = new Vector3(chasePosition.x, Mathf.Clamp(chasePosition.y, camMinY, 100f), chasePosition.z);
        transform.rotation = lookAngle;

        if (restorePitchYaw)
            targetTempPitchYaw = Vector2.Lerp(targetTempPitchYaw, Vector2.zero, 2f * Time.deltaTime);
    }

    private void Aim(float deltaTime)
    {
        Cam.fieldOfView = Mathf.Lerp(Cam.fieldOfView, defaultZoom / TargetZoom, zoomSpeed * Time.deltaTime);
        var lookAngle = Quaternion.Euler(pitchYaw.x, pitchYaw.y, 0f);
        if (pivotTransform != null)
            pivotPosition = pivotTransform.position + lookAngle * offset;

        distance = Mathf.Lerp(distance, 0f, DistanceCatupSpeed * deltaTime);

        transform.position = pivotPosition + lookAngle * Vector3.forward * -distance;
        transform.rotation = Quaternion.Euler(pitchYaw.x, pitchYaw.y, 0f);
    }

    private void Update()
    {
        var totalFrac = shakeCooldown / shakeTotalTime;

        if (shakeCooldown > 0)
        {
            shakeCooldown -= Time.deltaTime;

            shakeInterval -= Time.deltaTime;
            if (shakeInterval < 0)
            {
                curShakePos = GetAmplitude(shakeAt) * totalFrac * Random.onUnitSphere;
                shakeInterval = shakeFrequency;
            }
        }
        else
        {
            curShakePos = Vector3.zero;
        }
        var frac = Mathf.Clamp(1 - (shakeInterval / shakeFrequency), 0f, 1f);
        Cam.transform.localPosition = Vector3.Slerp(Cam.transform.localPosition, curShakePos, frac);
    }

    private float GetAmplitude(Vector3 position)
    {
        var distSquared = (position - Cam.transform.position).sqrMagnitude;
        if (distSquared < shakeMaxDistance * shakeMaxDistance)
            return shakeAmplitude / Mathf.Clamp(distSquared - (shakeMinDistance * shakeMinDistance), 1f, shakeMaxDistance * shakeMaxDistance);
        return 0f;
    }

    public void Shake(Vector3 atPosition, float minDistance, float maxDistance, float time, float amplitude, float frequency)
    {
        shakeAt = atPosition;
        shakeMinDistance = minDistance;
        shakeMaxDistance = maxDistance;
        shakeTotalTime = time;
        shakeCooldown = time;
        shakeAmplitude = amplitude;
        shakeFrequency = frequency;

        curShakePos = GetAmplitude(atPosition) * Random.onUnitSphere;
    }

    public enum CameraMode
    {
        Chase,
        Aim
    }
}
