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
        Instantiate(SpawnPrefab, transform.position, transform.rotation);
        Debug.Log("SPAWN!!!");
        StartCoroutine(Spawn(Frequency));
    }
}
