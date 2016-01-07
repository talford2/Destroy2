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
    public bool AttachHitEffect;
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
	        var missileHitEffect = (GameObject) Instantiate(HitEffect, hit.point, Quaternion.LookRotation(hit.normal));
	        if (AttachHitEffect)
	            missileHitEffect.transform.parent = hit.collider.transform;
	    }

	    var surfaceHitEffect = hit.collider.GetComponentInParent<HitEffect>();
		if (surfaceHitEffect != null)
		{
			Instantiate(surfaceHitEffect.Effect, hit.point, Quaternion.LookRotation(hit.normal));
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

	public virtual void HandleCollision(RaycastHit hit, Vector3 direction)
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
	            //Debug.Log("HIT COLLIDER: " + hit.collider.name);
	            hitKillable.Damage(hit.collider, hit.point, direction, this);
	            if (PlayerController.Current.GetVehicle() != null)
	            {
	                if (GetOwner() == PlayerController.Current.GetVehicle().gameObject)
	                {
	                    PlayerController.Current.OnTargetHit();
	                }
	            }
	        }
	        var hitCorpse = hit.collider.GetComponentInParent<Corpse>();
	        if (hitCorpse != null)
	        {
	            hit.collider.attachedRigidbody.AddForceAtPosition(direction.normalized*Power, hit.point, ForceMode.Force);
	        }
	        var hitEquipWeapon = hit.collider.GetComponentInParent<EquipWeapon>();
	        if (hitEquipWeapon != null)
	        {
	            hit.collider.attachedRigidbody.AddForceAtPosition(direction.normalized*Power, hit.point, ForceMode.Force);
	        }
	        var hitShootable = hit.collider.GetComponentInParent<Shootable>();
	        if (hitShootable != null)
	        {
                hit.collider.attachedRigidbody.AddForceAtPosition(direction.normalized * Power, hit.point, ForceMode.Force);
	        }
	    }
	}
}
