using UnityEngine;

public abstract class Corpse : MonoBehaviour
{
	public float MinLifeTime = 10f;
	private float lifeCooldown = 0;

	public Transform FocalPoint;

	private void Awake()
	{
		lifeCooldown = MinLifeTime;
	}

	public void Update()
	{
		lifeCooldown -= Time.deltaTime;
		if (lifeCooldown < 0 && !gameObject.GetComponent<Renderer>().isVisible)
		{
			Destroy(gameObject);
		}
	}
}
