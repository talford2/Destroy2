using UnityEngine;
using UnityEngine.UI;

public class HeadsUpDisplay : MonoBehaviour
{
	public Image Crosshair;

	public Image Damage;

	private bool isTargetInSight;
	private bool isFadeOutCrosshair;
	private float fadeOutTime;
	private float fadeOutCooldown;

	private static HeadsUpDisplay current;

	public static HeadsUpDisplay Current
	{
		get { return current; }
	}

	private void Awake()
	{
		isTargetInSight = false;
		isFadeOutCrosshair = false;
		current = this;
	}

	public void SetCrosshair(Sprite value)
	{
		Crosshair.sprite = value;
	}

	public float DamageCooldown = 0;
	private void Update()
	{
		Crosshair.color = isTargetInSight ? Color.red : Color.white;
		if (isFadeOutCrosshair)
		{
			if (fadeOutCooldown > 0f)
			{
				fadeOutCooldown -= Time.deltaTime;
				Crosshair.color = new Color(Crosshair.color.r, Crosshair.color.g, Crosshair.color.b, fadeOutCooldown / fadeOutTime);
				if (fadeOutCooldown < 0f)
				{
					Crosshair.enabled = false;
				}
			}
		}

		DamageCooldown -= Time.deltaTime;

		if (DamageCooldown > 0)
		{
			Damage.color = new Color(1, 1, 1, DamageCooldown);
		}
		else
		{
			DamageCooldown = 0;
		}
	}

	public void SetTargetInSight(bool value)
	{
		isTargetInSight = value;
	}

	public void FadeOutCrosshair(float time)
	{
		fadeOutTime = time;
		fadeOutCooldown = time;
		isFadeOutCrosshair = true;
	}
}