using UnityEngine;

public class HitResponse : MonoBehaviour
{
    public GameObject HitEffect;

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.impulse.sqrMagnitude > 200f)
        {
            foreach (var contact in collision.contacts)
            {
                Instantiate(HitEffect, contact.point, Quaternion.LookRotation(contact.normal));
            }
        }
    }
}
