using System.Collections.Generic;
using UnityEngine;

public class SoldierSounds : MonoBehaviour
{
    public List<AudioClip> LeftSteps;
    public List<AudioClip> RightSteps;

    public List<AudioClip> HurtCries;

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
        AudioSource.PlayClipAtPoint(HurtCries[Random.Range(0, HurtCries.Count)], transform.position);
    }
}
