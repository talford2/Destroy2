using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class VehicleGun : MonoBehaviour
{
    [Header("Gun")]
    public int GunId;
    public string GunName;
    public float RoundsPerMinute;
    public float TriggerTime; // Effectively the fastest fire rate when clicking fire key repeatedly.
    public int MissilesPerShot;
    public bool IsSimultaneous;
    public Vector2 Spread;
    public float WarmUpTime;
    public bool HasInfiniteAmmunition;
    public int ClipCapacity;
    public float ReloadTime;
    public GameObject EquipPrefab;
    public float MaxAimDistance = 1000f;
    public float Zoom = 1f;

    [Header("Target Locking")]
    public bool RequireTargetLock;
    public float TargetLockTime;
    public float MaxLockRange;
    public bool IsTargeting { get; set; }
    public AudioClip LockBeep;

    [Header("Effects")]
    public GameObject Muzzle;
    public AudioSource FireSound;

    // Overheating
    [Header("Overheating")]
    public bool IsOverHeatable;
    public float OverHeatValue;
    public float HeatPerShot;
    public float OverHeathCooldownDelay;
    public float CooldownDelay;
    public float CooldownRate;

    [Header("GUI")]
    public Sprite Crosshair;

    [Header("Missiles")]
    public GameObject MissileGameObject;
    public float MissileDamage;
    public float MissilePower;
    public float MissileRadius;
    //public GunAnimationType AnimationType;
    //public AmmunitionType AmmunitionType;
    public int MissileCount;

    // Privates
    private Vector3 shootDirection;
    private bool isTriggered;
    private float fireRate;
    private float fireCooldown;
    private float warmUpCooldown;
    private int clipRemaining;
    private bool isReloading;
    private float reloadCooldown;
    private int reloadClipRounds;

    //private Transform curShootPoint;
    private List<Missile> missiles;
    private int curMissileIndex;

    private GameObject gunOwner;
    private bool isReleased;

    // Overheating
    private float heat;
    private float heatCooldown;
    private bool isOverheated;

    // Shoot Points
    private Transform[] shootPointTransforms;
    private GunMuzzle[] muzzleInstances;

    // Targeting
    //private Killable lockedTarget;
    //private Killable targetingTarget;

    // Delegates
    public delegate void OnShootEvent();
    public event OnShootEvent OnShoot;

    public delegate void OnShootFinishedEvent();
    public event OnShootFinishedEvent OnShootFinished;

    public delegate void OnStartReloadEvent();
    public event OnStartReloadEvent OnStartReload;

    public delegate void OnFinishReloadEvent();
    public event OnFinishReloadEvent OnFinishReload;

    public delegate void OnClipEmptyEvent();
    public event OnClipEmptyEvent OnClipEmpty;

    public void InitGun(Transform[] shootPoints, GameObject owner)
    {
        shootPointTransforms = shootPoints;
        if (Muzzle != null)
        {
            muzzleInstances = new GunMuzzle[shootPoints.Length];
            for (var i = 0; i < shootPoints.Length; i++)
            {
                var muzzleObj = (GameObject)Instantiate(Muzzle, shootPoints[i].position, shootPoints[i].rotation);
                muzzleInstances[i] = muzzleObj.GetComponent<GunMuzzle>();
                muzzleInstances[i].transform.parent = shootPoints[i];
            }
        }
        gunOwner = owner;

        missiles = new List<Missile>();
        AddMissileInstance();

        curMissileIndex = 0;
        fireRate = 60f / RoundsPerMinute;
        warmUpCooldown = WarmUpTime;
        clipRemaining = 0;
        heat = 0f;
        isReleased = true;
    }

    private void AddMissileInstance()
    {
        var missileGameObject = Instantiate(MissileGameObject);
        missileGameObject.transform.parent = SceneManager.MissileContainer;
        var missile = missileGameObject.GetComponent<Missile>();
        missile.Initialize(gunOwner, MissileDamage, MissilePower, MissileRadius);
        missiles.Add(missile);
    }

    private void Update()
    {
        if (isTriggered)
        {
            warmUpCooldown -= Time.deltaTime;
        }
        else
        {
            warmUpCooldown += Time.deltaTime;
        }

        warmUpCooldown = Mathf.Clamp(warmUpCooldown, -float.Epsilon, WarmUpTime);

        if (isTriggered && warmUpCooldown <= 0 && ((IsOverHeatable && !isOverheated) || !IsOverHeatable))
        {
            // We're ready to fire
            if (fireCooldown <= 0)
            {
                if (clipRemaining > 0)
                {
                    if (OnShootFinished != null)
                        OnShootFinished();

                    if (IsSimultaneous)
                    {
                        ShootAll();
                    }
                    else
                    {
                        ShootAlternate();
                    }
                    isReleased = false;
                    //lockedTarget = null;
                    lockTargetCooldown = TargetLockTime;
                    fireCooldown = fireRate;
                }
                else
                {
                    if (OnClipEmpty != null)
                        OnClipEmpty();
                }
            }
        }

        if (IsOverHeatable)
        {
            if (heat > 0)
            {
                if (heatCooldown >= 0)
                    heatCooldown -= Time.deltaTime;

                if (heatCooldown < 0)
                    heat -= CooldownRate * Time.deltaTime;

                heat = Mathf.Clamp(heat, 0f, OverHeatValue);
                isOverheated = heat >= OverHeatValue;
            }
        }

        if (!isReloading)
        {
            if (fireCooldown >= 0)
                fireCooldown -= Time.deltaTime;
        }
        else
        {
            if (reloadCooldown >= 0f)
                reloadCooldown -= Time.deltaTime;
            if (reloadCooldown < 0)
                Reload();
        }
    }

    private Vector3 shootAtPosition;
    private float shotConvergence;

    private float lockTargetCooldown;
    /*
    public void SearchAndLock(Team targetTeam, Vector3 facingDirection, float lockRadius)
    {
        var targetCandidate = Targeting.TeamFindFacingAngle(targetTeam, GetShootPointsCentre(), facingDirection, MaxLockRange);
        if (targetCandidate != null && lockedTarget == null)
        {
            var targetScreenPosition = Camera.main.WorldToScreenPoint(targetCandidate.Position());
            var screenCentre = new Vector2(Screen.width / 2f, Screen.height / 2f);
            var fromAim = new Vector2(targetScreenPosition.x, targetScreenPosition.y) - screenCentre;
            if (fromAim.sqrMagnitude < lockRadius * lockRadius)
            {
                if (targetingTarget != targetCandidate)
                {
                    if (targetingTarget != null)
                        targetingTarget.IsTargeted = false;
                    targetCandidate.IsTargeted = true;
                    lockTargetCooldown = TargetLockTime;
                    targetingTarget = targetCandidate;
                }
                IsTargeting = true;
                if (lockTargetCooldown > 0)
                {
                    lockTargetCooldown -= Time.deltaTime;
                    if (lockTargetCooldown < 0)
                    {
                        lockedTarget = targetCandidate;
                        targetCandidate.IsLockedOn = true;
                        //HeadsUpDisplay.Current.PlaySound(LockBeep);
                        Debug.Log("TARGET LOCKED!");
                    }
                }
            }
            else
            {
                if (targetingTarget != null)
                {
                    targetingTarget.IsTargeted = false;
                    targetingTarget = null;
                    lockTargetCooldown = TargetLockTime;
                }
                //IsTargeting = false;
                //targetCandidate.IsTargeted = false;
                //targetCandidate.IsLockedOn = false;
                //lockedTarget = null;
                //lockTargetCooldown = TargetLockTime;
            }
        }
        else
        {
            if (lockedTarget == null)
            {
                Debug.Log("COULD NOT FIND TARGET!");
                IsTargeting = false;
            }
        }
    }
    
    public bool IsLockedOnTarget()
    {
        return lockedTarget != null;
    }

    public void ClearTargeting()
    {
        IsTargeting = false;
        lockTargetCooldown = TargetLockTime;
        if (targetingTarget != null)
            targetingTarget.IsTargeted = false;
        targetingTarget = null;
    }
    */
    public void TriggerShoot(Vector3 aimAt, float convergence)
    {
        isTriggered = true;
        shootAtPosition = aimAt;
        shotConvergence = convergence;
        IsTargeting = false;
        //shootDirection = aimAt - GetShootPointsCentre();
    }

    public void ReleaseTriggerShoot()
    {
        if (!isReleased)
            fireCooldown = TriggerTime;
        isTriggered = false;
        isReleased = true;
    }

    public void TriggerReload(int roundCount)
    {
        if (!isReloading)
        {
            isReloading = true;
            reloadCooldown = ReloadTime;
            reloadClipRounds = roundCount;
            if (OnStartReload != null)
                OnStartReload();
        }
    }

    private Vector3 GetSpreadDirection(Vector3 direction)
    {
        var yawSpread = Random.Range(-Spread.x / 2f, Spread.x / 2f);
        var pitchSpread = Random.Range(-Spread.y / 2f, Spread.y / 2f);
        var spreadAim = Quaternion.Euler(pitchSpread, yawSpread, 0);
        return spreadAim * direction;
    }

    public Vector3 GetShootPointsCentre()
    {
        return shootPointTransforms.Aggregate(Vector3.zero, (current, shootPoint) => current + shootPoint.position) / shootPointTransforms.Length;
    }

    private VelocityReference velocity;

    public void SetVelocityReference(VelocityReference velocityReference)
    {
        velocity = velocityReference;
    }
    
    public VelocityReference GetVelocityReference()
    {
        return velocity;
    }
    
    public void ShootAll()
    {
        var shootPointsCentre = GetShootPointsCentre();
        for (var j = 0; j < shootPointTransforms.Length; j++)
        {
            for (var i = 0; i < MissilesPerShot; i++)
            {
                curMissileIndex++;
                if (curMissileIndex >= MissileCount)
                    curMissileIndex = 0;

                if (curMissileIndex > missiles.Count - 1)
                    AddMissileInstance();

                var curMissile = missiles[curMissileIndex];
                curMissile.SetShootPoint(shootPointTransforms[j]);

                shootAtPosition = GetShootAtPosition(shootPointTransforms[j]);

                var convergedShotDirection = shootAtPosition - shootPointTransforms[j].position;
                var unconvergedShotDirection = shootAtPosition - shootPointsCentre;
                shootDirection = GetSpreadDirection(convergedShotDirection * shotConvergence + (1f - shotConvergence) * unconvergedShotDirection);

                /*
                if (curMissile.CanLock)
                    curMissile.SetLockOnTarget(lockedTarget);
                */
                curMissile.Shoot(shootPointTransforms[j].position, shootDirection.normalized, velocity.Value);

                if (FireSound != null) // && !FireSound.isPlaying)
                    FireSound.Play();

                if (muzzleInstances != null)
                    muzzleInstances[j].Flash();
            }
        }
        if (IsOverHeatable && !isOverheated)
        {
            heat += HeatPerShot;
            heatCooldown = heat > OverHeatValue
                ? OverHeathCooldownDelay
                : CooldownDelay;
        }
        clipRemaining--;

        fireCooldown = fireRate;
        if (OnShoot != null)
            OnShoot();
    }

    private int curShootPointIndex;

    private Vector3 GetShootAtPosition(Transform shootPoint)
    {
        Vector3 shootAt;
        var shootRay = new Ray(shootPoint.position, shootPoint.forward);
        RaycastHit shootHit;
        if (Physics.Raycast(shootRay, out shootHit, MaxAimDistance, ~LayerMask.GetMask("Player", "Sensors")))
        {
            shootAt = shootHit.point;
        }
        else
        {
            shootAt = shootRay.GetPoint(MaxAimDistance);
        }
        return shootAt;
    }

    public void ShootAlternate()
    {
        var shootPointsCentre = GetShootPointsCentre();
        curShootPointIndex++;
        if (curShootPointIndex >= shootPointTransforms.Length)
            curShootPointIndex = 0;

        var curShootPoint = shootPointTransforms[curShootPointIndex];

        for (var i = 0; i < MissilesPerShot; i++)
        {
            curMissileIndex++;
            if (curMissileIndex >= MissileCount)
                curMissileIndex = 0;

            if (curMissileIndex > missiles.Count - 1)
                AddMissileInstance();

            var curMissile = missiles[curMissileIndex];
            curMissile.SetShootPoint(curShootPoint);

            shootAtPosition = GetShootAtPosition(curShootPoint);

            var convergedShotDirection = shootAtPosition - curShootPoint.position;
            var unconvergedShotDirection = shootAtPosition - shootPointsCentre;
            shootDirection = GetSpreadDirection(convergedShotDirection * shotConvergence + (1f - shotConvergence) * unconvergedShotDirection);

            /*
            if (curMissile.CanLock)
                curMissile.SetLockOnTarget(lockedTarget);
            */
            curMissile.Shoot(curShootPoint.position, shootDirection.normalized, velocity.Value);

            if (FireSound != null) // && !FireSound.isPlaying)
                FireSound.Play();

            if (muzzleInstances != null)
                muzzleInstances[curShootPointIndex].Flash();
        }
        if (IsOverHeatable && !isOverheated)
        {
            heat += HeatPerShot;
            heatCooldown = heat > OverHeatValue
                ? OverHeathCooldownDelay
                : CooldownDelay;
        }
        clipRemaining--;

        fireCooldown = fireRate;
        if (OnShoot != null)
            OnShoot();
    }

    private void Reload()
    {
        isReloading = false;
        SetClipRemaining(GetClipRemaining() + reloadClipRounds);
        fireCooldown = 0;
        warmUpCooldown = WarmUpTime;
        if (OnFinishReload != null)
            OnFinishReload();
    }

    public int GetClipRemaining()
    {
        return clipRemaining;
    }

    public void SetClipRemaining(int roundCount)
    {
        clipRemaining = Mathf.Clamp(roundCount, 0, ClipCapacity);
    }

    public bool GetIsReloading()
    {
        return isReloading;
    }

    public float GetHeat()
    {
        return heat;
    }

    public bool IsOverheated()
    {
        return isOverheated;
    }

    public bool IsShootReady()
    {
        return fireCooldown <= 0;
    }
}