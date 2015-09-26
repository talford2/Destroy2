using UnityEngine;

public abstract class Corpse : MonoBehaviour
{
	public float MinLifeTime = 10f;
	private float lifeCooldown = 0;

	public Renderer RenderObject;

	public Transform FocalPoint;

	private void Awake()
	{
		lifeCooldown = MinLifeTime;
	}

	public void Update()
	{
		if (RenderObject != null)
		{
			lifeCooldown -= Time.deltaTime;
			if (lifeCooldown < 0 && !RenderObject.isVisible)
			{
				Destroy(gameObject);
			}
		}
	}
}
