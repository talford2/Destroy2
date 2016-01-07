using UnityEngine;

public class PodSpawner : MonoBehaviour
{
    public Spawner TriggerSpawner;

    private bool hasSpawned;

    private void Awake()
    {
        hasSpawned = false;
    }

    private void Update()
    {
        if (!hasSpawned)
        {
            var hitRay = new Ray(transform.position, Vector3.down);
            if (Physics.Raycast(hitRay, 0.5f, LayerMask.GetMask("Terrain")))
            {
                TriggerSpawner.Trigger(0.2f);
                hasSpawned = true;
            }
        }
    }
}
