using UnityEngine;

public class NpcSoldierController : Killable
{
    public GameObject CorpsePrefab;

    private Vector3 killPosition;
    private Vector3 killDirection;
    private float killPower;

    private void Awake()
    {
    }

    public override void LiveUpdate()
    {
    }

    public override void Damage(Vector3 position, Vector3 direction,float power, float damage, GameObject attacker)
    {
        killPosition = position;
        killDirection = direction;
        killPower = power;
        base.Damage(position, direction, power, damage, attacker);
    }

    public override void Die(GameObject attacker)
    {
        var colliders = GetComponentsInChildren<Collider>();
        foreach (var curCollider in colliders)
        {
            curCollider.enabled = false;
        }
        var corpse = (GameObject)Instantiate(CorpsePrefab, transform.position, transform.rotation);

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
}
