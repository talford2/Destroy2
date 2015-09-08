using UnityEngine;

[RequireComponent(typeof (Killable))]
public class HealthRengerator : MonoBehaviour
{
    [Tooltip("The rate the Killable's health increases when regenerating (Health/Second)")]
    public float RegenerateRate = 5f;
    [Tooltip("The delay after being damaged before health starts regenerating (Seconds)")]
    public float RegenerateDelay = 3f;

    private Killable killable;
    private float regenerateDelayCooldown;

    private void Awake()
    {
        killable = GetComponent<Killable>();
        killable.OnDamage += OnKillableDamage;
    }

    private void OnKillableDamage(Collider hitCollider, Vector3 position, Vector3 direction, float power, float damage, GameObject attacker)
    {
        regenerateDelayCooldown = RegenerateDelay;
    }

    private void Update()
    {
        if (killable.Health < killable.MaxHealth)
        {
            regenerateDelayCooldown -= Time.deltaTime;
            if (regenerateDelayCooldown < 0f)
            {
                killable.Health += RegenerateRate*Time.deltaTime;
                if (killable.Health > killable.MaxHealth)
                    killable.Health = killable.MaxHealth;
            }
        }
    }
}
