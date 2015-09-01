using UnityEngine;

public class NpcSoldierController : Killable
{
    public GameObject CorpsePrefab;

    private void Awake()
    {
    }

    public override void LiveUpdate()
    {
    }

    public override void Die(GameObject attacker)
    {
        var colliders = GetComponentsInChildren<Collider>();
        foreach (var curCollider in colliders)
        {
            curCollider.enabled = false;
        }
        var corpse = (GameObject)Instantiate(CorpsePrefab, transform.position, transform.rotation);
        base.Die(attacker);
        Destroy(gameObject);
    }
}
