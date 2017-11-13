using UnityEngine;

public class CameraShake : MonoBehaviour
{
    //public float Range = 5f;
    public float AngleRange = 2f;
    public float Frequency = 8f;

    private bool _isTriggered;
    private float _duration;
    private float _durationCooldown;
    private float _magntidueFraction;

    private Vector3 _atPosition;
    private float _maxDistance;
    private float _minDistance;
    private float _maxAmplitude;

    public void ShakeUpdate()
    {
        if (_isTriggered)
        {
            if (_durationCooldown >= 0f)
            {
                _durationCooldown -= Time.deltaTime;
                _magntidueFraction = Mathf.Clamp01(_durationCooldown / _duration)*GetAmplitudeFraction(_atPosition);
                if (_durationCooldown < 0f)
                {
                    _magntidueFraction = 0f;
                    _isTriggered = false;
                }
            }
            var t = Frequency * Time.time;

            transform.localPosition = new Vector3(
                Mathf.Clamp01(Mathf.PerlinNoise(t, 0f)) - 0.5f,
                Mathf.Clamp01(Mathf.PerlinNoise(t, 10f)) - 0.5f,
                Mathf.Clamp01(Mathf.PerlinNoise(t, 20f)) - 0.5f) * _maxAmplitude * _magntidueFraction;

            transform.localRotation = Quaternion.Euler(
                Mathf.PerlinNoise(t, 0f) * AngleRange * _magntidueFraction,
                Mathf.PerlinNoise(t, 0f) * AngleRange * _magntidueFraction,
                Mathf.PerlinNoise(t, 0f) * AngleRange * _magntidueFraction);

            //transform.localPosition = new Vector3(0, Mathf.Sin(Time.time * 2f) * 8f, 0);
        }
    }

    public void Trigger(float duration, Vector3 atPosition, float maxDistance, float minDistance, float maxAmplitude)
    {
        _isTriggered = true;
        _duration = duration;
        _durationCooldown = _duration;

        _atPosition = atPosition;
        _maxDistance = maxDistance;
        _minDistance = minDistance;
        _maxAmplitude = maxAmplitude;
    }

    private float GetAmplitudeFraction(Vector3 position)
    {
        var delta = position - transform.position;
        return 1f - Mathf.InverseLerp(_minDistance, _maxDistance, delta.magnitude);
    }
}
