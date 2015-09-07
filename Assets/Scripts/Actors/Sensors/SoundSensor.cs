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

    private void OnTriggerStay(Collider other)
    {
        if (!owner.HasTarget())
        {
            var target = Targeting.FindNearest(opposingTeam, owner.GetVehicle().transform.position, 50f);
            owner.SetTarget(target);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        var autoAgent = other.GetComponentInParent<AutonomousAgent>();
        if (autoAgent != null)
        {
            if (autoAgent.Team == opposingTeam)
            {
                var npc = owner.GetComponent<NpcSoldierController>();
                var target = Targeting.FindNearest(opposingTeam, owner.GetVehicle().transform.position, 50f);
                npc.SetTarget(target);
            }
        }
        else
        {
            var player = other.GetComponentInParent<PlayerController>();
            if (player != null)
            {
                if (player.Team == opposingTeam)
                {
                    var npc = owner.GetComponent<NpcSoldierController>();
                    var target = Targeting.FindNearest(opposingTeam, owner.GetVehicle().transform.position, 50f);
                    npc.SetTarget(target);
                }
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {

    }
}