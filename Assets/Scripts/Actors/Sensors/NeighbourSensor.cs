using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof (SphereCollider))]
public class NeighbourSensor : MonoBehaviour
{
    private AutonomousAgent owner;

    private List<Collider> inColliders;
    //private List<Collider> outColliders;

    private void Awake()
    {
        owner = GetComponentInParent<AutonomousAgent>();
        inColliders = new List<Collider>();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!inColliders.Contains(other))
        {
            inColliders.Add(other);
            var autoAgent = other.GetComponentInParent<ActorAgent>();
            if (autoAgent != null)
                owner.AddNeighbour(autoAgent);
            var cover = other.GetComponentInParent<Cover>();
            if (cover != null)
            {
                Debug.Log("FOUND COVER!");
                owner.AddDetectedCover(cover);
            }
        }
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
