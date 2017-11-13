using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class HeadsUpDisplay : MonoBehaviour
{
    public Image Crosshair;
    public Image ReloadCrosshair;
    public Image HitCrosshair;
    public Image Damage;
    public CanvasGroup AmmunitionContainer;
    public Text AmmunitionText;
    public CanvasGroup KillCountContainer;
    public Text KillCountText;

    public Color EnemyCrosshair;
    public Color FriendlyCrosshair;

    private InSightType inSightType;
    private bool isFriendlyInSight;
    private bool isFadeCrosshair;

    private Cooldown _crosshairFadeCooldown;
    private int fadeDirection;

    private Cooldown _ammunitionFadeCooldown;
    private int _ammunitionFadeDirection;

    private Cooldown _killCountFadeCooldown;
    private int _killCountFadeDirection;

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

        _crosshairFadeCooldown = new Cooldown { OnUpdate = CrosshairFadeUpdate, OnFinish = CrosshairFadeFinish };
        _ammunitionFadeCooldown = new Cooldown { OnUpdate = AmmunitionFadeUpdate };
        _killCountFadeCooldown = new Cooldown { OnUpdate = KillCountFadeUpdate };

        insightCrosshairColours = new Dictionary<InSightType, Color>
        {
            { InSightType.None, Color.white },
            { InSightType.Friendly, FriendlyCrosshair },
            { InSightType.Enemy, EnemyCrosshair }
        };

        AmmunitionContainer.alpha = 0f;
        KillCountContainer.alpha = 0f;

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

    private void CrosshairFadeUpdate(float remaining, float duration)
    {
        var fraction = fadeDirection < 0
            ? Mathf.Clamp01(remaining / duration)
            : 1f - Mathf.Clamp01(remaining / duration);
        Crosshair.color = Utility.ColorAlpha(Crosshair.color, fraction);
        ReloadCrosshair.color = Utility.ColorAlpha(Crosshair.color, fraction);
    }

    private void CrosshairFadeFinish()
    {
        isFadeCrosshair = false;
    }

    private void AmmunitionFadeUpdate(float remaining, float duration)
    {
        var fraction = _ammunitionFadeDirection < 0
            ? Mathf.Clamp01(remaining / duration)
            : 1f - Mathf.Clamp01(remaining / duration);
        Debug.LogFormat("FRACTION: {0:f2}", fraction);
        AmmunitionContainer.alpha = fraction;
    }

    private void KillCountFadeUpdate(float remaining, float duration)
    {
        var fraction = _killCountFadeDirection < 0
            ? Mathf.Clamp01(remaining / duration)
            : 1f - Mathf.Clamp01(remaining / duration);
        KillCountContainer.alpha = fraction;
    }

    private void Update()
    {
        _crosshairFadeCooldown.Update(Time.deltaTime);
        _ammunitionFadeCooldown.Update(Time.deltaTime);
        _killCountFadeCooldown.Update(Time.deltaTime);

        Crosshair.color = Utility.ColorAlpha(insightCrosshairColours[inSightType], Crosshair.color.a);

        var vehicle = PlayerController.Current.GetVehicle();
        if (vehicle != null)
        {
            KillCountText.text = string.Format("{0}", Player.Current.KillCount);
            AmmunitionText.text = string.Format("{0} / {1}", vehicle.GetPrimaryWeapon().GetClipRemaining(), vehicle.GetPrimaryWeapon().ClipCapacity);
        }

        if (hitCooldown >= 0f)
        {
            hitCooldown -= Time.deltaTime;
            var hitAlpha = hitCooldown / hitTime;
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
            fadeDirection = -1;
            isFadeCrosshair = true;
            _crosshairFadeCooldown.Trigger(time);
        }
    }

    public void FadeInCrosshair(float time)
    {
        if (!isFadeCrosshair && fadeDirection != 1)
        {
            fadeDirection = 1;
            isFadeCrosshair = true;
            _crosshairFadeCooldown.Trigger(time);
        }
    }

    public void ShowCrosshair()
    {
        Crosshair.color = Color.white;
        ReloadCrosshair.color = Color.white;
        Crosshair.enabled = true;
        ReloadCrosshair.enabled = false;
    }

    public void HideCrosshair()
    {
        Crosshair.enabled = false;
        ReloadCrosshair.enabled = false;
        HitCrosshair.enabled = false;
    }

    public void FadeInAmmunition()
    {
        _ammunitionFadeDirection = 1;
        _ammunitionFadeCooldown.Trigger(0.5f);
    }

    public void FadeOutAmmunition()
    {
        _ammunitionFadeDirection = -1;
        _ammunitionFadeCooldown.Trigger(0.5f);
    }

    public void FadeInKillCount()
    {
        _killCountFadeDirection = 1;
        _killCountFadeCooldown.Trigger(0.5f);
    }

    public void FadeOutKillCount()
    {
        _killCountFadeDirection = -1;
        _killCountFadeCooldown.Trigger(0.5f);
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