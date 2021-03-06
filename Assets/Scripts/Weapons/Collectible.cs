﻿using System;
using UnityEngine;

[RequireComponent(typeof(SphereCollider))]
public class Collectible : MonoBehaviour
{
	public bool Enabled;
	public GameObject GivePrefab;

	public delegate void OnCollectEvent();
	public OnCollectEvent OnCollect;

	private CollectibleTracker tracker;
	private SphereCollider triggerCollider;
	private PlayerController occupyingPlayer;

	private void Awake()
	{
		triggerCollider = GetComponent<SphereCollider>();
		triggerCollider.enabled = Enabled;
		tracker = GetComponentInChildren<CollectibleTracker>();
		SceneManager.CollectibleCount++;
	}

	private void Update()
	{
		triggerCollider.enabled = Enabled;
		if (occupyingPlayer != null)
		{
			if (occupyingPlayer.IsCollecting())
			{
				Enabled = false;
				if (tracker != null)
				{
					tracker.Hide();
				}
				if (OnCollect != null)
				{
					occupyingPlayer.Give(GivePrefab);
					OnCollect();
				}
			}
			//Debug.Log("PLAYER CAN COLLECT!");
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

	private void OnDestroy()
	{
		SceneManager.CollectibleCount--;
	}
}
