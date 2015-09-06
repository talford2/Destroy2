using UnityEngine;

public abstract class Missile : MonoBehaviour
{
	protected GameObject Owner;
	protected float Damage;
	protected float Power;
	protected float Radius;
	protected float DeactivateDistance = 10000f;

	[Header("Missile")]
	public GameObject HitEffect;
	public bool CanLock;

	protected bool isLive;

	protected Transform ShootPoint;
	//protected Killable LockOnTarget;

	public virtual void Initialize(GameObject owner, float damage, float power, float radius)
	{
		Owner = owner;
		Damage = damage;
		Power = power;
		Radius = 0f;
	}

	public virtual void SetShootPoint(Transform shootPoint)
	{
		ShootPoint = shootPoint;
	}

	public virtual void Shoot(Vector3 fromPosition, Vector3 direction)
	{
		isLive = true;
	}

	public virtual void Shoot(Vector3 fromPosition, Vector3 direction, Vector3 initVelocity)
	{
		isLive = true;
	}

	public virtual void Stop()
	{
		isLive = false;
	}

	public virtual void Hit(RaycastHit hit)
	{
		if (HitEffect != null)
		{
			var hitEffect = (GameObject)Instantiate(HitEffect, hit.point, Quaternion.LookRotation(hit.normal));
			hitEffect.transform.parent = hit.collider.transform;
		}

		var hitterEffect = hit.collider.GetComponentInParent<HitEffect>();
		if (hitterEffect != null)
		{
			var hitterEffectInst = (GameObject)Instantiate(hitterEffect.Effect, hit.point, Quaternion.LookRotation(hit.normal));
		}
		Stop();
	}

	public abstract void LiveUpate();

	public abstract void AlwaysUpdate();

	private void Update()
	{
		if (isLive)
			LiveUpate();
		AlwaysUpdate();
	}

	public GameObject GetOwner()
	{
		return Owner;
	}

	public float GetDamage()
	{
		return Damage;
	}

	public float GetPower()
	{
		return Power;
	}

	public virtual void HandleCollision(RaycastHit hit, Vector3 shootDirection)
	{
		if (hit.collider != null)
		{
			if (hit.collider.gameObject != null)
			{
				Hit(hit);
			}
			var hitKillable = hit.collider.GetComponentInParent<Killable>();
			if (hitKillable != null)
			{
				Debug.Log("HIT COLLIDER: " + hit.collider.name);
				hitKillable.Damage(hit.collider, hit.point, shootDirection, this);
			}
		    var hitCorpse = hit.collider.GetComponentInParent<Corpse>();
		    if (hitCorpse != null)
		    {
                hit.collider.attachedRigidbody.AddForceAtPosition(shootDirection.normalized*Power, hit.point, ForceMode.Force);
		    }
		    var hitEquipWeapon = hit.collider.GetComponentInParent<EquipWeapon>();
		    if (hitEquipWeapon!=null)
		    {
                hit.collider.attachedRigidbody.AddForceAtPosition(shootDirection.normalized * Power, hit.point, ForceMode.Force);
		    }
		}
	}
}
