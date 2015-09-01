using UnityEngine;

public abstract class Killable : MonoBehaviour
{
    public bool IsLive = true;
    public float MaxHealth;
    public float Health;

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

    public virtual void Damage(Vector3 position, Vector3 direction, float power, float damage, GameObject attacker)
    {
        if (IsLive)
            ApplyDamage(damage, attacker);
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
