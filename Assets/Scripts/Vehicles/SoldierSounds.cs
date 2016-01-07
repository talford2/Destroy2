using System.Collections.Generic;
using UnityEngine;

public class SoldierSounds : MonoBehaviour
{
    public GameObject WorldSoundPrefab;

    public List<AudioClip> LeftSteps;
    public List<AudioClip> RightSteps;

    public List<AudioClip> HurtCries;
    public List<AudioClip> AlertCalls;
    public List<AudioClip> RandomSpeech;

    private void Awake()
    {
        var killable = GetComponent<Killable>();
        killable.OnDamage += HurtCry;
    }

    private void RunLeftStep()
    {
        AudioSource.PlayClipAtPoint(LeftSteps[Random.Range(0, LeftSteps.Count)], transform.position);
    }

    private void RunRightStep()
    {
        AudioSource.PlayClipAtPoint(RightSteps[Random.Range(0, RightSteps.Count)], transform.position);
    }

    private void HurtCry(Collider hitcollider, Vector3 position, Vector3 direction, float power, float damage, GameObject attacker)
    {
        WorldSounds.PlayClipAt(transform.position + Vector3.up, HurtCries[Random.Range(0, AlertCalls.Count)]);
    }

    public void AlertCall()
    {
        WorldSounds.PlayClipAt(transform.position + Vector3.up, AlertCalls[Random.Range(0, AlertCalls.Count)]);
    }

    public void RandomSpeak()
    {
        WorldSounds.PlayClipAt(transform.position + Vector3.up, RandomSpeech[Random.Range(0, AlertCalls.Count)]);
    }
}
