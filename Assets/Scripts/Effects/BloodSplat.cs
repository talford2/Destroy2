using UnityEngine;

public class BloodSplat : MonoBehaviour
{
    public GameObject Prefab;

	void Start ()
	{
	    var floorRay = new Ray(transform.position, Vector3.down);
	    RaycastHit floorHit;
	    if (Physics.Raycast(floorRay, out floorHit, 10f))
	    {
            var instance = Instantiate(Prefab);
            instance.transform.position = floorHit.point + Vector3.up * 0.5f;
	    }
	}
}
