using UnityEngine;

public class Cover : MonoBehaviour
{
    public float Radius;

    void OnDrawGizmos()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, Radius);
    }
}
