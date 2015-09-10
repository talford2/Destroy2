using System;
using UnityEngine;

[RequireComponent(typeof(SphereCollider))]
public class Collectible : MonoBehaviour
{
    public bool Enabled;
    public GameObject GivePrefab;

    public delegate void OnCollectEvent();

    public OnCollectEvent OnCollect;

    private SphereCollider triggerCollider;
    private PlayerController occupyingPlayer;
    
    // HUD Highlighting
    private string label;
    private Vector3 toPlayer;
    private GUIStyle highlightStyle;

    private float fadeDistanceSquared;
    private float fadeTime;
    private float fadeCooldown;
    private float fadeTarget;
    private float oldFadeTarget;
    private float fadeFraction;

	private void Awake ()
	{
	    triggerCollider = GetComponent<SphereCollider>();
	    triggerCollider.enabled = Enabled;

	    var weapon = GivePrefab.GetComponent<VehicleGun>();
	    if (weapon != null)
	        label = weapon.GunName;

	    highlightStyle = new GUIStyle {alignment = TextAnchor.MiddleCenter, normal = {textColor = Color.white}};

	    fadeDistanceSquared = 10f*10f;
	    fadeTime = 1f;
	}

    private void Start()
    {
        toPlayer = PlayerController.Current.GetVehicle().transform.position - transform.position;
        var fadeColour = new Color(1f, 1f, 1f, toPlayer.sqrMagnitude < fadeDistanceSquared ? 1f : 0f);
        highlightStyle.normal.textColor = fadeColour;
    }

    private void Update()
    {
        triggerCollider.enabled = Enabled;
        if (occupyingPlayer != null)
        {
            if (occupyingPlayer.IsCollecting())
            {
                if (OnCollect != null)
                {
                    occupyingPlayer.Give(GivePrefab);
                    OnCollect();
                }
            }
            Debug.Log("PLAYER CAN COLLECT!");
        }

        if (PlayerController.Current.GetVehicle() != null)
        {
            oldFadeTarget = fadeTarget;
            fadeTarget = toPlayer.sqrMagnitude < fadeDistanceSquared ? 1f : 0f;

            if (Math.Abs(fadeTarget - oldFadeTarget) > 0.1f)
            {
                fadeCooldown = fadeTime;
            }

            if (fadeCooldown > 0f)
            {
                fadeCooldown -= Time.deltaTime;
                if (fadeCooldown < 0f)
                {
                    var fadeColour = new Color(1f, 1f, 1f, fadeTarget);
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
            toPlayer = PlayerController.Current.GetVehicle().transform.position - transform.position;
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (Enabled)
        {
            var player = other.GetComponentInParent<PlayerController>();
            if (player != null)
            {
                occupyingPlayer = player;
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (Enabled)
        {
            var player = other.GetComponentInParent<PlayerController>();
            if (player != null)
            {
                occupyingPlayer = null;
            }
        }
    }

    private void OnGUI()
    {
        if (PlayerController.Current.GetVehicle() != null)
        {
            var playerCamera = PlayerCamera.Current.Cam;
            if (Utility.IsInFrontOfCamera(playerCamera, transform.position))
            if (Enabled)
            {
                var onScreen = playerCamera.WorldToScreenPoint(transform.position);
                //GUI.Label(new Rect(onScreen.x - 20f, Screen.height - onScreen.y - 20f, 40f, 40f), string.Format("{0:f1}", toPlayer.sqrMagnitude), highlightStyle);
                GUI.Label(new Rect(onScreen.x - 20f, Screen.height - onScreen.y - 40f, 40f, 40f), label, highlightStyle);
            }
        }
    }
}
