using UnityEngine;

public class NpcSoldierController : Killable
{
    public Collider HeadCollider;
    public GameObject CorpsePrefab;

    private NpcSoldierState lastState;
    private NpcSoldierState state;
    private Vector3 killPosition;
    private Vector3 killDirection;
    private float killPower;

    private void Awake()
    {
        state = NpcSoldierState.Idle;
    }

    public override void LiveUpdate()
    {
        lastState = state;
    }

    public override void Damage(Collider hitCollider, Vector3 position, Vector3 direction, float power, float damage, GameObject attacker)
    {
        killPosition = position;
        killDirection = direction;
        killPower = power;
        if (hitCollider == HeadCollider)
        {
            Debug.Log("HEAD DAMAGE!");
            ApplyDamage(damage * 10f, attacker);
        }
        else
        {
            base.Damage(hitCollider, position, direction, power, damage, attacker);
        }
    }

    public override void Damage(Collider hitCollider, Vector3 position, Vector3 direction, Missile missile)
    {
        killPosition = position;
        killDirection = direction;
        killPower = missile.GetPower();
        if (hitCollider == HeadCollider)
        {
            Debug.Log("HEADSHOT!");
            ApplyDamage(missile.GetDamage()*10f, missile.GetOwner());
        }
        else
        {
            base.Damage(hitCollider, position, direction, missile);
        }
    }

    public override void Die(GameObject attacker)
    {
        var colliders = GetComponentsInChildren<Collider>();
        foreach (var curCollider in colliders)
        {
            curCollider.enabled = false;
        }
        var corpse = (GameObject)Instantiate(CorpsePrefab, transform.position, transform.rotation);
        corpse.transform.parent = SceneManager.CorpseContainer;
        var corpseColliders = corpse.GetComponentsInChildren<Collider>();
        foreach (var corpseCollider in corpseColliders)
        {
            var killRay = new Ray(killPosition - 5f*killDirection.normalized, killDirection);
            RaycastHit killHit;
            if (corpseCollider.Raycast(killRay, out killHit, 10f))
            {
                Debug.Log("KILLED!");
                corpseCollider.attachedRigidbody.AddForceAtPosition(killDirection.normalized * killPower, killHit.point, ForceMode.Impulse);
            }
        }

        base.Die(attacker);
        Destroy(gameObject);
    }

    public enum NpcSoldierState
    {
        Idle,
        Wander
    }
}
