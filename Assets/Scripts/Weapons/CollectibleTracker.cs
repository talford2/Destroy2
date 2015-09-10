using System;
using UnityEngine;

public class CollectibleTracker : MonoBehaviour
{
    public Collectible Collectible;

    public Texture2D TopLeft;
    public Texture2D TopRight;
    public Texture2D BottomLeft;
    public Texture2D BottomRight;

    // HUD Highlighting
    private string label;
    private Vector3 toCamera;
    private float toCameraDistance;
    private GUIStyle highlightStyle;

    private float fadeDistanceSquared;
    private float fadeTime;
    private float fadeCooldown;
    private float fadeTarget;
    private float oldFadeTarget;
    private float fadeFraction;

    private void Awake()
    {
        var weapon = Collectible.GivePrefab.GetComponent<VehicleGun>();
        if (weapon != null)
            label = weapon.GunName;

        highlightStyle = new GUIStyle
        {
            alignment = TextAnchor.MiddleCenter,
            normal = { textColor = Color.white },
            font = Resources.Load<Font>("Fonts/EU____"),
            fontSize = 12
        };

        fadeDistanceSquared = 10f * 10f;
        fadeTime = 1f;
    }

    private void Start()
    {
        toCamera = PlayerCamera.Current.transform.position - transform.position;
        var fadeColour = new Color(1f, 1f, 1f, toCamera.sqrMagnitude < fadeDistanceSquared ? 1f : 0f);
        highlightStyle.normal.textColor = fadeColour;
    }

    private void Update()
    {
        if (PlayerController.Current.GetVehicle() != null)
        {
            oldFadeTarget = fadeTarget;
            fadeTarget = toCamera.sqrMagnitude < fadeDistanceSquared ? 1f : 0f;

            if (Math.Abs(fadeTarget - oldFadeTarget) > 0.1f)
            {
                fadeCooldown = fadeTime;
            }

            if (fadeCooldown > 0f)
            {
                fadeCooldown -= Time.deltaTime;
                if (fadeCooldown < 0f)
                {
                    fadeFraction = fadeTarget;
                    var fadeColour = new Color(1f, 1f, 1f, fadeFraction);
                    highlightStyle.normal.textColor = fadeColour;
                }
                else
                {
                    fadeFraction = fadeTarget > 0f
                        ? 1f - Mathf.Clamp01(fadeCooldown/fadeTime)
                        : Mathf.Clamp01(fadeCooldown/fadeTime);

                    var fadeColour = new Color(1f, 1f, 1f, fadeFraction);
                    highlightStyle.normal.textColor = fadeColour;
                }
            }
            toCamera = PlayerCamera.Current.transform.position - transform.position;
            toCameraDistance = toCamera.magnitude;
            //label = string.Format("{0:f1}", toPlayerDistance);
        }
    }

    private void OnGUI()
    {
        if (PlayerController.Current.GetVehicle() != null)
        {
            var playerCamera = PlayerCamera.Current.Cam;
            if (Utility.IsInFrontOfCamera(playerCamera, transform.position))
            {
                if (Collectible.Enabled)
                {
                    var onScreen = playerCamera.WorldToScreenPoint(transform.position);

                    var length = 4*Mathf.Clamp(10f - toCameraDistance, 0f, 80f);

                    GUI.Label(new Rect(onScreen.x - 20f, Screen.height - onScreen.y - length - 40f, 40f, 40f), label, highlightStyle);

                    GUI.color = new Color(1f, 1f, 1f, fadeFraction);
                    GUI.DrawTexture(new Rect(onScreen.x - 8f - length, Screen.height - onScreen.y - 8f - length, 16f, 16f), TopLeft);
                    GUI.DrawTexture(new Rect(onScreen.x - 8f + length, Screen.height - onScreen.y - 8f - length, 16f, 16f), TopRight);

                    GUI.DrawTexture(new Rect(onScreen.x - 8f - length, Screen.height - onScreen.y - 8f + length, 16f, 16f), BottomLeft);
                    GUI.DrawTexture(new Rect(onScreen.x - 8f + length, Screen.height - onScreen.y - 8f + length, 16f, 16f), BottomRight);
                }
            }
        }
    }
}
