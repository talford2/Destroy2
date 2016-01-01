using System.Collections.Generic;
using UnityEngine;

public class SoldierSteps : MonoBehaviour
{
    public List<AudioClip> LeftSteps;
    public List<AudioClip> RightSteps;

    private void RunLeftStep()
    {
        AudioSource.PlayClipAtPoint(LeftSteps[Random.Range(0, LeftSteps.Count)], transform.position);
    }

    private void RunRightStep()
    {
        AudioSource.PlayClipAtPoint(RightSteps[Random.Range(0, RightSteps.Count)], transform.position);
    }
}
