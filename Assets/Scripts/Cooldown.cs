public class Cooldown
{
    private float _duration;
    private float _remaining;

    public delegate void OnCooldownUpdate(float remainingTime, float duration);
    public delegate void OnCooldownFinish();

    public OnCooldownUpdate OnUpdate;
    public OnCooldownFinish OnFinish;

    public Cooldown()
    {
        Stop();
    }

    public void Trigger(float duration)
    {
        _duration = duration;
        _remaining = _duration;
    }

    public void Update(float deltatime)
    {
        if (_remaining >= 0f)
        {
            _remaining -= deltatime;
            if (OnUpdate != null)
                OnUpdate(_remaining, _duration);
            if (_remaining < 0f)
            {
                if (OnFinish != null)
                    OnFinish();
            }
        }
    }

    public void Stop()
    {
        _remaining = -1f;
    }
}
