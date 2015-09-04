using UnityEngine;

public class SoundSensor : MonoBehaviour
{
    private AutonomousAgent owner;

    private void Awake()
    {
        owner = GetComponentInParent<AutonomousAgent>();
    }

    private void OnTriggerEnter(Collider other)
    {
        var autoAgent = other.GetComponentInParent<PlayerController>();
        if (autoAgent != null)
        {
            if (autoAgent.Team == Targeting.GetOpposingTeam(owner.Team))
            {
                var npc = owner.GetComponent<NpcSoldierController>();
                npc.SetTarget(autoAgent.GetVehicle());
                Debug.Log("SET TARGET!");
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {

    }
}