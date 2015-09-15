using UnityEngine;
using System.Collections;

public class Spawner : MonoBehaviour
{
    public GameObject SpawnPrefab;
    public float Frequency;


    private void Awake()
    {
        StartCoroutine(Spawn(Frequency));
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
        StartCoroutine(Spawn(Frequency));
    }
}
