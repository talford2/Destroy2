using System;
using UnityEngine;
using UnityEngine.UI;

public class HeadsUpDisplay : MonoBehaviour
{
	public Image Crosshair;
    public Image ReloadCrosshair;

	public Image Damage;

	private InSightType inSightType;
    private bool isFriendlyInSight;
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
		inSightType = InSightType.None;
		isFadeOutCrosshair = false;
		current = this;
	}

	public void SetCrosshair(Sprite value)
	{
		Crosshair.sprite = value;
	}

    public void SetReloadCrosshair(Sprite value)
    {
        ReloadCrosshair.sprite = value;
    }

	public float DamageCooldown = 0;
	private void Update()
	{
	    switch (inSightType)
	    {
	        case InSightType.Enemy:
	            Crosshair.color = Color.red;
	            break;
	        case InSightType.Friendly:
	            Crosshair.color = new Color(0.5f, 0.7f, 1f, 1f);
	            break;
	        default:
	            Crosshair.color = Color.white;
	            break;
	    }
	    if (isFadeOutCrosshair)
		{
			if (fadeOutCooldown > 0f)
			{
				fadeOutCooldown -= Time.deltaTime;
                Crosshair.color = new Color(Crosshair.color.r, Crosshair.color.g, Crosshair.color.b, fadeOutCooldown / fadeOutTime);
                ReloadCrosshair.color = new Color(Crosshair.color.r, Crosshair.color.g, Crosshair.color.b, fadeOutCooldown / fadeOutTime);
				if (fadeOutCooldown < 0f)
				{
					Crosshair.enabled = false;
				    ReloadCrosshair.enabled = false;
				}
			}
		}
		
		if (DamageCooldown > 0)
		{
			DamageCooldown -= Time.deltaTime;
		}
		if (DamageCooldown < 0)
		{
			DamageCooldown = 0;
		}
		
		Damage.color = new Color(1, 1, 1, Mathf.Clamp(DamageCooldown, 0, 1));
	}

    public void ShowReload()
    {
        Crosshair.enabled = false;
        ReloadCrosshair.enabled = true;
    }

    public void ShowNormal()
    {
        ReloadCrosshair.enabled = false;
        Crosshair.enabled = true;
    }

    public void SetReloadProgress(float progress)
    {
        ReloadCrosshair.fillAmount = Mathf.Clamp01(progress);
    }

	public void SetTargetInSight(InSightType value)
	{
		inSightType = value;
	}

	public void FadeOutCrosshair(float time)
	{
		fadeOutTime = time;
		fadeOutCooldown = time;
		isFadeOutCrosshair = true;
	}

    public void ShowCrosshair()
    {
        Crosshair.color = Color.white;
        ReloadCrosshair.color = Color.white;
        Crosshair.enabled = true;
        ReloadCrosshair.enabled = false;
    }

    public enum InSightType
    {
        None,
        Enemy,
        Friendly
    }
}