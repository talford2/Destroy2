using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[RequireComponent(typeof (SphereCollider))]
public class SoundSensor : MonoBehaviour
{
    public float CheckFrequency = 2f;

    private SphereCollider triggerCollider;
    private AutonomousAgent owner;
    private Team opposingTeam;

    private List<Collider> lastColliders;

    private void Start()
    {
        triggerCollider = GetComponent<SphereCollider>();
        owner = GetComponentInParent<AutonomousAgent>();
        opposingTeam = Targeting.GetOpposingTeam(owner.Team);

        lastColliders = new List<Collider>();
        StartCoroutine(CheckInside(Random.Range(1f, 2f)));
    }

    private IEnumerator CheckInside(float delay)
    {
        yield return new WaitForSeconds(delay);
        var recentColliders = Physics.OverlapSphere(transform.position, triggerCollider.radius, LayerMask.GetMask("Detectable")).ToList();

        var enteredColliders = recentColliders.Except(lastColliders).ToList();
        var exitedColliders = lastColliders.Except(recentColliders).ToList();

        if (enteredColliders.Count > 0 || exitedColliders.Count > 0)
        {
            if (!owner.HasTarget())
            {
                var target = Targeting.FindNearest(opposingTeam, owner.GetVehicle().transform.position, triggerCollider.radius);
                owner.SetTarget(target);
            }
        }

        lastColliders = recentColliders;
        StartCoroutine(CheckInside(CheckFrequency));
    }

    /*
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
    */
}