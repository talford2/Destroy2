using System.Collections;
using UnityEngine;

public class WorldSound : MonoBehaviour {

    public void PlayClip(AudioClip clip)
    {
        var source = GetComponent<AudioSource>();
        source.clip = clip;
        source.Play();
        StartCoroutine(SelfDestruction(clip.length));
    }

    private IEnumerator SelfDestruction(float time)
    {
        yield return new WaitForSeconds(time);
        Destroy(gameObject);
    }
}
