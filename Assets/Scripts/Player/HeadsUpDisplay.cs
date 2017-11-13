using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class HeadsUpDisplay : MonoBehaviour
{
	public Image Crosshair;
    public Image ReloadCrosshair;
    public Image HitCrosshair;
	public Image Damage;
    public Text AmmunitionText;
    public Text KillCountText;

    public Color EnemyCrosshair;
    public Color FriendlyCrosshair;

	private InSightType inSightType;
    private bool isFriendlyInSight;
	private bool isFadeCrosshair;
    private int fadeDirection;
	private float fadeTime;
	private float fadeCooldown;

    private bool isShowHit;
    private float hitTime;
    private float hitCooldown;

    private Dictionary<InSightType, Color> insightCrosshairColours;

    private GUIStyle ammoStyle;

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

        insightCrosshairColours = new Dictionary<InSightType, Color>
        {
            { InSightType.None, Color.white },
            { InSightType.Friendly, FriendlyCrosshair },
            { InSightType.Enemy, EnemyCrosshair }
        };

	    var hudFont = Resources.Load<Font>("Fonts/EU____");
        ammoStyle = new GUIStyle
        {
            alignment = TextAnchor.MiddleRight,
            normal = { textColor = Color.white },
            font = hudFont,
            fontSize = 30
        };
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
        Crosshair.color = Utility.ColorAlpha(insightCrosshairColours[inSightType], Crosshair.color.a);

        if (isFadeCrosshair)
        {
            if (fadeCooldown > 0f)
            {
                fadeCooldown -= Time.deltaTime;
                float fadeFraction;
                if (fadeDirection < 0)
                {
                    fadeFraction = Mathf.Clamp01(fadeCooldown / fadeTime);
                }
                else
                {
                    fadeFraction = 1f - Mathf.Clamp01(fadeCooldown / fadeTime);
                }
                Crosshair.color = Utility.ColorAlpha(Crosshair.color, fadeFraction);
                ReloadCrosshair.color = Utility.ColorAlpha(Crosshair.color, fadeFraction);
                if (fadeCooldown < 0f)
                {
                    isFadeCrosshair = false;
                    //Crosshair.enabled = false;
                    //ReloadCrosshair.enabled = false;
                }
            }
        }

        var vehicle = PlayerController.Current.GetVehicle();
        if (vehicle != null)
        {
            KillCountText.text = string.Format("{0}", Player.Current.KillCount);
            AmmunitionText.text = string.Format("{0} / {1}", vehicle.GetPrimaryWeapon().GetClipRemaining(), vehicle.GetPrimaryWeapon().ClipCapacity);
        }

	    if (hitCooldown >= 0f)
	    {
	        hitCooldown -= Time.deltaTime;
	        var hitAlpha = hitCooldown/hitTime;
	        HitCrosshair.color = new Color(1f, 1f, 1f, hitAlpha);
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
            fadeTime = time;
            fadeCooldown = time;
            fadeDirection = -1;
            isFadeCrosshair = true;
        }
    }

    public void FadeInCrosshair(float time)
    {
        if (!isFadeCrosshair && fadeDirection != 1)
        {
            fadeTime = time;
            fadeCooldown = time;
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
        fadeDirection = 1;
    }

    public void HideCrosshair()
    {
        Crosshair.enabled = false;
        ReloadCrosshair.enabled = false;
        HitCrosshair.enabled = false;
        isFadeCrosshair = false;
        fadeDirection = -1;
    }

    public void ShowHit(float time)
    {
        HitCrosshair.enabled = true;
        hitTime = time;
        hitCooldown = hitTime;
        HitCrosshair.color = new Color(1f, 1f, 1f, 1f);
    }

    public enum InSightType
    {
        None,
        Enemy,
        Friendly
    }
}