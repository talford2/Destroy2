using UnityEngine;
using System.Collections;

public class LightFade : MonoBehaviour
{
	public Light FadeOutLight;

	private float initIntensity;

	public float Cooldown = 1f;

	private float coolDownVal = 0;

	void Start()
	{
		initIntensity = FadeOutLight.intensity;
		coolDownVal = Cooldown;
	}

	void Update()
	{
		if (coolDownVal > 0)
		{
			coolDownVal -= Time.deltaTime;
			FadeOutLight.intensity = initIntensity * (coolDownVal / Cooldown);
		}
		else
		{
			FadeOutLight.intensity = 0;
		}
	}
}
