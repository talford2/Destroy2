using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[RequireComponent(typeof (SphereCollider))]
public class NeighbourSensor : MonoBehaviour
{
    public float CheckFrequency = 0.5f;
    private AutonomousAgent owner;

    private SphereCollider triggerCollider;
    private List<Collider> lastColliders;

    private void Awake()
    {
        owner = GetComponentInParent<AutonomousAgent>();

        triggerCollider = GetComponent<SphereCollider>();
        lastColliders = new List<Collider>();

        StartCoroutine(CheckInside(Random.Range(0.1f, 0.5f)));
    }

    private IEnumerator CheckInside(float delay)
    {
        yield return new WaitForSeconds(delay);
        var recentColliders = Physics.OverlapSphere(transform.position, triggerCollider.radius, LayerMask.GetMask("Detectable")).ToList();

        var enteredColliders = recentColliders.Except(lastColliders).ToList();
        var exitedColliders = lastColliders.Except(recentColliders).ToList();

        foreach (var enteredCollider in enteredColliders)
        {
            var detectable = enteredCollider.GetComponent<Detectable>();
            var agent = detectable.GetOwner();
            if (agent != null)
            {
                owner.AddNeighbour(agent);
            }
            var cover = detectable.GetCover();
            if (cover != null)
            {
                owner.AddDetectedCover(cover);
            }
        }

        foreach (var exitedCollider in exitedColliders)
        {
            if (exitedCollider != null)
            {
                var detectable = exitedCollider.GetComponent<Detectable>();
                var agent = detectable.GetOwner();
                if (agent != null)
                {
                    owner.RemoveNeighbour(agent);
                }
                var cover = detectable.GetCover();
                if (cover != null)
                {
                    owner.RemoveDetectedCover(cover);
                }
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
                var autoAgent = detectable.GetOwner();
                if (autoAgent != null)
                    owner.AddNeighbour(autoAgent);
                var cover = detectable.GetCover();
                if (cover != null)
                {
                    Debug.Log("FOUND COVER!");
                    owner.AddDetectedCover(cover);
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
                owner.RemoveNeighbour(autoAgent);
            var cover = detectable.GetCover();
            if (cover != null)
                owner.RemoveDetectedCover(cover);
        }
        inColliders.Remove(other);
    }
    */
}
