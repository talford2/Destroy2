using UnityEngine;
using System.Collections;

public class Spawner : MonoBehaviour
{
    public GameObject SpawnPrefab;
    public float Frequency;
    public bool StartOn = true;
    public bool SpawnOnce;

    private void Awake()
    {
        if (StartOn)
            Trigger(Frequency);
    }

    private IEnumerator Spawn(float delay)
    {
        yield return new WaitForSeconds(delay);
        var targetableCount = Targeting.GetTargetableCount();
        if (targetableCount < 50)
        {
            Instantiate(SpawnPrefab, transform.position, transform.rotation);
            Debug.Log("SPAWN!!! TARGETABLE COUNT: " + targetableCount);
        }
        else
        {
            Debug.Log("DON'T SPAWN, TARGETABLE COUNT: " + targetableCount);
        }
        if (!SpawnOnce)
            Trigger(Frequency);
    }

    public void Trigger(float delay)
    {
        StartCoroutine(Spawn(delay));
    }
}
