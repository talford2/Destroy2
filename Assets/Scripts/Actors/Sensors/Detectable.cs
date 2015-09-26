using UnityEngine;

[RequireComponent(typeof(SphereCollider))]
public class Detectable : MonoBehaviour
{
    private ActorAgent owner;
    private Cover cover;

    private void Start()
    {
        owner = GetComponentInParent<ActorAgent>();
        cover = GetComponentInParent<Cover>();
    }

    public ActorAgent GetOwner()
    {
        return owner;
    }

    public Cover GetCover()
    {
        return cover;
    }
}
