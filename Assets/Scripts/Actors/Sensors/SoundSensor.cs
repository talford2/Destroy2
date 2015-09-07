using UnityEngine;

public class SoundSensor : MonoBehaviour
{
    private AutonomousAgent owner;
    private Team opposingTeam;

    private void Start()
    {
        owner = GetComponentInParent<AutonomousAgent>();
        opposingTeam = Targeting.GetOpposingTeam(owner.Team);
    }

    private void OnTriggerEnter(Collider other)
    {
        var agent = other.GetComponentInParent<ActorAgent>();
        if (agent != null)
        {
            if (agent.Team == opposingTeam)
            {
                var target = Targeting.FindNearest(opposingTeam, owner.GetVehicle().transform.position, 50f);
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
                    var target = Targeting.FindNearest(opposingTeam, owner.GetVehicle().transform.position, 50f);
                    owner.SetTarget(target);
                }
            }
        }
    }
}