using UnityEngine;
using System.Collections;

public class SelfDestructor : MonoBehaviour
{
	public float Cooldown = 2;

	void Start()
	{

	}

	void Update()
	{
		Cooldown -= Time.deltaTime;
		if (Cooldown < 0)
		{
			Destroy(gameObject);
		}
	}
}
