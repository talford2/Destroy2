using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(SphereCollider))]
public class SoundSensor : MonoBehaviour
{
    private SphereCollider triggerCollider;
    private AutonomousAgent owner;
    private Team opposingTeam;

    private List<Collider> inColliders;

    private void Start()
    {
        triggerCollider = GetComponent<SphereCollider>();
        owner = GetComponentInParent<AutonomousAgent>();
        opposingTeam = Targeting.GetOpposingTeam(owner.Team);

        inColliders = new List<Collider>();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!inColliders.Contains(other))
        {
            inColliders.Add(other);
            var detectable = other.GetComponent<Detectable>();
            if (detectable != null)
            {
                var agent = detectable.GetOwner();
                if (agent != null)
                {
                    if (agent.Team == opposingTeam)
                    {
                        var target = Targeting.FindNearest(opposingTeam, owner.GetVehicle().transform.position, triggerCollider.radius);
                        owner.SetTarget(target);
                    }
                }
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        var detectable = other.GetComponent<Detectable>();
        if (detectable != null)
        {
            var autoAgent = detectable.GetOwner();
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
        inColliders.Remove(other);
    }
}