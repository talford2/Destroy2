using System.Collections.Generic;
using UnityEngine;

public class SoldierSounds : MonoBehaviour
{
    public GameObject WorldSoundPrefab;

    public List<AudioClip> LeftSteps;
    public List<AudioClip> RightSteps;

    public List<AudioClip> HurtCries;
    public List<AudioClip> SightCalls;

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
        var worldSound = ((GameObject)Instantiate(WorldSoundPrefab, transform.position + Vector3.up, Quaternion.identity)).GetComponent<WorldSound>();
        worldSound.PlayClip(HurtCries[Random.Range(0, HurtCries.Count)]);
    }

    public void SightCall()
    {
        var worldSound = ((GameObject)Instantiate(WorldSoundPrefab, transform.position + Vector3.up, Quaternion.identity)).GetComponent<WorldSound>();
        worldSound.PlayClip(SightCalls[Random.Range(0, SightCalls.Count)]);
    }
}
