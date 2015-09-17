using UnityEngine;

[RequireComponent(typeof (SphereCollider))]
public class MissileSensor : MonoBehaviour
{
    private SphereCollider triggerCollider;
    private AutonomousAgent owner;
    private Team opposingTeam;

    private void Awake()
    {
        triggerCollider = GetComponent<SphereCollider>();
        owner = GetComponentInParent<AutonomousAgent>();
        opposingTeam = Targeting.GetOpposingTeam(owner.Team);
    }

    public void Trigger(Missile missile)
    {
        Debug.Log("MISSILE SENSOR TRIGGERED!!!!");
        var missileOwner = missile.GetOwner();
        if (missileOwner != null)
        {
            var missileOwnerActor = missileOwner.GetComponent<ActorAgent>();
            if (missileOwnerActor != null)
            {
                if (missileOwnerActor.Team == opposingTeam)
                {
                    var target = Targeting.FindNearest(opposingTeam, owner.GetVehicle().transform.position, triggerCollider.radius);
                    owner.SetTarget(target);
                }
            }
        }
    }
}