using UnityEngine;

public abstract class Killable : MonoBehaviour
{
    [Header("Killable")]
    public bool IsLive = true;
    public float MaxHealth;
    public float Health;

    public delegate void OnDamageEvent(Collider hitCollider, Vector3 position, Vector3 direction, float power, float damage, GameObject attacker);
    public event OnDamageEvent OnDamage;

    public delegate void OnDieEvent(GameObject attacker);
    public event OnDieEvent OnDie;

    private void Awake()
    {
    }

    private void Update()
    {
        if (IsLive)
        {
            LiveUpdate();
        }
    }

    public abstract void LiveUpdate();

    public virtual void Damage(Collider hitCollider, Vector3 position, Vector3 direction, float power, float damage, GameObject attacker)
    {
        if (IsLive)
        {
            ApplyDamage(damage, attacker);
            if (OnDamage != null)
                OnDamage(hitCollider, position, direction, power, damage, attacker);
        }
    }

    public virtual void Damage(Collider hitCollider, Vector3 position, Vector3 direction, Missile missile)
    {
        if (IsLive)
        {
            ApplyDamage(missile.GetDamage(), missile.GetOwner());
            if (OnDamage != null)
                OnDamage(hitCollider, position, direction, missile.GetPower(), missile.GetDamage(), missile.GetOwner());
        }
    }

    protected void ApplyDamage(float damage, GameObject attacker)
    {
        Health -= damage;
        if (Health <= 0)
        {
            IsLive = false;
            Die(attacker);
        }
    }

    public virtual void Die(GameObject attacker)
    {
        if (OnDie != null)
            OnDie(attacker);
    }
}
