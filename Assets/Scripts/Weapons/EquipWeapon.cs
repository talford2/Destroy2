using UnityEngine;

public class EquipWeapon : MonoBehaviour
{
	public Transform[] ShootPoints;

    public bool DoNotDestroy = false;
	public float MinLifeTime = 15f;
	private float lifeCooldown = 0;
	
    private Collectible collectible;

	private void Awake()
	{
		collectible = GetComponentInChildren<Collectible>();
		if (collectible != null)
			collectible.OnCollect += OnCollect;

		lifeCooldown = MinLifeTime;
	}

	public void EnableCollect()
	{
		if (collectible != null)
			collectible.Enabled = true;
	}

	public void Update()
	{
		if (collectible.Enabled && !DoNotDestroy)
		{
			lifeCooldown -= Time.deltaTime;
			if (lifeCooldown < 0 && !gameObject.GetComponent<Renderer>().isVisible)
			{
				Destroy(gameObject);
			}
		}
	}

	public void OnCollect()
	{
		Destroy(gameObject);
	}
}
