using UnityEngine;

public class SoundSensor : MonoBehaviour
{
    private AutonomousAgent owner;
    private Team opposingTeam;

    private void Awake()
    {
        owner = GetComponentInParent<AutonomousAgent>();
        opposingTeam = Targeting.GetOpposingTeam(owner.Team);
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
                Debug.Log("SET TARGET!");
            }
        }
        else
        {
            var player = other.GetComponentInParent<PlayerController>();
            if (player.Team == opposingTeam)
            {
                var npc = owner.GetComponent<NpcSoldierController>();
                var target = Targeting.FindNearest(opposingTeam, owner.GetVehicle().transform.position, 50f);
                npc.SetTarget(target);
                Debug.Log("SET TARGET!");
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {

    }
}