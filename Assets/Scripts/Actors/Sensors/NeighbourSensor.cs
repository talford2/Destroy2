using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof (SphereCollider))]
public class NeighbourSensor : MonoBehaviour
{
    private AutonomousAgent owner;

    private void Awake()
    {
        owner = GetComponentInParent<AutonomousAgent>();
    }

    private void OnTriggerEnter(Collider other)
    {
        var autoAgent = other.GetComponentInParent<ActorAgent>();
        if (autoAgent != null)
            owner.AddNeighbour(autoAgent);
        var cover = other.GetComponentInParent<Cover>();
        if (cover != null)
            owner.AddDetectedCover(cover);
    }

    private void OnTriggerExit(Collider other)
    {
        var autoAgent = other.GetComponentInParent<ActorAgent>();
        if (autoAgent != null)
            owner.RemoveNeighbour(autoAgent);
        var cover = other.GetComponentInParent<Cover>();
        if (cover != null)
            owner.RemoveDetectedCover(cover);
    }
}
