using UnityEngine;
using UnityEngine.UI;

public class HeadsUpDisplay : MonoBehaviour
{
	public Image Crosshair;
    public Image ReloadCrosshair;
    public Image HitCrosshair;
	public Image Damage;

	private InSightType inSightType;
    private bool isFriendlyInSight;
	private bool isFadeCrosshair;
    private int fadeDirection;
	private float fadeOutTime;
	private float fadeOutCooldown;

    private bool isShowHit;
    private float hitCooldown;

	private static HeadsUpDisplay current;

	public static HeadsUpDisplay Current
	{
		get { return current; }
	}

	private void Awake()
	{
		inSightType = InSightType.None;
		isFadeCrosshair = false;
		current = this;
	    HitCrosshair.enabled = false;
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
	            Crosshair.color = new Color(1f, 0f, 0f, Crosshair.color.a);
	            break;
	        case InSightType.Friendly:
	            Crosshair.color = new Color(0.1f, 0.3f, 1f, Crosshair.color.a);
	            break;
	        default:
	            Crosshair.color = new Color(1f, 1f, 1f, Crosshair.color.a);
	            break;
	    }

	    if (isFadeCrosshair)
		{
			if (fadeOutCooldown > 0f)
			{
				fadeOutCooldown -= Time.deltaTime;
			    float fadeFraction;
			    if (fadeDirection < 0)
			    {
			        fadeFraction = Mathf.Clamp01(fadeOutCooldown/fadeOutTime);
			    }
			    else
			    {
                    fadeFraction = 1f - Mathf.Clamp01(fadeOutCooldown / fadeOutTime);
			    }
                Crosshair.color = new Color(Crosshair.color.r, Crosshair.color.g, Crosshair.color.b, fadeFraction);
                ReloadCrosshair.color = new Color(Crosshair.color.r, Crosshair.color.g, Crosshair.color.b, fadeFraction);
				if (fadeOutCooldown < 0f)
				{
				    isFadeCrosshair = false;
				    //Crosshair.enabled = false;
				    //ReloadCrosshair.enabled = false;
				}
			}
		}

	    if (hitCooldown >= 0f)
	    {
	        hitCooldown -= Time.deltaTime;
	        if (hitCooldown < 0)
	        {
	            HitCrosshair.enabled = false;
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
        if (!isFadeCrosshair && fadeDirection != -1)
        {
            fadeOutTime = time;
            fadeOutCooldown = time;
            fadeDirection = -1;
            isFadeCrosshair = true;
        }
    }

    public void FadeInCrosshair(float time)
    {
        if (!isFadeCrosshair && fadeDirection != 1)
        {
            fadeOutTime = time;
            fadeOutCooldown = time;
            fadeDirection = 1;
            isFadeCrosshair = true;
        }
    }

    public void ShowCrosshair()
    {
        Crosshair.color = Color.white;
        ReloadCrosshair.color = Color.white;
        Crosshair.enabled = true;
        ReloadCrosshair.enabled = false;
        isFadeCrosshair = false;
    }

    public void ShowHit(float time)
    {
        HitCrosshair.enabled = true;
        hitCooldown = time;
    }

    public enum InSightType
    {
        None,
        Enemy,
        Friendly
    }
}