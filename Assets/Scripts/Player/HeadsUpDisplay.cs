using UnityEngine;
using UnityEngine.UI;

public class HeadsUpDisplay : MonoBehaviour
{
    public Image Crosshair;

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

    private void Update()
    {
        Crosshair.color = isTargetInSight ? Color.red : Color.white;
        if (isFadeOutCrosshair)
        {
            if (fadeOutCooldown > 0f)
            {
                fadeOutCooldown -= Time.deltaTime;
                Crosshair.color = new Color(Crosshair.color.r, Crosshair.color.g, Crosshair.color.b, fadeOutCooldown/fadeOutTime);
                if (fadeOutCooldown < 0f)
                {
                    Crosshair.enabled = false;
                }
            }
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