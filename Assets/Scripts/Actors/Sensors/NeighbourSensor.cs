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
    }

    private void OnTriggerExit(Collider other)
    {
        var autoAgent = other.GetComponentInParent<ActorAgent>();
        if (autoAgent != null)
            owner.RemoveNeighbour(autoAgent);
    }
}
