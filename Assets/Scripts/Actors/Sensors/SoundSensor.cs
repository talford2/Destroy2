using UnityEngine;

[RequireComponent(typeof(SphereCollider))]
public class SoundSensor : MonoBehaviour
{
    private SphereCollider triggerCollider;
    private AutonomousAgent owner;
    private Team opposingTeam;

    private void Start()
    {
        triggerCollider = GetComponent<SphereCollider>();
        owner = GetComponentInParent<AutonomousAgent>();
        opposingTeam = Targeting.GetOpposingTeam(owner.Team);
    }

    private void OnTriggerStay(Collider other)
    {
        var agent = other.GetComponentInParent<ActorAgent>();
        if (agent != null)
        {
            if (agent.Team == opposingTeam)
            {
                var target = Targeting.FindNearest(opposingTeam, owner.GetVehicle().transform.position, triggerCollider.radius);
                owner.SetTarget(target);
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        var autoAgent = other.GetComponentInParent<ActorAgent>();
        if (autoAgent != null)
        {
            if (!owner.HasTarget())
            {
                if (autoAgent.Team == opposingTeam)
                {
                    var target = Targeting.FindNearest(opposingTeam, owner.GetVehicle().transform.position, triggerCollider.radius);
                    owner.SetTarget(target);
                }
            }
        }
    }
}