using Mirror;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using Random = UnityEngine.Random;

[DisallowMultipleComponent]
public class Projectile : MonoBehaviour, IFireable
{
    bool isDestroying;

    #region Tooltip
    [Tooltip("Populate with child TrailRenderer component")]
    #endregion Tooltip
    [SerializeField] TrailRenderer trailRenderer;
    [SerializeField] LayerMask layerMask;
    [SerializeField] Transform belongingParent;
    [SerializeField] SpriteRenderer spriteRenderer;

    [HideInInspector] public Coroutine playerBlockCoroutine;

    [HideInInspector] public bool isHittingWall; // Flag is for wall hit check for penetration arrow

    public ProjectileDetailsSO projectileDetails;

    NetworkIdentity ownerNetworkIdentity;

    Material projectileMaterial;
    Player player;
    Player targetPlayer;
    Enemy belongingEnemy;
    float projectileRange = 0f;
    float projectileSpeed;
    Vector3 fireDirectionVector;
    Vector3 stoppedPosition;
    bool isStopped;
    float fireDirectionAngle;
    float projectileChargeTimer;
    bool isProjectileMaterialSet;
    bool overrideProjectileMovement;
    bool isColliding;
    Vector3 velocity;
    PolygonCollider2D polygonCollider2D;
    ProjectileAnimSync animSync;

    ContactFilter2D filter = new ContactFilter2D();

    //DropOnAxeThrow dropOnAxeThrow;

    readonly Dictionary<uint, float> damageTimers = new Dictionary<uint, float>();

    // NYVERAN ARROWS
    bool isPenetrationArrow;
    bool isTripleThreat;
    bool isBindingArrow;
    bool isArrowOfTheSeven;

    // MYCARA SPELLS
    bool isIceBreaker;

    // KYNARA SPELLS
    bool isFireBlast;
    bool isBlazingCyclone;

    // KARNAG SPELLS
    bool isThrowingAxe;

    // NYXA SPELLS
    bool isShiruken;

    // NYMARA SPELLS
    bool isChainLightning;
    [HideInInspector] public float chainJumpRadius = 6f;
    [HideInInspector] public float chainDelay = 0.05f;       // small delay feels nice; set 0 for instant
    [HideInInspector] public float chainDamageFalloff = 0.8f; // 20% less each jump (tune as you like)
    [HideInInspector] public ChainLightningPhase chainLightningPhase;
    // Chain state
    [HideInInspector] public HashSet<Enemy> chainHitSet; // carries across jumps

    float countDown = 3f;
    [SerializeField] float blastRadius = 5f;
    Coroutine explosionRoutine;
    int damageDone = 0;
    bool lightningStroke;

    // Timer type
    public UnityEvent damageWindowInActiveEvent;
    public UnityEvent damageWindowActiveEvent;
    bool eventIsSubscribed;

    // Guided features
    bool isGuided;

    // Laser features
    [SerializeField] float angleSpeed = 10f;  // How fast the laser extends

    // Laser
    bool isLaserBeam;
    float laserDuration;
    float laserDurationOffset = 0.4f;
    Transform laserStartPoint;

    // GRAPPLE HOOK
    [Space(10)]
    [Header("Grapple Settings")]
    [SerializeField] Transform ropeTransform;
    [SerializeField] SpriteRenderer ropeSpriteRenderer;
    [SerializeField] Transform grappleHeadTransform;
    // ---- Grapple after-latch anti-stuck ----
    [SerializeField] LayerMask solidLayers;         // e.g. Environment, CollisionTilemap
    [SerializeField] float pullAssistForce = 35f;   // tangent push when rubbing on walls
    [SerializeField] float minPullSpeed = 7f;       // enforce progress toward anchor
    [SerializeField] float stallTimeout = 0.35f;    // cancel if no progress for this long
    [SerializeField] float stallProgressEpsilon = 0.06f; // minimum shrink per frame to count as progress
    [SerializeField] PhysicsMaterial2D grappleSlideMaterial;
    PhysicsMaterial2D _originalPlayerMaterial;
    float _stallTimer = 0f;
    float _lastDistToAnchor = float.MaxValue;
    // Grapple
    bool isGrappleHook;
    Transform grappleStartPoint;
    bool hasGrappled = false;
    float currentRopeLength = 0f;
    float grapplePullSpeed = 0f;
    bool grappleHasDealtDamage;
    bool isHookedToTheWall;
    bool isGrappleReleased;
    bool missRetractStarted = false;
    LayerMask playerLayer;
    LayerMask poolLayer;

    // Throwing axe
    [HideInInspector] public float maxDamage;

    Vector3 targetPosition;

    // Locked aim field
    [HideInInspector] public Vector3 lockedTargetVector;
    [HideInInspector] public float lockedAngle;
    bool isLocked = false;

    // Lifetime countdown
    float lifeTimeCountdownTimer = 0f;
    Transform target;
    float updateGuidedMissleTimer = 0f;

    // Guided missle features
    Vector2 straightDirection; // Cache this once
    bool directionInitialized = false;

    ProjectileNetwork projectileNetwork;
    ProjectilePatternNetwork projectilePatternNetwork;

    bool isMultiplayer = false;
    bool initializationCompleted = false;
    float additionalBowAccuracyModifier = 0f;
    float additionalAttackRangeModifier = 0f;

    WeaponTitle lastWeaponTitle;
    WeaponDetailsSO currentWeaponDetails;

    AttackContext prjContext;

    // MULTIPLAYER
    int projectileIndex;

    private void Awake()
    {
        playerLayer = LayerMask.NameToLayer("Player");
        poolLayer = LayerMask.NameToLayer("Pool");

        belongingParent = transform.parent;

        polygonCollider2D = GetComponent<PolygonCollider2D>();
        projectileNetwork = GetComponent<ProjectileNetwork>();
        projectilePatternNetwork = GetComponentInParent<ProjectilePatternNetwork>();

        if (polygonCollider2D == null) polygonCollider2D = GetComponentInChildren<PolygonCollider2D>(); // Grapple head
    }

    private void OnEnable()
    {
        if (!eventIsSubscribed)
        {
            damageWindowInActiveEvent.AddListener(OnDamageWindowEnd);
            damageWindowActiveEvent.AddListener(OnDamageWindowStart);
        }
    }

    private void OnDisable()
    {
        damageWindowInActiveEvent.RemoveListener(OnDamageWindowEnd);
        damageWindowActiveEvent.RemoveListener(OnDamageWindowStart);
    }


    /// <summary>
    /// Initialize the projectile being fired - using the projectileDetails, the aimangle, weaponAngle, and weaponAimDirectionVector. If this 
    /// projectile is part of a pattern the projectile movement can be overriden by setting overrideAmmoMovement to true - PROJECTILE
    /// </summary>
    public void InitializeProjectile(float aimAngle, float weaponAimAngle, Vector3 weaponAimDirectionVector, float projectileSpeed, ProjectileKind projectileKind, ProjectileDetailsSO projectileDetails, AttackContext attackContext,
        bool overrideProjectileMovement, bool fallingFromSkies, int projectileCounter, int projectilePerShot, int projectileIndex, uint ownerNetId, uint targetNetId, Enemy ownerEnemyForSp)
    {
        if (!NetworkServer.active && !NetworkClient.active)
        {
            player = GameManager.Instance.GetLocalPlayer();

            belongingEnemy = ownerEnemyForSp;
        }
        else
        {
            isMultiplayer = true;
            this.projectileIndex = projectileIndex;

            NetworkIdentity ownerIdentity = null;
            projectileDetails = WartheonDatabase.Instance.GetProjectile(projectileIndex);

            if (NetworkServer.active) NetworkServer.spawned.TryGetValue(ownerNetId, out ownerIdentity);
            else NetworkClient.spawned.TryGetValue(ownerNetId, out ownerIdentity);

            ownerNetworkIdentity = ownerIdentity;

            if (ownerIdentity != null)
            {
                Player ownerPlayer = ownerIdentity.GetComponent<Player>();
                Enemy ownerEnemy = ownerIdentity.GetComponent<Enemy>();

                animSync = GetComponent<ProjectileAnimSync>();

                if (ownerEnemy != null)
                {
                    belongingEnemy = ownerEnemy;
                    player = HelperUtilities.GetClosestPlayer(transform.position);
                }
                else if (ownerPlayer != null)
                {
                    player = ownerPlayer;
                }
            }
        }

        this.projectileDetails = projectileDetails;
        projectileMaterial = projectileDetails.projectileMaterial;

        // Set unique projectiles
        SetUniqueProjectiles(projectileKind, attackContext);

        prjContext = attackContext;

        if (projectileDetails.isPlayerProjectile)
        {
            additionalBowAccuracyModifier = player.additionalBowAccuracyModifier;
            additionalAttackRangeModifier = player.additionalAttackRangeModifier;
        }
            
        //if (isThrowingAxe) dropOnAxeThrow = GetComponent<DropOnAxeThrow>();

        // Grapple hook
        isGrappleHook = projectileKind == ProjectileKind.Grapple;

        if (isGrappleHook)
        {
            grappleStartPoint = transform;
            isGrappleReleased = false;
        }

        isLaserBeam = projectileDetails.isLaser;

        // Initialize isColliding
        isColliding = false;

        // Set fire direction
        SetFireDirection(aimAngle, weaponAimAngle, weaponAimDirectionVector, projectileKind, projectileDetails, attackContext, projectileCounter, projectilePerShot, ownerNetId);

        // Play sound if it is a unique projectile
        if (attackContext.galvanusPhase == GalvanusPhase.Lightning)
        {
            lightningStroke = true;
        }
        else if (attackContext.cryotharPhase == CryotharPhase.Icicle || attackContext.pyrotharPhase == PyrotharPhase.FirePillar)
        {
            if (player.NetAuth == null) WorldSoundManager.Instance.PlayWorldSound(projectileDetails.projectileFireSoundEffect, transform.position);
            else NetworkSoundManager.Instance.CmdPlaySound(SoundName.CryotharIcicle, transform.position);
        }
        else if (attackContext.moldranPhase == MoldranPhase.Spike)
        {
            //SoundEffectManager.Instance.PlaySoundEffect(projectileDetails.projectileFireSoundEffect);
        }

        // Set initial projectile material depending on whether there is an projectile charge period
        SetProjectileChargeSettings(projectileDetails);

        // Set projectile range
        if (isPenetrationArrow)
        {
            projectileRange = 100;
            spriteRenderer.material = player.playerDetails.penetrateMaterial;
        }
        else
        {
            projectileRange = projectileDetails.isPlayerProjectile ? projectileDetails.projectileRange + additionalAttackRangeModifier : projectileDetails.projectileRange;
        }

        // Set projectile speed
        this.projectileSpeed = projectileSpeed;

        // Override projectile movement
        this.overrideProjectileMovement = overrideProjectileMovement;

        if (isFireBlast) blastRadius = projectileDetails.blastRadius;

        if (isLaserBeam) laserStartPoint = transform;

        if (projectileDetails != null && projectileDetails.hasLifeTime) lifeTimeCountdownTimer = projectileDetails.lifeDuration;

        if (!eventIsSubscribed)
        {
            damageWindowInActiveEvent.AddListener(OnDamageWindowEnd);
            damageWindowActiveEvent.AddListener(OnDamageWindowStart);
        }

        eventIsSubscribed = true;

        // Activate projectile gameObject
        gameObject.SetActive(true);

        velocity = fireDirectionVector.normalized * projectileSpeed;

        PrepareToFire(NetworkServer.active || NetworkClient.active);

        SetTrailSettings(projectileDetails);

        initializationCompleted = true;
    }

    public void DisableDamageWindowEvent() => damageWindowInActiveEvent?.Invoke();
    public void EnableDamageWindowEvent() => damageWindowActiveEvent?.Invoke();

    private void OnDamageWindowEnd()
    {
        polygonCollider2D.enabled = false;
    }

    private void OnDamageWindowStart()
    {
        polygonCollider2D.enabled = true;
    }

    private void Update()
    {
        if (isMultiplayer && !initializationCompleted) return;
        if (projectileDetails == null) return; // Don't update until projectile details are populated

        if (NetworkServer.active || (!NetworkServer.active && !NetworkClient.active))
        {
            // Life time time reduces over time
            lifeTimeCountdownTimer -= Time.deltaTime;

            if (projectileDetails != null && projectileDetails.hasLifeTime && lifeTimeCountdownTimer < 0)
            {
                DestroyProjectile(isMultiplayer);
            }

            if (updateGuidedMissleTimer > 0f) updateGuidedMissleTimer -= Time.deltaTime;

            // Projectile charge effect
            if (projectileChargeTimer > 0f)
            {
                projectileChargeTimer -= Time.deltaTime;
                return;
            }
            else if (!isProjectileMaterialSet)
            {
                SetProjectileMaterial(projectileMaterial);
                isProjectileMaterialSet = true;
            }

            if (isGrappleHook)
            {
                player.playersGrapple = this;

                if (player.isHuntersReachActive && player.springJoint2D.enabled)
                {
                    InputManager.dodgeRollDisabled = true;

                    Vector2 playerPos = player.GetPlayerPosition();
                    Vector2 hookPos = player.springJoint2D.connectedAnchor;

                    float dist = Vector2.Distance(playerPos, hookPos);

                    if (dist <= 2f) // Lowered threshold
                    {
                        ReleaseGrapple(isMultiplayer);
                    }
                }

                if (!isGrappleReleased) UpdateGrappleHook(isMultiplayer);
            }
            else if (isLaserBeam) UpdateLaser(isMultiplayer);
            else
            {
                if (isGuided && target != null) MoveGuidedProjectile(isMultiplayer);
                else MoveStandardProjectile(isMultiplayer);
            }
        }
    }

    /// <summary>
    /// Set projectile fire direction and angle based on the input angle and direction adjusted by the
    /// random spread - PROJECTILE
    private void SetFireDirection(float aimAngle, float weaponAimAngle, Vector3 weaponAimDirectionVector, ProjectileKind projectileKind, ProjectileDetailsSO projectileDetails,
        AttackContext attackContext, int projectileCounter = 0, int projectilePerShot = 0, uint netId = 0)
    {
        if (isTripleThreat)
        {
            // Define the total angle spread (e.g., 30 degrees spread)
            float totalSpreadAngle = 30f;

            // Calculate the angle increment between projectiles
            float angleIncrement = (projectilePerShot > 1) ? totalSpreadAngle / (projectilePerShot - 1) : 0f;

            // Adjust the starting angle to center the spread
            float startAngle = aimAngle - (totalSpreadAngle / 2);

            // Set the fire direction angle based on the projectile index
            fireDirectionAngle = startAngle + (angleIncrement * projectileCounter);
        }
        else if (attackContext.moravellePhase == MoravellePhase.SpreadArrowShot)
        {
            // Define the total angle spread (e.g., 45 degrees spread)
            float totalSpreadAngle = 120f;

            // Calculate the angle increment between projectiles
            float angleIncrement = (projectilePerShot > 1) ? totalSpreadAngle / (projectilePerShot - 1) : 0f;

            // Adjust the starting angle to center the spread
            float startAngle = aimAngle - (totalSpreadAngle / 2);

            // Set the fire direction angle based on the projectile index
            fireDirectionAngle = startAngle + (angleIncrement * projectileCounter);
        }
        else if (attackContext.cryotharPhase == CryotharPhase.IceProjectile || attackContext.moldranPhase == MoldranPhase.Projectile)
        {
            // Define the total angle spread (e.g., 45 degrees spread)
            float totalSpreadAngle = 120f;

            // Calculate the angle increment between projectiles
            float angleIncrement = (projectilePerShot > 1) ? totalSpreadAngle / (projectilePerShot - 1) : 0f;

            // Adjust the starting angle to center the spread
            float startAngle = aimAngle - (totalSpreadAngle / 2);

            // Set the fire direction angle based on the projectile index
            fireDirectionAngle = startAngle + (angleIncrement * projectileCounter);
        }
        else if (attackContext.pyrotharPhase == PyrotharPhase.FireProjectile)
        {
            // Define the total angle spread (e.g., 45 degrees spread)
            float totalSpreadAngle = 90f;

            // Calculate the angle increment between projectiles
            float angleIncrement = (projectilePerShot > 1) ? totalSpreadAngle / (projectilePerShot - 1) : 0f;

            // Adjust the starting angle to center the spread
            float startAngle = aimAngle - (totalSpreadAngle / 2);

            // Set the fire direction angle based on the projectile index
            fireDirectionAngle = startAngle + (angleIncrement * projectileCounter);
        }

        else if (attackContext.venomancerPhase == VenomancerPhase.SludgeThrow)
        {
            // Define the total angle spread (e.g., 60 degrees spread)
            float totalSpreadAngle = 360f;

            // Calculate the total weight for the decreasing intervals
            float weightSum = 0f;
            for (int i = 0; i < projectilePerShot; i++)
            {
                weightSum += (float)Math.Pow(1f, -i); // Exponential decrease
            }

            // Determine the incremental angle for each projectile
            float cumulativeAngle = 0f;
            for (int i = 0; i < projectilePerShot; i++)
            {
                float weight = (float)Math.Pow(1f, -i) / weightSum; // Normalize weight
                float angle = totalSpreadAngle * weight;

                if (i == projectileCounter)
                {
                    fireDirectionAngle = aimAngle - (totalSpreadAngle / 2) + cumulativeAngle + (angle / 2);
                    break;
                }

                cumulativeAngle += angle;
            }
        }
        else if (attackContext.sylvarokPhase == SylvarokPhase.RazorLeaf)
        {
            // Define the total angle spread (e.g., 60 degrees spread)
            float totalSpreadAngle = 145f;

            // Calculate the total weight for the decreasing intervals
            float weightSum = 0f;
            for (int i = 0; i < projectilePerShot; i++)
            {
                weightSum += (float)Math.Pow(1.2f, -i); // Exponential decrease
            }

            // Determine the incremental angle for each projectile
            float cumulativeAngle = 0f;
            for (int i = 0; i < projectilePerShot; i++)
            {
                float weight = (float)Math.Pow(1.2f, -i) / weightSum; // Normalize weight
                float angle = totalSpreadAngle * weight;

                if (i == projectileCounter)
                {
                    fireDirectionAngle = aimAngle - (totalSpreadAngle / 2) + cumulativeAngle + (angle / 2);
                    break;
                }

                cumulativeAngle += angle;
            }
        }
        else
        {
            // Calculate random spread angle between min and max
            float randomSpread = Random.Range(projectileDetails.projectileSpreadMin, projectileDetails.projectileSpreadMax);

            if (projectileDetails.isPlayerProjectile)
            {
                if (projectileDetails.belongingWeaponDetails != null)
                {
                    if (projectileDetails.belongingWeaponDetails.weaponClass == WeaponClass.Bow ||
                        projectileDetails.belongingWeaponDetails.weaponClass == WeaponClass.Crossbow)
                    {
                        float arrowSpreadReduction = randomSpread * (additionalBowAccuracyModifier);
                        randomSpread = randomSpread - arrowSpreadReduction;
                    }
                }
            }

            // Get a random spread toggle of 1 or -1
            int spreadToggle = Random.Range(0, 2) * 2 - 1;

            if (weaponAimDirectionVector.magnitude < Settings.useAimAngleDistance)
            {
                fireDirectionAngle = aimAngle;
            }
            else
            {
                fireDirectionAngle = weaponAimAngle;
            }

            // Adjust projectile fire angle by random spread
            fireDirectionAngle += spreadToggle * randomSpread;
        }

        // Set projectile rotation
        if (attackContext.galvanusPhase == GalvanusPhase.Lightning || attackContext.cryotharPhase == CryotharPhase.Icicle || attackContext.venomancerPhase == VenomancerPhase.StoneRain ||
            attackContext.pyrotharPhase == PyrotharPhase.FirePillar || attackContext.moldranPhase == MoldranPhase.Spike || attackContext.venomancerPhase == VenomancerPhase.ToxicPool)
        {
            transform.eulerAngles = new Vector3(0f, 0f, 0f);
        }
        else if (isIceBreaker)
        {
            transform.eulerAngles = new Vector3(0f, 0f, 0f);
            // Set projectile fire direction
            fireDirectionVector = HelperUtilities.GetDirectionVectorFromAngle(fireDirectionAngle);
        }
        else
        {
            transform.eulerAngles = new Vector3(0f, 0f, fireDirectionAngle);
            // Set projectile fire direction
            fireDirectionVector = HelperUtilities.GetDirectionVectorFromAngle(fireDirectionAngle);
        }
    }

    /// <summary>
    /// Disable the projectile - thus returning it to the object pool
    /// </summary>
    private void DestroyProjectile(bool isMultiplayer)
    {
        if (isMultiplayer)
        {
            if (projectileNetwork != null) projectileNetwork.Server_DestroyProjectile();
            else if (projectilePatternNetwork != null) projectilePatternNetwork.Server_DestroyProjectilePattern();
        }
        else
        {
            if (!isPenetrationArrow && !isBlazingCyclone)
            {
                velocity = Vector2.zero;
                isStopped = true;
                stoppedPosition = transform.position;
            }
            else
            {
                if (isHittingWall)
                {
                    velocity = Vector2.zero;
                    isStopped = true;
                    stoppedPosition = transform.position;
                }
            }

            if (transform.GetComponentInParent<ProjectilePattern>() != null || isGrappleHook || isIceBreaker || isFireBlast) { }
            else
            {
                GetComponent<Animator>().SetTrigger("impact");
                animSync?.UpdateProjectileImpact(impact: true);
            }

            if (!isPenetrationArrow && !isBlazingCyclone)
            {
                StartCoroutine(DestroyProcess(0.2f));
            }
            else
            {
                if (isHittingWall)
                {
                    StartCoroutine(DestroyProcess(0.2f));
                }
            }
        }
    }

    public void Server_DestroyProjectile()
    {
        if (isDestroying) return;

        isDestroying = true;

        if (!isPenetrationArrow && !isBlazingCyclone)
        {
            velocity = Vector2.zero;
            isStopped = true;
            stoppedPosition = transform.position;
        }
        else
        {
            if (isHittingWall)
            {
                velocity = Vector2.zero;
                isStopped = true;
                stoppedPosition = transform.position;
            }
        }

        if (transform.GetComponentInParent<ProjectilePattern>() != null || isGrappleHook || isIceBreaker || isFireBlast) { }
        else
        {
            GetComponent<Animator>().SetTrigger("impact");
            animSync?.UpdateProjectileImpact(impact: true);
        }

        if (!isPenetrationArrow && !isBlazingCyclone)
        {
            StartCoroutine(DestroyProcess(0.2f));
        }
        else
        {
            if (isHittingWall)
            {
                StartCoroutine(DestroyProcess(0.2f));
            }
        }
    }

    private void PrepareToFire(bool isMultiplayer)
    {
        // GUIDED MISSLE CHECK
        isGuided = projectileDetails.isGuided;

        if (isChainLightning)
        {
            if (chainLightningPhase == ChainLightningPhase.Second || chainLightningPhase == ChainLightningPhase.Third) isGuided = true;
        }

        damageDone = Random.Range(projectileDetails.projectilePhyDamageMin, projectileDetails.projectilePhyDamageMax);

        if (projectileDetails.isPlayerProjectile)
        {
            if (isGuided && target == null)
            {
                if (updateGuidedMissleTimer <= 0f)
                {
                    updateGuidedMissleTimer = 1f;

                    Transform t = FindClosestEnemy();
                    if (t != null && Vector3.Distance(transform.position, t.position) < 5f)
                    {
                        target = t;
                    }
                    else
                    {
                        DestroyProjectile(isMultiplayer);
                    }
                }
            }
        }
        else
        {
            if (isGuided)
            {
                if (belongingEnemy != null)
                {
                    if (!NetworkServer.active && !NetworkClient.active) target = GameManager.Instance.GetLocalPlayer().transform;
                    else if(NetworkServer.active)
                    {
                        targetPlayer = HelperUtilities.GetClosestPlayer(belongingEnemy.transform.position);

                        if (targetPlayer != null)
                        {
                            target = targetPlayer.transform;
                        }
                    }
                }
            }
        }

        if (isLaserBeam && !isGrappleHook)
        {
            float laserCooldown = 2f; // Currently only for Sepharoth Boss

            laserDuration = laserCooldown - laserDurationOffset;
        }
    }

    private void MoveStandardProjectile(bool isMultiplayer, VenomancerPhase venomancerPhase = VenomancerPhase.None)
    {
        // Don't move projectile if movement has been overriden - e.g. this projectile is part of an projectile pattern
        if (!overrideProjectileMovement)
        {
            // Move the projectile based on its velocity
            transform.position += velocity * Time.deltaTime;

            // Disable after max range reached
            projectileRange -= velocity.magnitude * Time.deltaTime;

            if (projectileRange < 0f)
            {
                if (!projectileDetails.isTrap && venomancerPhase != VenomancerPhase.ToxicPool)
                {
                    DestroyProjectile(isMultiplayer);
                }
            }
            if (lightningStroke)
            {
                lightningStroke = false;
                StartCoroutine(DestroyProcess(3.5f));
            }
        }
        else
        {
            if (isStopped)
            {
                transform.position = stoppedPosition;
            }
        }
    }

    private void MoveGuidedProjectile(bool isMultiplayer)
    {
        if (!overrideProjectileMovement)
        {
            projectileRange -= velocity.magnitude * Time.deltaTime;

            Vector2 directionToTarget;

            Debug.Assert(player, "Player is null.");

            if (player.isStealthActive)
            {
                if (!directionInitialized)
                {
                    straightDirection = (target.position + new Vector3(0f, 0.5f, 0f) - transform.position).normalized;
                    directionInitialized = true;
                }

                directionToTarget = straightDirection; // Keep going straight
            }
            else
            {
                directionToTarget = (target.position + new Vector3(0f, 0.5f, 0f) - transform.position).normalized;
                directionInitialized = false; // Reset to re-guide next time
            }

            float targetAngle = HelperUtilities.GetAngleFromVector(directionToTarget);

            transform.rotation = Quaternion.Euler(0f, 0f, targetAngle);

            // Update velocity in the new direction
            fireDirectionVector = HelperUtilities.GetDirectionVectorFromAngle(targetAngle);
            velocity = fireDirectionVector.normalized * projectileSpeed;

            transform.position += velocity * Time.deltaTime;

            if (projectileRange < 0f)
            {
                DestroyProjectile(isMultiplayer);
            }
        }
    }

    private void OnTriggerStay2D(Collider2D collision)
    {
        if (!collision.CompareTag(Settings.playerTag)) return;

        Player player = collision.GetComponent<Player>();

        uint id = player.NetAuth.netId;

        if (!damageTimers.ContainsKey(id))
        {
            damageTimers[id] = 0f;
        }

        damageTimers[id] += Time.deltaTime;

        if (damageTimers[id] >= 0.5f)
        {
            damageTimers[id] = 0f;

            DamageContext ctx = new DamageContext
            {
                source = DamageSourceType.Environment,
                dealerPosition = transform.position,
                receiverPosition = player.transform.position
            };

            ReceiveProjectileDamage receiveProjectileDamage = player.GetComponent<ReceiveProjectileDamage>();
            receiveProjectileDamage.TakeProjectileDamage(15, ctx);

            CheckPoisonStatusOnPlayer(player); // TOXIC POOL
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (projectileDetails == null) return;

        uint playerOwnerNetId = player != null ? player.NetAuth != null ? player.NetAuth.netId : 0 : 0;

        bool isMultiplayer = NetworkServer.active || NetworkClient.active;

        if(NetworkServer.active || (!NetworkServer.active && !NetworkClient.active))
        {
            // BOUNCING PROCESS
            if (projectileDetails != null && projectileDetails.isBouncing && GetComponentInParent<ProjectilePattern>() == null)
            {
                if (collision.collider.tag == Settings.collisionTilemap || collision.collider.tag == Settings.environment)
                {
                    // Reflect off wall/prop
                    Vector2 normal = collision.contacts[0].normal;
                    velocity = Vector2.Reflect(velocity, normal);

                    float angle = HelperUtilities.GetAngleFromVector(velocity);
                    transform.rotation = Quaternion.Euler(0f, 0f, angle);
                }
            }

            // If this is a laser, don't disable it on collision
            if (isLaserBeam) return;

            // If already colliding with something return
            if (isColliding) return;

            int inflictedDamage = 0;

            if (collision.collider.tag == Settings.playerTag)
            {
                Player player = collision.collider.GetComponent<Player>();

                if (player.isValorActive)
                {
                    //SoundEffectManager.Instance.PlaySoundEffect(player.playerDetails.activeSkillTwoSoundEffect);
                    player.health.PostHitImmunity(true);
                }
                else
                {
                    if (projectileDetails.isTrap)
                    {
                        if (explosionRoutine == null)
                        {
                            explosionRoutine = StartCoroutine(ExplosionRoutine(true));
                            return;
                        }
                    }

                    int diceRoll = Random.Range(1, 101);
                    bool isProjectileDodged = 100 - player.currentDodgeValue * 100 < diceRoll ? true : false;

                    // Dodge check
                    if (isProjectileDodged || player.playerControl.isPlayerRolling)
                    {
                        if (isProjectileDodged && player.isShiftingStanceActive)
                        {
                            if (!player.shiftingStanceOnProcess)
                            {
                                player.shiftingStanceOnProcess = true;
                                StartCoroutine(ShiftingStanceRoutine());
                            }
                        }

                        player.health.isDodging = true;
                        player.healthEvent.CallDodgeEvent();

                        if (player.NetAuth == null) WorldSoundManager.Instance.PlayWorldSound(GameManager.Instance.GetLocalPlayer().playerDetails.dodgeSoundEffect, transform.position);
                        else NetworkSoundManager.Instance.CmdPlaySound(SoundName.PlayerDodge, transform.position);

                        player.health.PostHitImmunity(true);
                        //player.health.TakeDamage(0, transform.position, player.health.transform.position, false);
                    }
                    else
                    {
                        if (player.activeWeapon.GetCurrentOffHandWeapon() != null && player.activeWeapon.GetCurrentOffHandWeapon().weaponStats.weaponClass == WeaponClass.Shield)
                        {
                            // Get enemy projectile direction
                            Vector2 enemyProjectileDirection = (player.transform.position - transform.position).normalized;

                            // Get weapon pointer direction
                            Vector2 cursorPosition = InputManager.Instance.pointerPosition.action.ReadValue<Vector2>();
                            Vector2 cursorWorldPosition = player.cameraManager.GetGameplayCamera().ScreenToWorldPoint(cursorPosition);

                            Vector2 pointerDirection = (cursorWorldPosition - new Vector2(player.transform.position.x, player.transform.position.y)).normalized;

                            // Calculate the dot product between the shield's forward direction and the projectile direction
                            float dotProduct = Vector2.Dot(pointerDirection, enemyProjectileDirection);

                            float blockingThreshold = player.activeWeapon.GetCurrentOffHandWeapon().weaponStats.blockChance;

                            // Check if the dot product is greater than the threshold, block fails
                            if (dotProduct > blockingThreshold - 1f && player.health.GetCurrentHealth() > 0)
                            {
                                // Deal Damage To Collision Object
                                DealDamage(collision.collider, ref inflictedDamage);

                                if (player.health.GetCurrentHealth() > 0)
                                {
                                    // Status checks
                                    CheckBleedingStatusOnPlayer(player);
                                    CheckStunStatusOnPlayer(player);
                                    CheckSlowStatusOnPlayer(player);
                                    CheckWarmStatusOnPlayer(player);
                                    CheckBurnStatusOnPlayer(player);
                                    CheckPoisonStatusOnPlayer(player);
                                    CheckChillStatusOnPlayer(player);
                                    CheckFrostStatusOnPlayer(player);
                                    CheckStaticStatusOnPlayer(player);
                                    CheckParalyzeStatusOnPlayer(player);
                                    CheckRootStatusOnPlayer(player);
                                    CheckCurseStatusOnPlayer(player);
                                    CheckFearStatusOnPlayer(player);
                                    CheckBlindStatusOnPlayer(player);
                                }
                            }
                            else
                            {
                                // If the player is attacking, guard is down so block is disabled
                                if (player.meleeAttackMainHand.IsAttacking)
                                {
                                    // Deal Damage To Collision Object
                                    DealDamage(collision.collider, ref inflictedDamage);

                                    if (player.health.GetCurrentHealth() > 0)
                                    {
                                        // Status checks
                                        CheckBleedingStatusOnPlayer(player);
                                        CheckStunStatusOnPlayer(player);
                                        CheckSlowStatusOnPlayer(player);
                                        CheckWarmStatusOnPlayer(player);
                                        CheckBurnStatusOnPlayer(player);
                                        CheckPoisonStatusOnPlayer(player);
                                        CheckChillStatusOnPlayer(player);
                                        CheckFrostStatusOnPlayer(player);
                                        CheckStaticStatusOnPlayer(player);
                                        CheckParalyzeStatusOnPlayer(player);
                                        CheckRootStatusOnPlayer(player);
                                        CheckCurseStatusOnPlayer(player);
                                        CheckFearStatusOnPlayer(player);
                                        CheckBlindStatusOnPlayer(player);
                                    }
                                }
                                else
                                {
                                    if (playerBlockCoroutine == null)
                                    {
                                        // The projectile is within the blocking angle
                                        playerBlockCoroutine = StartCoroutine(PlayerBlockAnimRoutine(collision.collider));
                                    }
                                }
                            }
                        }
                        else
                        {
                            // Deal Damage To Collision Object
                            DealDamage(collision.collider, ref inflictedDamage);

                            if (player.health.GetCurrentHealth() > 0)
                            {
                                // Status checks
                                CheckBleedingStatusOnPlayer(player);
                                CheckStunStatusOnPlayer(player);
                                CheckSlowStatusOnPlayer(player);
                                CheckWarmStatusOnPlayer(player);
                                CheckBurnStatusOnPlayer(player);
                                CheckPoisonStatusOnPlayer(player);
                                CheckChillStatusOnPlayer(player);
                                CheckFrostStatusOnPlayer(player);
                                CheckStaticStatusOnPlayer(player);
                                CheckParalyzeStatusOnPlayer(player);
                                CheckRootStatusOnPlayer(player);
                                CheckCurseStatusOnPlayer(player);
                                CheckFearStatusOnPlayer(player);
                                CheckBlindStatusOnPlayer(player);
                            }
                        }
                    }
                }

                if (prjContext.galvanusPhase != GalvanusPhase.Lightning && prjContext.cryotharPhase != CryotharPhase.Icicle && prjContext.venomancerPhase != VenomancerPhase.StoneRain &&
                    prjContext.pyrotharPhase != PyrotharPhase.FirePillar && prjContext.moldranPhase != MoldranPhase.Spike && prjContext.venomancerPhase != VenomancerPhase.ToxicPool)
                {
                    // Show projectile hit effect
                    ProjectileHitEffect();

                    DestroyProjectile(isMultiplayer);
                }
            }
            else if (collision.collider.tag == Settings.playerWeapon) { }
            else if (collision.collider.tag == Settings.enemyTag)
            {
                Enemy enemy = collision.collider.GetComponent<Enemy>();

                if (collision.collider.GetComponent<Enemy>() != null)
                {
                    IHealthAuthority enemyHealthAuthority = HealthAuthorityResolver.GetAuthority(enemy.gameObject);

                    if (enemy.enemyDetails.hasShield)
                    {
                        int diceRoll = Random.Range(0, 100);
                        bool deflectHappened = 100 - enemy.enemyDetails.deflectionValue * 100 < diceRoll ? true : false;

                        if (deflectHappened)
                        {
                            enemy.health.isBlocking = true;
                            enemy.healthEvent.CallDodgeEvent();

                            DamageContext ctx = new DamageContext { owner = DamageOwner.Player, source = DamageSourceType.Projectile, dealerPosition = transform.position, receiverPosition = enemy.health.transform.position };
                            ReceiveProjectileDamage receiveProjectileDamage = enemy.GetComponent<ReceiveProjectileDamage>();
                            receiveProjectileDamage.TakeProjectileDamage(0, ctx);
                        }
                        else
                        {
                            // Deal Damage To Collision Object
                            DealDamage(collision.collider, ref inflictedDamage);

                            if (enemyHealthAuthority.CurrentHealth > 0)
                            {
                                if (isArrowOfTheSeven)
                                {
                                    int rng = Random.Range(1, 8);

                                    switch (rng)
                                    {
                                        case 1: CheckSlowStatusOnEnemy(enemy, true); break;
                                        case 2: CheckBleedingStatusOnEnemy(enemy, true); break;
                                        case 3: CheckPoisonStatusOnEnemy(enemy, true); break;
                                        case 4: CheckBurnStatusOnEnemy(enemy, true); break;
                                        case 5: CheckFrostStatusOnEnemy(enemy, true); break;
                                        case 6: CheckFearStatusOnEnemy(enemy, true); break;
                                        case 7: CheckBlindStatusOnEnemy(enemy, true); break;
                                        default: break;
                                    }
                                }
                                else
                                {
                                    // Status checks - PROJECTILE
                                    CheckBleedingStatusOnEnemy(enemy, false, isThrowingAxe);
                                    CheckStunStatusOnEnemy(enemy);
                                    CheckSlowStatusOnEnemy(enemy);
                                    CheckWarmStatusOnEnemy(enemy);
                                    CheckBurnStatusOnEnemy(enemy);
                                    CheckPoisonStatusOnEnemy(enemy);
                                    CheckChillStatusOnEnemy(enemy);
                                    CheckFrostStatusOnEnemy(enemy);
                                    CheckShatterStatusOnEnemy(enemy, ref inflictedDamage, isIceBreaker);
                                    CheckStaticStatusOnEnemy(enemy, player.isConductiveTouchActive);
                                    CheckParalyzeStatusOnEnemy(enemy);
                                    CheckRootStatusOnEnemy(enemy, playerOwnerNetId, projectileIndex, isBindingArrow);
                                    CheckCurseStatusOnEnemy(enemy);
                                    CheckFearStatusOnEnemy(enemy);
                                    CheckBlindStatusOnEnemy(enemy);
                                }
                            }
                            else
                            {
                                if (enemyHealthAuthority.CurrentHealth <= 0 && NetworkServer.active)
                                {
                                    Player damageDealerPlayer = ownerNetworkIdentity.GetComponent<Player>();

                                    // Check if player levels-after killing the enemy
                                    int levelBeforeKillingEnemy = damageDealerPlayer.currentLevel;

                                    damageDealerPlayer?.NetAuth.Server_ExpGain(ownerNetworkIdentity, levelBeforeKillingEnemy, enemy.enemyNetwork?.netIdentity);
                                }
                            }
                        }
                    }
                    else
                    {
                        // Deal Damage To Collision Object
                        DealDamage(collision.collider, ref inflictedDamage);

                        if (enemyHealthAuthority.CurrentHealth > 0)
                        {
                            if (isArrowOfTheSeven)
                            {
                                int rng = Random.Range(1, 8);

                                switch (rng)
                                {
                                    case 1: CheckSlowStatusOnEnemy(enemy, true); break;
                                    case 2: CheckBleedingStatusOnEnemy(enemy, true); break;
                                    case 3: CheckPoisonStatusOnEnemy(enemy, true); break;
                                    case 4: CheckBurnStatusOnEnemy(enemy, true); break;
                                    case 5: CheckFrostStatusOnEnemy(enemy, true); break;
                                    case 6: CheckFearStatusOnEnemy(enemy, true); break;
                                    case 7: CheckBlindStatusOnEnemy(enemy, true); break;
                                    default: break;
                                }
                            }
                            else
                            {
                                // Status checks - PROJECTILE
                                CheckBleedingStatusOnEnemy(enemy, false, isThrowingAxe);
                                CheckStunStatusOnEnemy(enemy);
                                CheckSlowStatusOnEnemy(enemy);
                                CheckWarmStatusOnEnemy(enemy);
                                CheckBurnStatusOnEnemy(enemy);
                                CheckPoisonStatusOnEnemy(enemy);
                                CheckChillStatusOnEnemy(enemy);
                                CheckFrostStatusOnEnemy(enemy);
                                CheckStaticStatusOnEnemy(enemy, player.isConductiveTouchActive);
                                CheckParalyzeStatusOnEnemy(enemy);
                                CheckShatterStatusOnEnemy(enemy, ref inflictedDamage, isIceBreaker);
                                CheckRootStatusOnEnemy(enemy, playerOwnerNetId, projectileIndex, isBindingArrow);
                                CheckCurseStatusOnEnemy(enemy);
                                CheckFearStatusOnEnemy(enemy);
                                CheckBlindStatusOnEnemy(enemy);
                            }
                        }
                        else
                        {
                            if (enemyHealthAuthority.CurrentHealth <= 0 && NetworkServer.active)
                            {
                                Player damageDealerPlayer = ownerNetworkIdentity.GetComponent<Player>();

                                // Check if player levels-after killing the enemy
                                int levelBeforeKillingEnemy = damageDealerPlayer.currentLevel;

                                damageDealerPlayer?.NetAuth.Server_ExpGain(ownerNetworkIdentity, levelBeforeKillingEnemy, enemy.enemyNetwork?.netIdentity);
                            }
                        }
                    }
                }
                else
                {
                    // Deal Damage To Collision Object
                    DealDamage(collision.collider, ref inflictedDamage);

                    if (enemy.health.GetCurrentHealth() > 0)
                    {
                        if (isArrowOfTheSeven)
                        {
                            int rng = Random.Range(1, 8);

                            switch (rng)
                            {
                                case 1: CheckSlowStatusOnEnemy(enemy, true); break;
                                case 2: CheckBleedingStatusOnEnemy(enemy, true); break;
                                case 3: CheckPoisonStatusOnEnemy(enemy, true); break;
                                case 4: CheckBurnStatusOnEnemy(enemy, true); break;
                                case 5: CheckFrostStatusOnEnemy(enemy, true); break;
                                case 6: CheckFearStatusOnEnemy(enemy, true); break;
                                case 7: CheckBlindStatusOnEnemy(enemy, true); break;
                                default: break;
                            }
                        }
                        else
                        {
                            // Status checks - PROJECTILE
                            CheckBleedingStatusOnEnemy(enemy, false, isThrowingAxe);
                            CheckStunStatusOnEnemy(enemy);
                            CheckSlowStatusOnEnemy(enemy);
                            CheckWarmStatusOnEnemy(enemy);
                            CheckBurnStatusOnEnemy(enemy);
                            CheckPoisonStatusOnEnemy(enemy);
                            CheckChillStatusOnEnemy(enemy);
                            CheckFrostStatusOnEnemy(enemy);
                            CheckStaticStatusOnEnemy(enemy, player.isConductiveTouchActive);
                            CheckParalyzeStatusOnEnemy(enemy);
                            CheckShatterStatusOnEnemy(enemy, ref inflictedDamage, isIceBreaker);
                            CheckRootStatusOnEnemy(enemy, playerOwnerNetId, projectileIndex, isBindingArrow);
                            CheckCurseStatusOnEnemy(enemy);
                            CheckFearStatusOnEnemy(enemy);
                            CheckBlindStatusOnEnemy(enemy);
                        }
                    }
                }

                // Show projectile hit effect
                ProjectileHitEffect();

                // Projectile behaviour on contact (replace with:)
                if (enemy != null)
                {
                    OnHitEnemy(enemy, isMultiplayer);
                }
                else
                {
                    // Non-enemy contact behavior (walls, ground, etc.)
                    DestroyProjectile(isMultiplayer);
                }
            }
            else if (collision.collider.tag == Settings.decoyTag)
            {
                // Deal Damage To Collision Object
                DealDamage(collision.collider, ref inflictedDamage);

                // Show projectile hit effect
                ProjectileHitEffect();

                if (isFireBlast)
                {
                    if (explosionRoutine == null)
                    {
                        velocity = Vector3.zero;
                        explosionRoutine = StartCoroutine(ExplosionRoutine(isMultiplayer));
                    }
                }
                else
                {
                    DestroyProjectile(isMultiplayer);
                }
            }
            else if (collision.collider.tag == Settings.practiceDummy)
            {
                DummyCheck(collision.collider);

                if (isFireBlast)
                {
                    if (explosionRoutine == null)
                    {
                        velocity = Vector3.zero;
                        explosionRoutine = StartCoroutine(ExplosionRoutine(isMultiplayer));
                    }
                }
                else
                {
                    DestroyProjectile(isMultiplayer);
                }
            }
            else if (collision.collider.tag == Settings.playerWeapon)
            {
                return;
            }
            else // HIT WALL CHECK
            {
                isHittingWall = true;

                // Deal Damage To Collision Object
                DealDamage(collision.collider, ref inflictedDamage);

                // Show projectile hit effect
                ProjectileHitEffect();

                if (isFireBlast)
                {
                    if (explosionRoutine == null)
                    {
                        velocity = new Vector3(0f, 0f, 1f);
                        explosionRoutine = StartCoroutine(ExplosionRoutine(isMultiplayer));
                    }
                }
                else if (isShiruken)
                {

                }
                else
                {
                    DestroyProjectile(isMultiplayer);
                }
            }
        }
    }

    private bool HasEnemyNegativeStatusEffect(Enemy enemy)
    {
        if (enemy.moveStatus != MoveStatus.Idle) return true;

        if (enemy.healthStatus != HealthStatus.Normal) return true;

        if (enemy.isCursed || enemy.isFeared || enemy.isRevealed || enemy.isStatic || enemy.isWarmed || enemy.isChilled || enemy.isBlind || enemy.isSlowed) return true;

        return false;
    }

    IEnumerator PlayerBlockAnimRoutine(Collider2D collision)
    {
        Player player = collision.GetComponent<Player>();

        WeaponDetailsSO offhandWeaponDetails = WartheonDatabase.Instance.GetWeaponDetails(player.activeWeapon.GetCurrentOffHandWeapon().weaponStats.weaponTitle);

        // Adjust animator layer weights
        player.transform.GetChild(2).GetComponent<Animator>().SetTrigger(Settings.block);

        if (player.NetAuth == null) WorldSoundManager.Instance.PlayWorldSound(offhandWeaponDetails.weaponSwingSoundEffect, transform.position);
        else NetworkSoundManager.Instance.CmdPlaySound(SoundName.SwordSwing, transform.position);

       player.healthEvent.CallBlockEvent();

        yield return new WaitForSeconds(0.6f);

        playerBlockCoroutine = null;
        player.animatePlayer.ResetAnimatonParameters();
        player.animator.SetBool(Settings.isIdle, true);
    }

    IEnumerator ShiftingStanceRoutine()
    {
        player.CurrentAgilityValue += 3;

        yield return new WaitForSeconds(3f);

        player.shiftingStanceOnProcess = false;
        player.CurrentAgilityValue -= 3;
    }

    private void DealDamage(Collider2D collision, ref int inflictedDamage, bool isLaser = false)
    {
        ReceiveProjectileDamage receiveProjectileDamage = collision.GetComponent<ReceiveProjectileDamage>();

        if (receiveProjectileDamage != null)
        {
            // Set isColliding to prevent ammo dealing damage multiple times
            if (isPenetrationArrow || isBlazingCyclone)
            {
                StartCoroutine(ColliderTimeThreshold());
            }
            else if(!isLaser) // Laser do not need this
            {
                isColliding = true;
            }

            float drainedHealth = 0f;

            if (collision != null && projectileDetails.isPlayerProjectile)
            {
                // Caster is player
                if (isThrowingAxe)
                {
                    damageDone = Random.Range(player.currentMainHandMinDamageValue * 2, player.currentMainHandMaxDamageValue * 2);
                }
                else
                {
                    damageDone = Random.Range(player.currentMainHandMinDamageValue, player.currentMainHandMaxDamageValue);
                }
            }
            else
            {
                damageDone = Random.Range(projectileDetails.projectilePhyDamageMin, projectileDetails.projectilePhyDamageMax);
            }

            if (collision != null && collision.GetComponent<Enemy>() != null)
            {
                Enemy enemy = collision.GetComponent<Enemy>();

                if (projectileDetails.isPlayerProjectile)
                {
                    // LOWER DAMAGE IF PLAYER IS CURSED - PROJECTILE
                    damageDone = player.isCursed ? player.currentMainHandMinDamageValue : Random.Range(player.currentMainHandMinDamageValue,
                        player.currentMainHandMaxDamageValue);

                    float punishersWillModifier = 0.05f;

                    float totalDamageModifiers = 0f;

                    float specialAttackModifier = 0f; // Default value

                    float enemyCurrrentHealth = enemy.health.GetCurrentHealth();
                    float enemyMaximumHealth = enemy.health.GetMaximumHealth();

                    // SPECIAL SKILL ATTACK BOOST CHECK
                    if (player != null)
                    {
                        if (isPenetrationArrow)
                        {
                            specialAttackModifier = player.playerDetails.firstActiveSkillDetails.GetCurrentActiveLevel() switch
                            {
                                1 => 0.15f,
                                2 => 0.25f,
                                3 => 0.35f,
                                _ => 0f
                            };
                        }
                        else if (isTripleThreat)
                        {
                            specialAttackModifier = player.playerDetails.secondActiveSkillDetails.GetCurrentActiveLevel() switch
                            {
                                1 => 0f,
                                2 => 0.1f,
                                3 => 0.25f,
                                _ => 0f
                            };
                        }
                        else if (isShiruken)
                        {
                            specialAttackModifier = player.playerDetails.fifthActiveSkillDetails.GetCurrentActiveLevel() switch
                            {
                                1 => -0.5f,
                                2 => -0.4f,
                                3 => -0.25f,
                                _ => 0f
                            };
                        }
                        else if (isFireBlast)
                        {
                            specialAttackModifier = player.playerDetails.firstActiveSkillDetails.GetCurrentActiveLevel() switch
                            {
                                1 => 0.1f,
                                2 => 0.2f,
                                3 => 0.35f,
                                _ => 0f
                            };
                        }
                        else if (isBlazingCyclone)
                        {
                            specialAttackModifier = player.playerDetails.fifthActiveSkillDetails.GetCurrentActiveLevel() switch
                            {
                                1 => -0.4f,
                                2 => -0.3f,
                                3 => -0.1f,
                                _ => 0f
                            };
                        }
                        else if (isChainLightning)
                        {
                            switch (chainLightningPhase)
                            {
                                case ChainLightningPhase.None:
                                    break;
                                case ChainLightningPhase.First:
                                    specialAttackModifier = player.playerDetails.thirdActiveSkillDetails.GetCurrentActiveLevel() switch
                                    {
                                        1 => 0f,
                                        2 => 0.1f,
                                        3 => 0.2f,
                                        _ => 0f
                                    };
                                    break;
                                case ChainLightningPhase.Second:
                                    specialAttackModifier = player.playerDetails.thirdActiveSkillDetails.GetCurrentActiveLevel() switch
                                    {
                                        1 => -0.25f,
                                        2 => -0.17f,
                                        3 => -0.1f,
                                        _ => 0f
                                    };
                                    break;
                                case ChainLightningPhase.Third:
                                    specialAttackModifier = player.playerDetails.thirdActiveSkillDetails.GetCurrentActiveLevel() switch
                                    {
                                        1 => -0.5f,
                                        2 => -0.45f,
                                        3 => -0.5f,
                                        _ => 0f
                                    };
                                    break;
                                default:
                                    break;
                            }
                        }
                    }

                    // Punisher's Will Check
                    if (player.isPunishersWillActive && enemyCurrrentHealth / enemyMaximumHealth < 0.5f) totalDamageModifiers += punishersWillModifier;

                    damageDone = Mathf.RoundToInt(damageDone * (1 + totalDamageModifiers + specialAttackModifier)); // Add additional damage modifiers

                    // Critical Hit Check
                    bool criticalHitHappened = CriticalHitHappened(enemy);

                    if (criticalHitHappened)
                    {
                        enemy.healthEvent.CallCriticalHitEvent();

                        float critMultiplier = player.currentMainHandCriticalHitDamage;

                        if ((enemy != null && enemy.isBlind)) critMultiplier += player.additionalCriticalDamageOnCloakedPrecision;

                        if (player.playerDetails.playerCharacterIndex == Character.Nyveran && HasEnemyNegativeStatusEffect(enemy)) //Deadeye's Quiver
                        {
                            damageDone = (int)(damageDone * (player.currentMainHandCriticalHitDamage + 0.25f));
                            player.playerSkillController.activeSkillTypeOneAnimator.SetTrigger("deadeye");
                        }
                        else damageDone = (int)(damageDone * critMultiplier);
                    }
                }

                // Segregate elemental and non-elemental damage
                int elementalDamage = 0;
                int nonElementalDamage = 0;
                int inflictedNonElementalDamage = 0;
                int additionalElementalDamage = 0;

                if (isIceBreaker || isFireBlast || isBlazingCyclone || isShiruken || isChainLightning)
                {
                    elementalDamage = (int)(player.activeWeapon.GetCurrentMainHandWeapon().weaponStats.elementalForgeRate * damageDone);
                    additionalElementalDamage = (int)(elementalDamage * (1 + player.additionalMagicDamageModifier));
                    elementalDamage += additionalElementalDamage;
                }
                else if (isThrowingAxe)
                {
                    if(!NetworkServer.active && !NetworkClient.active) elementalDamage = (int)(DropItem.droppedThrowingAxe.weaponStats.elementalForgeRate * damageDone);
                    else elementalDamage = (int)(DropItemNetwork.droppedThrowingAxe.weaponStats.elementalForgeRate * damageDone);

                    additionalElementalDamage = (int)(elementalDamage * (1 + player.additionalMagicDamageModifier));
                    elementalDamage += additionalElementalDamage;
                }
                else
                {
                    elementalDamage = (int)(projectileDetails.belongingWeaponDetails.elementalForgeRate * damageDone);
                    additionalElementalDamage = (int)(elementalDamage * (1 + player.additionalMagicDamageModifier));
                    elementalDamage += additionalElementalDamage;
                }

                nonElementalDamage = damageDone - elementalDamage + additionalElementalDamage;

                // ARMOR DEDUCTIONS
                float effectiveArmor = enemy.currentArmor;

                if (isPenetrationArrow) effectiveArmor *= 0.7f; // 30% Armor Penetration

                if (player.isShatterCryActive)
                {
                    switch (player.playerDetails.secondActiveSkillDetails.GetCurrentActiveLevel())
                    {
                        case 1:
                            effectiveArmor *= 0.8f;
                            break;
                        case 2:
                            effectiveArmor *= 0.7f;
                            break;
                        case 3:
                            effectiveArmor *= 0.6f;
                            break;
                        default:
                            break;
                    }
                }
                    
                inflictedNonElementalDamage = (int)(nonElementalDamage * (1 - effectiveArmor));

                int inflictedElementalDamage = 0;

                // Damage inflicted to enemy after deducting enemy armor
                WeaponDetailsSO weaponDetails;

                if (isIceBreaker || isFireBlast || isBlazingCyclone || isShiruken || isChainLightning)
                {
                    weaponDetails = WartheonDatabase.Instance.GetWeaponDetails(player.activeWeapon.GetCurrentMainHandWeapon().weaponStats.weaponTitle);
                }
                else if (isThrowingAxe)
                {
                    if (!NetworkServer.active && !NetworkClient.active) weaponDetails = WartheonDatabase.Instance.GetWeaponDetails(DropItem.droppedThrowingAxe.weaponStats.weaponTitle);
                    else weaponDetails = WartheonDatabase.Instance.GetWeaponDetails(DropItemNetwork.droppedThrowingAxe.weaponStats.weaponTitle);
                }
                else
                {
                    weaponDetails = projectileDetails.belongingWeaponDetails;
                }

                inflictedElementalDamage = (int)(elementalDamage * (1 - enemy.enemyDetails.magicResistance));

                inflictedDamage = inflictedElementalDamage + inflictedNonElementalDamage;
            }
            else if (collision != null && collision.GetComponent<Player>() != null)
            {
                Player player = collision.GetComponent<Player>();

                inflictedDamage = 0;

                // Segregate elemental and non-elemental damage
                int elementalDamage = (int)(projectileDetails.belongingWeaponDetails.elementalForgeRate * damageDone);
                int nonElementalDamage = damageDone - elementalDamage;

                int inflictedNonElementalDamage = (int)(nonElementalDamage * (1 - player.currentArmorValue));

                int inflictedElementalDamage = 0;
                // Calculate inflicted elemental damage
                inflictedElementalDamage = (int)(elementalDamage * (1 - player.currentMagicResistanceValue));

                // Damage inflicted to player after deducting player armor
                inflictedDamage = Mathf.RoundToInt((inflictedElementalDamage + inflictedNonElementalDamage) * (1 - player.currentDamageReductionValue));
            }
            else if (collision != null && collision.GetComponent<Environment>() != null)
            {
                // Damage inflicted equals damage done for environment objects
                inflictedDamage = damageDone;
            }

            // Life Drain Check
            if (player != null)
            {
                drainedHealth = inflictedDamage * player.additionalLifeStealModifier;

                if (drainedHealth > 0f + Mathf.Epsilon) player.health.AddHealth((int)drainedHealth);
            }

            // **Apply Time.deltaTime Scaling ONLY for Laser Beams**
            if (isLaserBeam)
            {
                inflictedDamage = Mathf.Max(1, inflictedDamage); // Ensure at least 1 damage per frame
            }

            DamageContext ctx = new DamageContext { source = DamageSourceType.Projectile, dealerPosition = transform.position, receiverPosition = receiveProjectileDamage.transform.position };

            if (tag == Settings.playerProjectile) ctx.owner = DamageOwner.Player;
            else if (tag == Settings.enemyProjectile) ctx.owner = DamageOwner.Enemy;

            receiveProjectileDamage.TakeProjectileDamage(inflictedDamage, ctx);
        }
    }

    /// <summary>
    /// Critical hit check
    /// </summary>
    /// <returns></returns>
    private bool CriticalHitHappened(Enemy enemy)
    {
        bool criticalHitHappened = false;

        float randomCriticalDice = Random.Range(0f, 1f);

        if (player.isBlind)
        {
            criticalHitHappened = false;
        }
        else if (enemy != null && player.playerDetails.playerCharacterIndex == Character.Nyveran)
        {
            criticalHitHappened = randomCriticalDice < player.currentMainHandCriticalHitChance + 0.15f; // Nyveran - Deadeye's Quiver Passive
        }
        else
        {
            criticalHitHappened = randomCriticalDice < player.currentMainHandCriticalHitChance - player.currentCriticalResistanceValue;
        }

        return criticalHitHappened;
    }

    /// <summary>
    /// Dummy hit interactions
    /// </summary>
    private void DummyCheck(Collider2D collider)
    {
        Health health = collider.GetComponent<Health>();
        ReceiveProjectileDamage receiveProjectileDamage = collider.GetComponent<ReceiveProjectileDamage>();

        // Damage produced by player
        int damageDone = player.isCursed ? player.currentMainHandMinDamageValue : Random.Range(player.currentMainHandMinDamageValue, player.currentMainHandMaxDamageValue);
        int offHandDamageDone = player.isCursed ? player.currentOffHandMinDamageValue : Random.Range(player.currentOffHandMinDamageValue, player.currentOffHandMinDamageValue);
        damageDone += offHandDamageDone;

        bool criticalHitHappened = false;

        // Calculate damage after critical hit check
        if (player.isStealthActive)
        {
            damageDone = criticalHitHappened ? (int)(damageDone * (player.activeWeapon.GetCurrentMainHandWeapon().weaponStats.criticalHitDamage +
                player.additionalCriticalMeleeDamageModifier + player.additionalCriticalDamageOnCloakedPrecision)) : damageDone;
        }
        else
        {
            damageDone = criticalHitHappened ? (int)(damageDone * player.activeWeapon.GetCurrentMainHandWeapon().weaponStats.criticalHitDamage +
                player.additionalCriticalMeleeDamageModifier) : damageDone;
        }

        health.PostHitImmunity();

        DamageContext ctx = new DamageContext { source = DamageSourceType.Projectile, dealerPosition = transform.position, receiverPosition = health.transform.position };

        if (tag == Settings.playerProjectile) ctx.owner = DamageOwner.Player;
        else if (tag == Settings.enemyProjectile) ctx.owner = DamageOwner.Enemy;

        receiveProjectileDamage.TakeProjectileDamage(damageDone, ctx);
        collider.GetComponent<HealthEvent>().CallHealthChangedEvent(1000000000, damageDone, MeleeHand.None);
    }

    IEnumerator ColliderTimeThreshold()
    {
        yield return new WaitForSeconds(0.04f);

        isColliding = false;
    }

    private void UpdateLaser(bool isMultiplayer)
    {
        laserDuration -= Time.deltaTime;

        if (laserDuration <= 0)
        {
            DestroyProjectile(isMultiplayer);
            return;
        }

        Vector3 startPos = laserStartPoint.position;
        Vector3 direction = new Vector3();
        float startAngle = 0f;

        if (!isLocked)
        {
            direction = lockedTargetVector;
            startAngle = lockedAngle;
            isLocked = true;
        }
        else
        {
            // Update direction for raycast
            direction = HelperUtilities.GetDirectionVectorFromAngle(lockedAngle);
        }

        RaycastHit2D validHit = default;

        RaycastHit2D[] hits = Physics2D.RaycastAll(transform.position, direction, Mathf.Infinity, layerMask);

        foreach (RaycastHit2D hit in hits)
        {
            if (hit.collider == null) continue;

            // Ignore helper colliders
            if (hit.collider is CircleCollider2D) continue;
            if (hit.collider is CapsuleCollider2D) continue;

            // Only accept polygon collider 
            if (hit.collider is PolygonCollider2D)
            {
                validHit = hit;
                break;
            }
        }

        float maxDistance = validHit.collider != null ? Vector2.Distance(startPos, validHit.point + new Vector2(0f, 0.5f)) : Vector2.Distance(startPos, targetPosition);

        if (validHit.collider is CircleCollider2D) return;
        if (validHit.collider is CapsuleCollider2D) return;

        // If the player is inside the beam, apply continuous damage**
        if (validHit.collider != null && validHit.collider.CompareTag(Settings.playerTag))
        {
            player = validHit.collider.GetComponent<Player>();

            if (player != null)
            {
                // Status checks
                CheckPoisonStatusOnPlayer(player);
                CheckFrostStatusOnPlayer(player);
                CheckStunStatusOnPlayer(player);
                CheckCurseStatusOnPlayer(player);
                CheckBlindStatusOnPlayer(player);

                DealLaserDamage(validHit.collider);
            }
        }

        // **Smoothly Extend Laser Length**
        spriteRenderer.transform.localScale = new Vector3(maxDistance * 5, spriteRenderer.transform.localScale.y, 1f);

        // Update laser start points transform
        laserStartPoint = transform;

        // Check Player's location for beam
        bool isBeamAbovePlayer = IsBeamAbovePlayer(ref direction);

        // Change angle towards player position
        if (isBeamAbovePlayer) lockedAngle -= angleSpeed * Time.deltaTime;
        else lockedAngle += angleSpeed * Time.deltaTime;

        // **Rotate Laser Sprite**
        spriteRenderer.transform.rotation = Quaternion.Euler(0f, 0f, lockedAngle);

        // Tweak with sorting order
        Vector3 playerDirectionVector = Vector3.zero;

        if (player != null)
        {
            playerDirectionVector = player.GetPlayerPosition() - belongingEnemy.transform.position;
        }

        float enemyAngleDegrees = HelperUtilities.GetAngleFromVector(playerDirectionVector);
        AimDirection enemyAimDirection = HelperUtilities.GetAimDirection(lockedAngle);

        if (enemyAimDirection == AimDirection.Up)
        {
            spriteRenderer.sortingOrder = -1;
        }
        else
        {
            spriteRenderer.sortingOrder = 3;
        }
    }

    private void UpdateGrappleHook(bool isMultiplayer)
    {
        Vector3 startPos = grappleStartPoint.position;
        Vector2 direction = fireDirectionVector.normalized;
        float angle = HelperUtilities.GetAngleFromVector(direction);
        float maxGrappleRange = 0f;

        if (player != null)
        {
            maxGrappleRange = player.playerDetails.fifthActiveSkillDetails.GetCurrentActiveLevel() switch
            {
                1 => projectileDetails.projectileRange,
                2 => projectileDetails.projectileRange * 1.25f,
                3 => projectileDetails.projectileRange * 1.5f,
                _ => projectileDetails.projectileRange
            };
        }

        RaycastHit2D hit = Physics2D.Raycast(startPos, direction, maxGrappleRange, layerMask);
        float targetRopeLength = hit.collider != null ? Vector2.Distance(startPos, hit.point) : maxGrappleRange;

        if (!hasGrappled)
        {
            // Extend rope toward max range or hit point
            currentRopeLength = Mathf.MoveTowards(currentRopeLength, targetRopeLength, projectileSpeed * Time.deltaTime);

            // Update rope sprite
            ropeSpriteRenderer.size = new Vector2(currentRopeLength, ropeSpriteRenderer.size.y);
            ropeSpriteRenderer.transform.position = startPos;
            ropeSpriteRenderer.transform.rotation = Quaternion.Euler(0f, 0f, angle);

            // Move grapple head to rope tip
            Vector3 hookTip = startPos + (Vector3)(direction * currentRopeLength);
            grappleHeadTransform.position = hookTip;
            grappleHeadTransform.rotation = Quaternion.Euler(0f, 0f, angle);

            // --- Missed-grapple check ---
            if (!missRetractStarted && hit.collider == null && currentRopeLength >= maxGrappleRange - 0.01f)
            {
                missRetractStarted = true;
                StartCoroutine(RetractMissedGrapple(isMultiplayer));
                return;                                    // stop further processing this frame
            }
        }
        // ----------  PHASE 2 : grapple latched  ----------
        else
        {
            Vector3 endPos = grappleHeadTransform.position;   // fixed latch point
            Vector3 ropeVector = endPos - startPos;

            float ropeLength = ropeVector.magnitude;
            float ropeAngle = Mathf.Atan2(ropeVector.y, ropeVector.x) * Mathf.Rad2Deg;

            // Stretch rope between player and fixed head
            ropeSpriteRenderer.size = new Vector2(ropeLength, ropeSpriteRenderer.size.y);
            ropeSpriteRenderer.transform.position = startPos;
            ropeSpriteRenderer.transform.rotation = Quaternion.Euler(0f, 0f, ropeAngle);
        }

        // Handle damage if enemy hit
        if (hit.collider != null)
        {
            if (hit.collider.CompareTag(Settings.enemyTag) && !grappleHasDealtDamage && currentRopeLength >= targetRopeLength - 0.01f)
            {
                grappleHasDealtDamage = true;

                CheckPoisonStatusOnPlayer(player);
                CheckFrostStatusOnPlayer(player);
                CheckStunStatusOnPlayer(player);
                CheckCurseStatusOnPlayer(player);
                CheckBlindStatusOnPlayer(player);

                damageDone = Random.Range(projectileDetails.projectilePhyDamageMin, projectileDetails.projectilePhyDamageMax);
                int inflictedDamage = (int)(damageDone * (1 - hit.transform.GetComponent<Enemy>().currentArmor));

                DamageContext ctx = new DamageContext { owner = DamageOwner.Player, source = DamageSourceType.Projectile, dealerPosition = grappleHeadTransform.position, receiverPosition = hit.transform.position };
                ReceiveProjectileDamage receiveProjectileDamage = hit.transform.GetComponent<ReceiveProjectileDamage>();
                receiveProjectileDamage.TakeProjectileDamage(inflictedDamage, ctx);

                player.health.PostHitImmunity(true); // Temporary inviciblity

                if (hit.transform.TryGetComponent(out Enemy enemy))
                {
                    IEnemyCombatData enemyCombatData = EnemyDataResolver.Resolve<IEnemyCombatData>(enemy.gameObject);

                    if (player.activeWeapon.GetCurrentMainHandWeapon().weaponStats.weaponTitle != lastWeaponTitle)
                    {
                        currentWeaponDetails = WartheonDatabase.Instance.GetWeaponDetails(player.activeWeapon.GetCurrentMainHandWeapon().weaponStats.weaponTitle);
                        lastWeaponTitle = player.activeWeapon.GetCurrentMainHandWeapon().weaponStats.weaponTitle;
                    }

                    player.meleeAttackMainHand.CheckStunStatus(currentWeaponDetails, enemy, enemyCombatData, false, isGrapple: true);
                }

                Physics2D.IgnoreLayerCollision(playerLayer, poolLayer, false); // re-enable
                return;
            }
            else if (hit.collider.CompareTag(Settings.collisionTilemap) && !isHookedToTheWall)
            {
                // Disable pool layer
                Physics2D.IgnoreLayerCollision(playerLayer, poolLayer, true); // disable

                isHookedToTheWall = true;

                if (player.NetAuth == null) WorldSoundManager.Instance.PlayWorldSound(player.playersAllActiveUniqueSkills[4].activeUniqueSkillSoundEffectTwo, transform.position);
                else NetworkSoundManager.Instance.CmdPlaySound(SoundName.HuntersReach2, transform.position);
            }

            // GRAPPLE - PULL PLAYER PHASE
            // Lock joint only when rope fully extended
            if (!hasGrappled && currentRopeLength >= targetRopeLength - 0.01f)
            {
                if(player.HasPlayerStrongNegativeStatusEffectExcludingHealth() && !missRetractStarted)
                {
                    missRetractStarted = true;
                    StartCoroutine(RetractMissedGrapple(isMultiplayer));
                    return;
                }

                hasGrappled = true;

                Vector2 grapplePoint = grappleHeadTransform.position;

                player.springJoint2D.enabled = true;
                player.springJoint2D.connectedAnchor = grapplePoint;
                player.springJoint2D.distance = 0f; // Pulls player to exact point
                player.springJoint2D.enableCollision = false;

                // Control pull strength and smoothness
                player.springJoint2D.frequency = 2f;      // Pulling strength � lower = slower pull
                player.springJoint2D.dampingRatio = 0.1f;   // How much it resists overshooting/bouncing

                player.isHuntersReachActive = true;

                // NEW:
                BeginLatchedState();

                // Assist sliding & enforce progress; auto-cancel if stalled
                if (hasGrappled && player.springJoint2D.enabled)
                {
                    if (LatchedMoveAssist())
                    {
                        // stalled -> retract or release
                        if (!missRetractStarted)
                        {
                            missRetractStarted = true;
                            StartCoroutine(RetractMissedGrapple(isMultiplayer));
                        }
                        return;
                    }
                }
            }
        }
    }

    private void BeginLatchedState()
    {
        // swap to low-friction so we slide along walls
        if (player?.polygonCollider2D != null)
        {
            _originalPlayerMaterial = player.polygonCollider2D.sharedMaterial;
            player.polygonCollider2D.sharedMaterial = grappleSlideMaterial;
        }

        _stallTimer = 0f;
        _lastDistToAnchor = float.MaxValue;
    }

    private void EndLatchedState()
    {
        // restore original friction
        if (player?.polygonCollider2D != null) player.polygonCollider2D.sharedMaterial = _originalPlayerMaterial;

        _stallTimer = 0f;
        _lastDistToAnchor = float.MaxValue;
    }

    // Call only while hasGrappled && springJoint2D.enabled == true
    private bool LatchedMoveAssist()
    {
        Vector2 anchor = player.springJoint2D.connectedAnchor;

        // 1) If line to anchor is blocked, push along surface tangent
        Vector2 dirToAnchor = (anchor - player.rb2D.position).normalized;
        RaycastHit2D hit = Physics2D.Raycast(player.rb2D.position, dirToAnchor, 0.6f, solidLayers);

        if (hit)
        {
            // tangent = +/- perpendicular to normal, choose the one closer to rope direction
            Vector2 n = hit.normal;
            Vector2 t = new Vector2(-n.y, n.x);
            float sign = Mathf.Sign(Vector2.Dot(t, dirToAnchor));
            t *= sign;
            player.rb2D.AddForce(t * pullAssistForce, ForceMode2D.Force);
        }

        // 2) Enforce some forward progress (component of velocity along rope)
        float vAlong = Vector2.Dot(player.rb2D.linearVelocity, dirToAnchor);
        if (vAlong < minPullSpeed) player.rb2D.linearVelocity = player.rb2D.linearVelocity - dirToAnchor * vAlong + dirToAnchor * minPullSpeed;

        // 3) Stall watchdog: cancel if distance not shrinking enough
        float dist = Vector2.Distance(player.rb2D.position, anchor);
        if (Mathf.Abs(_lastDistToAnchor - dist) < stallProgressEpsilon) _stallTimer += Time.deltaTime;
        else
        {
            _stallTimer = 0f;
            _lastDistToAnchor = dist;
        }

        return _stallTimer > stallTimeout; // true => cancel
    }

    public void ReleaseGrapple(bool isMultiplayer)
    {
        EndLatchedState();

        // OLD
        ResetGrapple();
        DestroyProjectile(isMultiplayer);
        InputManager.dodgeRollDisabled = false;
    }

    private void ResetGrapple()
    {
        Physics2D.IgnoreLayerCollision(playerLayer, poolLayer, false); // re-enable

        player.springJoint2D.enabled = false;
        player.isHuntersReachActive = false;
        hasGrappled = false;
        currentRopeLength = 0f;
        isHookedToTheWall = false;
        missRetractStarted = false;
        isGrappleReleased = true;
        grappleHasDealtDamage = false;

        transform.SetParent(belongingParent);

        grappleHeadTransform.GetComponent<SpriteRenderer>().enabled = false;

        grappleHeadTransform.localPosition = Vector3.zero;
        grappleHeadTransform.localEulerAngles = Vector3.zero;
        ropeTransform.localPosition = Vector3.zero;
        ropeTransform.localEulerAngles = Vector3.zero;

        EndLatchedState();
    }

    IEnumerator RetractMissedGrapple(bool isMultiplayer)
    {
        float retractSpeed = 50f;

        while (currentRopeLength > 0.5f)
        {
            currentRopeLength = Mathf.MoveTowards(currentRopeLength, 0f, retractSpeed * Time.deltaTime);

            // Visually shrink rope
            ropeSpriteRenderer.size = new Vector2(currentRopeLength, ropeSpriteRenderer.size.y);

            // Rope direction still same
            Vector3 startPos = grappleStartPoint.position;
            Vector2 direction = fireDirectionVector.normalized;
            float angle = HelperUtilities.GetAngleFromVector(direction);
            ropeSpriteRenderer.transform.position = startPos;
            ropeSpriteRenderer.transform.rotation = Quaternion.Euler(0f, 0f, angle);

            // Move hook head along with rope tip
            Vector3 hookTip = startPos + (Vector3)(direction * currentRopeLength);
            grappleHeadTransform.position = hookTip;
            grappleHeadTransform.rotation = Quaternion.Euler(0f, 0f, angle);

            yield return null;
        }

        // After fully retracted
        ReleaseGrapple(isMultiplayer); // Safely reset and return to pool
    }

    private bool IsBeamAbovePlayer(ref Vector3 direction)
    {
        Vector3 playerPos = Vector3.zero;

        if (player != null)
        {
            playerPos = player.transform.position + new Vector3(0f, 0.5f, 0f);
        }

        Vector3 laserPos = laserStartPoint.position;
        Vector3 laserDirection = direction.normalized; // Laser's firing direction

        // Calculate a perpendicular vector to the laser's direction
        Vector3 perpendicularVector = new Vector2(-laserDirection.y, laserDirection.x);

        // Check if the player is above or below the laser line
        float positionCheck = Vector2.Dot(playerPos - laserPos, perpendicularVector);

        if (positionCheck > 0)
        {
            return false;
        }
        else
        {
            return true;
        }
    }

    private void DealLaserDamage(Collider2D collider)
    {
        int inflictedDamage = 15;

        DealDamage(collider, ref inflictedDamage, true);
    }

    IEnumerator DestroyProcess(float disableDuration)
    {
        yield return new WaitForSeconds(disableDuration);

        if (isGrappleHook) grappleHeadTransform.GetComponent<SpriteRenderer>().enabled = true;

        if (isThrowingAxe)
        {
            //dropOnAxeThrow.DropProcess();
            player.isAxeThrowActive = false;
        }

        isHittingWall = false;

        // Multiplayer
        if (projectileNetwork != null)
        {
            if (NetworkServer.active) NetworkServer.Destroy(gameObject);

            yield break;
        }

        // Single Player
        Destroy(gameObject);
    }

    /// <summary>
    /// Display the projectile hit effect
    /// </summary>
    private void ProjectileHitEffect()
    {
        //if (!NetworkServer.active && !NetworkClient.active) projectileEffectPrefab = projectileDetails.projectileHitEffect != null ? projectileDetails.projectileHitEffect.projectileHitEffectPrefab : null;
        //else projectileEffectPrefab = projectileDetails.projectileHitEffect != null ? projectileDetails.projectileHitEffect.projectileHitEffectPrefabMP : null;

        GameObject projectileEffectPrefab = projectileDetails.projectileHitEffect != null ? projectileDetails.projectileHitEffect.projectileHitEffectPrefab : null;

        // Process if a hit effect has been specified
        if (projectileDetails.projectileHitEffect != null && projectileEffectPrefab != null)
        {
            GameObject projGameObject = Instantiate(projectileEffectPrefab, transform.position, Quaternion.identity);
            ProjectileHitEffect projectileHitEffect = projGameObject.GetComponent<ProjectileHitEffect>();

            ProjectileHitEffectSO hitEffectDetails = projectileDetails.projectileHitEffect;

            // Set Hit Effect
            projectileHitEffect.SetHitEffect(hitEffectDetails.duration, hitEffectDetails.startParticleSize, hitEffectDetails.startParticleSpeed, hitEffectDetails.startLifetime, hitEffectDetails.effectGravity, hitEffectDetails.maxParticleNumber,
                hitEffectDetails.emissionRate, hitEffectDetails.burstParticleNumber, hitEffectDetails.velocityOverLifetimeMin, hitEffectDetails.velocityOverLifetimeMax);

            // Set gameobject active (the particle system is set to automatically disable the gameobject once finished)
            projectileHitEffect.gameObject.SetActive(true);
        }
    }

    #region CC PLAYER

    /// <summary>
    /// Check bleeding status - Player
    /// </summary>
    private void CheckBleedingStatusOnPlayer(Player player)
    {
        if (player.isImmunetoBleeding) return;

        // Check get poisoned
        float randomDice = Random.Range(0f, 1f);
        if (randomDice < projectileDetails.bleedingChance - player.currentStatusResistance)
        {
            player.statusEffectAnimators.bleedAnimator.SetTrigger(Settings.activateVFX);
            player.healthEvent.CallGetBleedingEvent();
            player.healthStatus |= HealthStatus.Bleeding; // Add Bleeding status
        }
    }

    /// <summary>
    /// Check warmed status - Player
    /// </summary>
    public void CheckWarmStatusOnPlayer(Player player)
    {
        if (player.isImmunetoBurn) return;

        bool isWarmed = (player.healthStatus & HealthStatus.Burned) != 0;

        if (!isWarmed)
        {
            float randomDice = Random.Range(0f, 1f);

            if (randomDice < projectileDetails.warmChance - player.currentStatusResistance)
            {
                if (player.isWarmed && !isWarmed)
                {
                    player.playerControl.isPlayerRolling = false;

                    player.healthStatus |= HealthStatus.Burned;
                    player.healthEvent.CallGetBurnEvent();
                }
                else if (!player.isWarmed && !isWarmed)
                {
                    player.isWarmed = true;
                    player.healthEvent.CallGetWarmedEvent();
                }
            }
        }
    }

    /// <summary>
    /// Check burn status - Player
    /// </summary>
    private void CheckBurnStatusOnPlayer(Player player)
    {
        if (player.isImmunetoBurn) return;

        // Check get poisoned
        float randomDice = Random.Range(0f, 1f);
        if (randomDice < projectileDetails.burnChance - player.currentStatusResistance)
        {
            player.statusEffectAnimators.burnAnimator.SetTrigger(Settings.activateVFX);
            player.healthEvent.CallGetBurnEvent();
            player.healthStatus |= HealthStatus.Burned; // Add Burned status
        }
    }

    /// <summary>
    /// Check slow status - Player
    /// </summary>
    private void CheckSlowStatusOnPlayer(Player player)
    {
        if (player.isImmunetoSlow) return;

        if (!player.isSlowed)
        {
            // Check get poisoned
            float randomDice = Random.Range(0f, 1f);
            if (randomDice < projectileDetails.slowChance - player.currentStatusResistance)
            {
                player.statusEffectAnimators.slowAnimator.SetTrigger(Settings.activateVFX);
                player.healthEvent.CallGetSlowEvent();
                player.isSlowed = true;
            }
        }
    }

    /// <summary>
    /// Check poison status - Player
    /// </summary>
    private void CheckPoisonStatusOnPlayer(Player player)
    {
        if (player.isImmunetoPoison) return;

        // Check get poisoned
        float randomDice = Random.Range(0f, 1f);
        if (randomDice < projectileDetails.poisonChance - player.currentStatusResistance)
        {
            player.statusEffectAnimators.poisonAnimator.SetTrigger(Settings.activateVFX);
            player.healthEvent.CallGetPoisonedEvent();
            player.healthStatus |= HealthStatus.Poisoned; // Add Poisoned status
        }
    }

    /// <summary>
    /// Check stun status - Player
    /// </summary>
    private void CheckStunStatusOnPlayer(Player player)
    {
        bool isStunned = (player.moveStatus & MoveStatus.Stun) != 0;

        if (!isStunned)
        {
            float randomDice = Random.Range(0f, 1f);
            if (randomDice < projectileDetails.stunChance - player.currentStatusResistance)
            {
                player.playerControl.isPlayerRolling = false;

                player.statusEffectAnimators.stunAnimator.SetTrigger(Settings.activateVFX);
                player.moveStatus |= MoveStatus.Stun;
                player.healthEvent.CallGetStunEvent();
                player.rb2D.constraints = RigidbodyConstraints2D.FreezeAll;
                player.animator.SetBool(Settings.isStunned, true);
            }
        }
    }

    /// <summary>
    /// Check root status - Player
    /// </summary>
    private void CheckRootStatusOnPlayer(Player player)
    {
        bool isRooted = (player.moveStatus & MoveStatus.Root) != 0;

        if (!isRooted)
        {
            float randomDice = Random.Range(0f, 1f);
            if (randomDice < projectileDetails.rootChance - player.currentStatusResistance)
            {
                player.playerControl.isPlayerRolling = false;

                player.statusEffectAnimators.rootAnimator.SetTrigger(Settings.activateVFX);
                player.moveStatus |= MoveStatus.Root;
                player.healthEvent.CallGetRootEvent();
                player.rb2D.constraints = RigidbodyConstraints2D.FreezeAll;
            }
        }
    }

    /// <summary>
    /// Check chill status - Player
    /// </summary>
    private void CheckChillStatusOnPlayer(Player player)
    {
        if (player.isImmunetoFrost) return;

        bool isFrozen = (player.moveStatus & MoveStatus.Frozen) != 0;

        if (!isFrozen)
        {
            float randomDice = Random.Range(0f, 1f);

            if (randomDice < projectileDetails.chillChance - player.currentStatusResistance)
            {
                if (player.isChilled && !isFrozen)
                {
                    player.playerControl.isPlayerRolling = false;

                    player.moveStatus |= MoveStatus.Frozen;
                    player.healthEvent.CallGetFrostEvent();
                    player.rb2D.constraints = RigidbodyConstraints2D.FreezeAll;
                    player.animatePlayer.ResetAnimatonParameters();
                    player.animator.SetBool(Settings.isFrozen, true);

                    player.healthEvent.CallChillCuredEvent();
                }
                else if (!player.isChilled && !isFrozen)
                {
                    player.isChilled = true;
                    player.healthEvent.CallGetChillEvent();
                }
            }
        }
    }

    /// <summary>
    /// Check static status - Player
    /// </summary>
    private void CheckStaticStatusOnPlayer(Player player)
    {
        if (player.isImmunetoParalyze) return;

        bool isParalyzed = (player.moveStatus & MoveStatus.Paralyze) != 0;

        if (!isParalyzed)
        {
            float randomDice = Random.Range(0f, 1f);

            if (randomDice < projectileDetails.staticChance - player.currentStatusResistance)
            {
                if (player.isStatic && !isParalyzed)
                {
                    player.playerControl.isPlayerRolling = false;

                    player.moveStatus |= MoveStatus.Paralyze;
                    player.healthEvent.CallGetParalyzedEvent();
                    player.rb2D.constraints = RigidbodyConstraints2D.FreezeAll;
                    player.animatePlayer.ResetAnimatonParameters();

                    player.healthEvent.CallStaticCuredEvent();
                }
                else if (!player.isStatic && !isParalyzed)
                {
                    player.isStatic = true;
                    player.healthEvent.CallGetStaticEvent();
                }
            }
        }
    }

    /// <summary>
    /// Check frost status - Player
    /// </summary>
    private void CheckFrostStatusOnPlayer(Player player)
    {
        if (player.isImmunetoFrost) return;

        bool isFrozen = (player.moveStatus & MoveStatus.Frozen) != 0;

        if (!isFrozen)
        {
            float randomDice = Random.Range(0f, 1f);
            if (randomDice < projectileDetails.frostChance - player.currentStatusResistance)
            {
                player.playerControl.isPlayerRolling = false;

                player.statusEffectAnimators.frostAnimator.SetTrigger(Settings.activateVFX);
                player.moveStatus |= MoveStatus.Frozen;
                player.healthEvent.CallGetFrostEvent();
                player.rb2D.constraints = RigidbodyConstraints2D.FreezeAll;
                player.animatePlayer.ResetAnimatonParameters();
                player.animator.SetBool(Settings.isFrozen, true);
            }
        }
    }

    /// <summary>
    /// Check paralyze status - Player
    /// </summary>
    private void CheckParalyzeStatusOnPlayer(Player player)
    {
        if (player.isImmunetoParalyze) return;

        bool isParalyzed = (player.moveStatus & MoveStatus.Paralyze) != 0;

        if (!isParalyzed)
        {
            float randomDice = Random.Range(0f, 1f);
            if (randomDice < projectileDetails.paralyzeChance - player.currentStatusResistance)
            {
                player.playerControl.isPlayerRolling = false;

                player.statusEffectAnimators.paralyzeAnimator.SetTrigger(Settings.activateVFX);
                player.moveStatus |= MoveStatus.Paralyze;
                player.healthEvent.CallGetParalyzedEvent();
                player.rb2D.constraints = RigidbodyConstraints2D.FreezeAll;
                player.animatePlayer.ResetAnimatonParameters();
            }
        }
    }

    /// <summary>
    /// Check blind status - Player
    /// </summary>
    private void CheckBlindStatusOnPlayer(Player player)
    {
        if (player.isImmunetoBlind) return;

        float randomDice = Random.Range(0f, 1f);
        if (randomDice < projectileDetails.blindChance - player.currentStatusResistance)
        {
            player.statusEffectAnimators.blindAnimator.SetTrigger(Settings.activateVFX);
            player.healthEvent.CallGetBlindEvent();
        }
    }

    /// <summary>
    /// Check curse status - Player
    /// </summary>
    private void CheckCurseStatusOnPlayer(Player player)
    {
        if (!player.isCursed)
        {
            float randomDice = Random.Range(0f, 1f);
            if (randomDice < projectileDetails.curseChance - player.currentStatusResistance)
            {
                player.statusEffectAnimators.curseAnimator.SetTrigger(Settings.activateVFX);
                player.isCursed = true;
                player.healthEvent.CallGetCurseEvent();
            }
        }
    }

    /// <summary>
    /// Check fear status - Player
    /// </summary>
    private void CheckFearStatusOnPlayer(Player player)
    {
        if (!player.isFeared)
        {
            float randomDice = Random.Range(0f, 1f);
            if (randomDice < projectileDetails.fearChance - player.currentStatusResistance)
            {
                player.statusEffectAnimators.fearAnimator.SetTrigger(Settings.activateVFX);
                player.isFeared = true;
                player.healthEvent.CallGetFearEvent();
            }
        }
    }
    #endregion

    #region CC ENEMY
    /// <summary>
    /// Check bleeding status - Enemy
    /// </summary>
    private void CheckBleedingStatusOnEnemy(Enemy enemy, bool arrowOfTheSeven = false, bool isThrowingAxe = false)
    {
        if (!NetworkServer.active && !NetworkClient.active)
        {
            if (enemy.enemyDetails.isImmuneToBleeding) return;

            if (arrowOfTheSeven)
            {
                if (player != null)
                {
                    enemy.bleedDuration = player.playerDetails.fourthActiveSkillDetails.GetCurrentActiveLevel() switch
                    {
                        1 => 6,
                        2 => 7,
                        3 => 8,
                        _ => 2 // Default
                    };
                }
            }

            float bleedingChance = 0f;

            if (isThrowingAxe)
            {
                if (player != null)
                {
                    bleedingChance = player.playerDetails.thirdActiveSkillDetails.GetCurrentActiveLevel() switch
                    {
                        1 => 0.2f,
                        2 => 0.3f,
                        3 => 0.45f,
                        _ => 0f // Default
                    };
                }
            }

            // Check get bleeding
            float randomDice = Random.Range(0f, 1f);

            if (randomDice < projectileDetails.bleedingChance + bleedingChance + player.additionalStatusEffectInflictModifier + player.additionalBleedChance
                || arrowOfTheSeven)
            {
                enemy.statusEffectAnimators.bleedAnimator.SetTrigger(Settings.activateVFX);
                enemy.healthEvent.CallGetBleedingEvent();
                enemy.healthStatus |= HealthStatus.Bleeding; // Add Bleeding status
            }
        }
        else
        {
            player.NetAuth.CmdApplyBleeding(enemy.enemyNetwork.netIdentity, player.NetAuth.netId, projectileIndex, arrowOfTheSeven, isThrowingAxe);
        }
    }

    /// <summary>
    /// Check warmed status - Enemy
    /// </summary>
    public void CheckWarmStatusOnEnemy(Enemy enemy)
    {
        EnemyAI enemyAI = enemy.GetComponent<EnemyAI>();

        bool isWarmed = (enemy.healthStatus & HealthStatus.Burned) != 0;

        if (!isWarmed)
        {
            float randomDice = Random.Range(0f, 1f);

            if (randomDice < projectileDetails.warmChance)
            {
                if (enemy.isWarmed && !isWarmed)
                {
                    enemy.healthEvent.CallGetBurnEvent();
                    enemy.healthStatus |= HealthStatus.Burned; // Add Burned status

                    enemy.healthEvent.CallWarmCuredEvent();
                }
                else if (!enemy.isWarmed && !isWarmed)
                {
                    enemy.isWarmed = true;
                    enemy.healthEvent.CallGetWarmedEvent();
                }
            }
        }
    }

    /// <summary>
    /// Check burn status - Enemy
    /// </summary>
    private void CheckBurnStatusOnEnemy(Enemy enemy, bool arrowOfTheSeven = false)
    {
        if (enemy.enemyDetails.isImmuneToBurn) return;

        if (arrowOfTheSeven)
        {
            if (player != null)
            {
                enemy.burnDuration = player.playerDetails.fourthActiveSkillDetails.GetCurrentActiveLevel() switch
                {
                    1 => 6,
                    2 => 7,
                    3 => 8,
                    _ => 2 // Default
                };
            }
        }

        // Check get burn
        float randomDice = Random.Range(0f, 1f);
        if (randomDice < projectileDetails.burnChance + player.additionalStatusEffectInflictModifier + player.additionalBurnChance || arrowOfTheSeven)
        {
            enemy.statusEffectAnimators.burnAnimator.SetTrigger(Settings.activateVFX);
            enemy.healthEvent.CallGetBurnEvent();
            enemy.healthStatus |= HealthStatus.Burned; // Add Burned status
        }
    }

    /// <summary>
    /// Check slow status - Enemy
    /// </summary>
    private void CheckSlowStatusOnEnemy(Enemy enemy, bool arrowOfTheSeven = false)
    {
        if (enemy.enemyDetails.isImmuneToSlow) return;

        if (!enemy.isSlowed)
        {
            if (arrowOfTheSeven)
            {
                if (player != null)
                {
                    enemy.slowDuration = player.playerDetails.fourthActiveSkillDetails.GetCurrentActiveLevel() switch
                    {
                        1 => 6,
                        2 => 7,
                        3 => 8,
                        _ => 2 // Default
                    };
                }
            }

            // Check get bleeding
            float randomDice = Random.Range(0f, 1f);
            if (randomDice < projectileDetails.slowChance + player.additionalStatusEffectInflictModifier + player.additionalSlowChance || arrowOfTheSeven)
            {
                enemy.statusEffectAnimators.slowAnimator.SetTrigger(Settings.activateVFX);
                enemy.healthEvent.CallGetSlowEvent();
                enemy.isSlowed = true;
            }
        }
    }

    /// <summary>
    /// Check poison status - Enemy
    /// </summary>
    private void CheckPoisonStatusOnEnemy(Enemy enemy, bool arrowOfTheSeven = false)
    {
        if (enemy.enemyDetails.isImmuneToPoison) return;

        if (arrowOfTheSeven)
        {
            if (player != null)
            {
                enemy.poisonDuration = player.playerDetails.fourthActiveSkillDetails.GetCurrentActiveLevel() switch
                {
                    1 => 6,
                    2 => 7,
                    3 => 8,
                    _ => 2 // Default
                };
            }
        }

        // Check get poison
        float randomDice = Random.Range(0f, 1f);
        if (randomDice < projectileDetails.poisonChance + player.additionalStatusEffectInflictModifier + player.additionalPoisonChance || arrowOfTheSeven)
        {
            enemy.statusEffectAnimators.poisonAnimator.SetTrigger(Settings.activateVFX);
            enemy.healthEvent.CallGetPoisonedEvent();
            enemy.healthStatus |= HealthStatus.Poisoned; // Add Poisoned status
        }
    }

    /// <summary>
    /// Check stun status - Enemy
    /// </summary>
    private void CheckStunStatusOnEnemy(Enemy enemy)
    {
        if (!NetworkServer.active && !NetworkClient.active)
        {
            if (enemy.enemyDetails.isImmuneToStun) return;

            bool isStunned = (enemy.moveStatus & MoveStatus.Stun) != 0;

            if (!isStunned)
            {
                float randomDice = Random.Range(0f, 1f);
                if (randomDice < projectileDetails.stunChance + player.additionalStatusEffectInflictModifier + player.additionalStunChance)
                {
                    StartCoroutine(StunRoutine(enemy));
                }
            }
        }
        else
        {
            player.NetAuth.CmdApplyStun(enemy.enemyNetwork.netIdentity, player.NetAuth.netId, projectileIndex);
        }
    }

    /// <summary>
    /// Check root status - Enemy
    /// </summary>
    private void CheckRootStatusOnEnemy(Enemy enemy, uint projectileOwnerNetId, int projectileIndex, bool isBindingArrow = false)
    {
        if (!NetworkServer.active && !NetworkClient.active)
        {
            if (enemy.enemyDetails.isImmuneToRoot) return;

            bool isRooted = (enemy.moveStatus & MoveStatus.Root) != 0;

            if ((!isRooted && enemy.health.currentHealth > 0) || isBindingArrow)
            {
                if (isBindingArrow)
                {
                    if (player != null)
                    {
                        enemy.rootDuration = player.playerDetails.thirdActiveSkillDetails.GetCurrentActiveLevel() switch
                        {
                            1 => 3,
                            2 => 4,
                            3 => 5,
                            _ => 2 // Default
                        };
                    }
                }

                float randomDice = Random.Range(0f, 1f);

                if (randomDice < projectileDetails.rootChance + player.additionalStatusEffectInflictModifier + player.additionalRootChance || isBindingArrow)
                {
                    StartCoroutine(RootRoutine(enemy));
                }
            }
        }
        else
        {
            player.NetAuth.CmdApplyRoot(enemy.enemyNetwork.netIdentity, player.NetAuth.netId, projectileIndex, isBindingArrow);
        }
    }

    /// <summary>
    /// Check chill status - Enemy
    /// </summary>
    public void CheckChillStatusOnEnemy(Enemy enemy, bool isIceBreaker = false)
    {
        bool isFrozen = (enemy.moveStatus & MoveStatus.Frozen) != 0;

        if (!isFrozen)
        {
            float randomDice = Random.Range(0f, 1f);

            if (randomDice < projectileDetails.chillChance)
            {
                if (enemy.isChilled && !isFrozen)
                {
                    StartCoroutine(FrostRoutine(enemy)); // Second chill

                    enemy.healthEvent.CallChillCuredEvent();
                }
                else if (!enemy.isChilled && !isFrozen)
                {
                    enemy.isChilled = true;
                    enemy.healthEvent.CallGetChillEvent();
                }
            }
        }
    }

    /// <summary>
    /// Check static status - Enemy
    /// </summary>
    public void CheckStaticStatusOnEnemy(Enemy enemy, bool isVindictiveTouchActive = false)
    {
        bool isParalyzed = (enemy.moveStatus & MoveStatus.Paralyze) != 0;

        if (!isParalyzed)
        {
            float randomDice = Random.Range(0f, 1f);

            if (randomDice < projectileDetails.staticChance || isVindictiveTouchActive)
            {
                if (enemy.isStatic && !isParalyzed)
                {
                    StartCoroutine(ParalyzeRoutine(enemy)); // Second static

                    enemy.healthEvent.CallStaticCuredEvent();
                }
                else if (!enemy.isStatic && !isParalyzed)
                {
                    enemy.isStatic = true;
                    enemy.healthEvent.CallGetStaticEvent();
                }
            }
        }
    }

    /// <summary>
    /// Check frost status - Enemy
    /// </summary>
    private void CheckFrostStatusOnEnemy(Enemy enemy, bool arrowOfTheSeven = false)
    {
        if (enemy.enemyDetails.isImmuneToFrost) return;

        bool isFrozen = (enemy.moveStatus & MoveStatus.Frozen) != 0;

        if (!isFrozen || arrowOfTheSeven)
        {
            if (arrowOfTheSeven)
            {
                if (player != null)
                {
                    enemy.freezeDuration = player.playerDetails.fourthActiveSkillDetails.GetCurrentActiveLevel() switch
                    {
                        1 => 6,
                        2 => 7,
                        3 => 8,
                        _ => 2 // Default
                    };
                }
            }

            float randomDice = Random.Range(0f, 1f);
            if (randomDice < projectileDetails.frostChance + player.additionalStatusEffectInflictModifier + player.additionalFreezeChance || arrowOfTheSeven)
            {
                StartCoroutine(FrostRoutine(enemy));
            }
        }
    }

    /// <summary>
    /// Check paralyze status - Enemy
    /// </summary>
    private void CheckParalyzeStatusOnEnemy(Enemy enemy, bool arrowOfTheSeven = false)
    {
        if (enemy.enemyDetails.isImmuneToParalyze) return;

        bool isParalyzed = (enemy.moveStatus & MoveStatus.Paralyze) != 0;

        if (!isParalyzed || arrowOfTheSeven)
        {
            if (arrowOfTheSeven)
            {
                if (player != null)
                {
                    enemy.paralyzeDuration = player.playerDetails.fourthActiveSkillDetails.GetCurrentActiveLevel() switch
                    {
                        1 => 6,
                        2 => 7,
                        3 => 8,
                        _ => 2 // Default
                    };
                }
            }

            float randomDice = Random.Range(0f, 1f);
            if (randomDice < projectileDetails.paralyzeChance + player.additionalStatusEffectInflictModifier + player.additionalParalyzeChance || arrowOfTheSeven)
            {
                StartCoroutine(ParalyzeRoutine(enemy));
            }
        }
    }

    /// <summary>
    /// Check shatter status - Enemy
    /// </summary>
    private void CheckShatterStatusOnEnemy(Enemy enemy, ref int inflictedDamage, bool isIceBreaker = false)
    {
        Health enemyHealth = enemy.GetComponent<Health>();

        bool isFrozen = (enemy.moveStatus & MoveStatus.Frozen) != 0;

        float randomDice = Random.Range(0f, 1f);

        if (isFrozen && (randomDice < 0.1f || isIceBreaker))
        {
            inflictedDamage = (int)(inflictedDamage * 1.5f);

            if (isIceBreaker)
            {
                float iceBreakerModifier = player.playerDetails.fourthActiveSkillDetails.GetCurrentActiveLevel() switch
                {
                    1 => 1.5f,
                    2 => 1.7f,
                    3 => 2f,
                    _ => 1.5f
                };

                inflictedDamage = (int)(inflictedDamage * iceBreakerModifier);
            }

            enemy.isShattered = true;

            DamageContext ctx = new DamageContext { owner = DamageOwner.Player, source = DamageSourceType.Projectile, dealerPosition = transform.position, receiverPosition = enemy.transform.position };
            ReceiveProjectileDamage receiveProjectileDamage = enemy.GetComponent<ReceiveProjectileDamage>();
            receiveProjectileDamage.TakeProjectileDamage(inflictedDamage, ctx);
            enemy.healthEvent.CallGetShatteredEvent();

            if (player.NetAuth == null) WorldSoundManager.Instance.PlayWorldSound(enemy.enemyDetails.suddenDeathSoundEffect, transform.position);
            else NetworkSoundManager.Instance.CmdPlaySound(SoundName.SuddenDeath, transform.position);
        }
    }

    /// <summary>
    /// Check blind status - Enemy
    /// </summary>
    private void CheckBlindStatusOnEnemy(Enemy enemy, bool arrowOfTheSeven = false)
    {
        if (enemy.enemyDetails.isImmuneToBlind) return;

        if (arrowOfTheSeven)
        {
            if (arrowOfTheSeven)
            {
                if (player != null)
                {
                    enemy.blindDuration = player.playerDetails.fourthActiveSkillDetails.GetCurrentActiveLevel() switch
                    {
                        1 => 6,
                        2 => 7,
                        3 => 8,
                        _ => 2 // Default
                    };
                }
            }

            float randomDice = Random.Range(0f, 1f);

            if (randomDice < projectileDetails.blindChance + player.additionalStatusEffectInflictModifier + player.additionalBlindChance +
                player.additionalBlindMakerModifier || arrowOfTheSeven)
            {
                enemy.statusEffectAnimators.blindAnimator.SetTrigger(Settings.activateVFX);
                enemy.healthEvent.CallGetBlindEvent();
            }
        }
    }

    /// <summary>
    /// Check curse status - Enemy
    /// </summary>
    private void CheckCurseStatusOnEnemy(Enemy enemy)
    {
        if (enemy.enemyDetails.isImmuneToCurse) return;

        if (!enemy.isCursed)
        {
            float randomDice = Random.Range(0f, 1f);
            if (randomDice < projectileDetails.curseChance + player.additionalStatusEffectInflictModifier + player.additionalCurseChance)
            {
                enemy.statusEffectAnimators.curseAnimator.SetTrigger(Settings.activateVFX);
                enemy.isCursed = true;
                enemy.healthEvent.CallGetCurseEvent();
            }
        }
    }

    /// <summary>
    /// Check curse status - Enemy
    /// </summary>
    private void CheckFearStatusOnEnemy(Enemy enemy, bool arrowOfTheSeven = false)
    {
        if (enemy.enemyDetails.isImmuneToCurse) return;

        if (!enemy.isFeared || arrowOfTheSeven)
        {
            if (arrowOfTheSeven)
            {
                if (player != null)
                {
                    enemy.fearDuration = player.playerDetails.fourthActiveSkillDetails.GetCurrentActiveLevel() switch
                    {
                        1 => 6,
                        2 => 7,
                        3 => 8,
                        _ => 2 // Default
                    };
                }
            }

            float randomDice = Random.Range(0f, 1f);
            if (randomDice < projectileDetails.fearChance + player.additionalStatusEffectInflictModifier + player.additionalFearChance || arrowOfTheSeven)
            {
                enemy.statusEffectAnimators.fearAnimator.SetTrigger(Settings.activateVFX);
                enemy.isFeared = true;
                enemy.healthEvent.CallGetFearEvent();
            }
        }
    }
    #endregion

    #region CC Routines

    IEnumerator StunRoutine(Enemy enemy)
    {
        enemy.statusEffectAnimators.stunAnimator.SetTrigger(Settings.activateVFX);
        enemy.moveStatus |= MoveStatus.Stun;
        enemy.healthEvent.CallGetStunEvent();
        enemy.rb2D.constraints = RigidbodyConstraints2D.FreezeAll;
        enemy.animator.SetBool(Settings.isStunned, true);

        yield return new WaitForFixedUpdate();
    }

    IEnumerator RootRoutine(Enemy enemy)
    {
        enemy.statusEffectAnimators.rootAnimator.SetTrigger(Settings.activateVFX);
        enemy.moveStatus |= MoveStatus.Root;
        enemy.healthEvent.CallGetRootEvent();
        enemy.rb2D.constraints = RigidbodyConstraints2D.FreezeAll;

        yield return new WaitForFixedUpdate();
    }

    IEnumerator FrostRoutine(Enemy enemy)
    {
        enemy.statusEffectAnimators.frostAnimator.SetTrigger(Settings.activateVFX);
        enemy.moveStatus |= MoveStatus.Frozen;
        enemy.healthEvent.CallGetFrostEvent();
        enemy.rb2D.constraints = RigidbodyConstraints2D.FreezeAll;
        enemy.animator.SetBool(Settings.isStunned, true);

        yield return new WaitForFixedUpdate();
    }

    IEnumerator ParalyzeRoutine(Enemy enemy)
    {
        enemy.statusEffectAnimators.paralyzeAnimator.SetTrigger(Settings.activateVFX);
        enemy.moveStatus |= MoveStatus.Paralyze;
        enemy.healthEvent.CallGetParalyzedEvent();
        enemy.rb2D.constraints = RigidbodyConstraints2D.FreezeAll;

        yield return new WaitForFixedUpdate();
    }

    IEnumerator ExplosionRoutine(bool isMultiplayer, bool isEnemy = false)
    {
        Animator animator = GetComponent<Animator>();

        if (isFireBlast)
        {
            animator.SetTrigger("impact");
            animSync?.UpdateProjectileImpact(impact: true);

            if (player.NetAuth == null) WorldSoundManager.Instance.PlayWorldSound(projectileDetails.projectileImpactSoundEffect, transform.position);
            else NetworkSoundManager.Instance.CmdPlaySound(SoundName.BombBurst, transform.position);


            Explosion();

            yield return new WaitForSeconds(0.5f);

            DestroyProjectile(isMultiplayer);
        }
        else if (isEnemy)
        {
            animator.SetTrigger("burst");

            if (player.NetAuth == null) WorldSoundManager.Instance.PlayWorldSound(projectileDetails.projectileImpactSoundEffect, transform.position);
            else NetworkSoundManager.Instance.CmdPlaySound(SoundName.BombBurst, transform.position);

            Explosion(true);

            yield return new WaitForSeconds(0.5f);

            DestroyProjectile(isMultiplayer);
        }
    }

    #endregion

    #region THE REST

    /// <summary>
    /// Based on circle radius of the bomb, detect all enemy colliders for damage
    /// </summary>
    public void Explosion(bool isEnemy = false)
    {
        StaticEventHandler.CallCameraShakeEvent(4, 0.6f);

        filter.SetLayerMask(layerMask);

        Collider2D[] results = new Collider2D[20];
        int hitCount = Physics2D.OverlapCircle(transform.position, blastRadius, filter, results);

        for (int i = 0; i < hitCount; i++)
        {
            Collider2D collider = results[i];
            if (collider == null) continue;

            if (collider is PolygonCollider2D)
            {
                if (isEnemy)
                {
                    // Don't hit yourself if player is also in the collider list
                    if (collider.tag == Settings.enemyTag) continue;

                    if (collider.tag == Settings.playerTag)
                    {
                        Player player = collider.GetComponent<Player>();

                        int inflictedDamage = CalculateDamageAmount(null, true);

                        DamageContext ctx = new DamageContext { source = DamageSourceType.Projectile, dealerPosition = transform.position, receiverPosition = player.transform.position };
                        ReceiveProjectileDamage receiveProjectileDamage = collider.GetComponent<ReceiveProjectileDamage>();
                        receiveProjectileDamage.TakeProjectileDamage(inflictedDamage, ctx);
                        CheckStunStatusOnPlayer(player);

                        // Apply knockback
                        Vector2 knockbackDir = (player.transform.position - transform.position).normalized;
                        float knockbackForce = 0.5f; // Adjust force to your liking

                        player.movementByForce.ApplyKnockback(knockbackDir, knockbackForce, 1);
                    }
                }
                else
                {
                    // Don't hit yourself if player is also in the collider list
                    if (collider.tag == Settings.playerTag) continue;

                    if (collider.tag == Settings.enemyTag)
                    {
                        Enemy enemy = collider.GetComponent<Enemy>();

                        int inflictedDamage = CalculateDamageAmount(enemy);

                        DamageContext ctx = new DamageContext { source = DamageSourceType.Projectile, dealerPosition = transform.position, receiverPosition = enemy.transform.position };
                        ReceiveProjectileDamage receiveProjectileDamage = collider.GetComponent<ReceiveProjectileDamage>();
                        receiveProjectileDamage.TakeProjectileDamage(inflictedDamage, ctx);

                        if (isFireBlast)
                        {
                            CheckBurnStatusOnEnemy(enemy);
                        }

                        if (!enemy.enemyDetails.hasKnockbackResistance && enemy.GetComponent<Health>().currentHealth > 0)
                        {
                            // Knockback
                            Vector2 knockbackDir = (enemy.transform.position - transform.position).normalized;
                            float knockbackForce = 5f;
                            float dealDamageMass = 1f;

                            enemy.movementToPosition.ApplyKnockbackToEnemy(knockbackDir, knockbackForce, dealDamageMass);
                        }
                    }
                }
            }
            else
            {
                if (isEnemy)
                {
                    DamageContext ctx = new DamageContext { source = DamageSourceType.Projectile, dealerPosition = transform.position, receiverPosition = collider.transform.position };
                    ReceiveProjectileDamage receiveProjectileDamage = collider.GetComponent<ReceiveProjectileDamage>();
                    receiveProjectileDamage.TakeProjectileDamage(Random.Range(projectileDetails.burstDamageMin, projectileDetails.burstDamageMax), ctx);
                }
                else
                {
                    if (isFireBlast)
                    {
                        float specialAttackModifier = player.playerDetails.fifthActiveSkillDetails.GetCurrentActiveLevel() switch
                        {
                            1 => 0.1f,
                            2 => 0.2f,
                            3 => 0.35f,
                            _ => 0f
                        };

                        Weapon weapon = player.activeWeapon.GetCurrentMainHandWeapon();

                        damageDone = Random.Range(player.currentMainHandMinDamageValue, player.currentMainHandMaxDamageValue);

                        damageDone = (int)(damageDone * (1 + specialAttackModifier)); // Add additional damage modifiers

                        int inflictedDamage = 0;

                        if (collider.GetComponent<Enemy>() != null)
                        {
                            Enemy enemy = collider.GetComponent<Enemy>();

                            inflictedDamage = (int)(damageDone * enemy.enemyDetails.physicalResistance);
                        }
                        else
                        {
                            inflictedDamage = damageDone;
                        }

                        DamageContext ctx = new DamageContext { owner = DamageOwner.Player, source = DamageSourceType.Projectile, dealerPosition = transform.position, receiverPosition = collider.transform.position };
                        ReceiveProjectileDamage receiveProjectileDamage = collider.GetComponent<ReceiveProjectileDamage>();
                        receiveProjectileDamage.TakeProjectileDamage(inflictedDamage, ctx);
                    }
                }
            }
        }
    }

    /// <summary>
    /// Calculate damage amount
    /// </summary>
    public int CalculateDamageAmount(Enemy enemy, bool isEnemy = false)
    {
        int damageDone = 0;
        int inflictedDamage = 0;
        Health health;

        if (isEnemy)
        {
            // Damage produced by enemy
            damageDone = Random.Range(projectileDetails.burstDamageMin, projectileDetails.burstDamageMax);

            // Damage inflicted to player after deducting player armor
            health = player.GetComponent<Health>();
            int inflictedDamageBeforeDmgReduction = Mathf.RoundToInt(damageDone * (1 - player.currentArmorValue));

            inflictedDamage = Mathf.RoundToInt(Mathf.Max(inflictedDamageBeforeDmgReduction * (1 - player.currentDamageReductionValue), 0));
        }
        else
        {
            if (isFireBlast)
            {
                // Damage produced by player
                damageDone = Random.Range(projectileDetails.burstDamageMin, projectileDetails.burstDamageMax);
            }

            // Damage inflicted to enemy after deducting enemy armor
            health = enemy.GetComponent<Health>();

            float armorAfterPen = Mathf.Max(enemy.currentArmor - player.currentArmorPenetrationValue, 0f);
            inflictedDamage = Mathf.RoundToInt(damageDone * (1 - armorAfterPen));
        }

        return inflictedDamage;
    }

    private Transform FindClosestEnemy()
    {
        float closestDistance = Mathf.Infinity;
        Transform closestEnemy = null;

        Enemy[] enemyArrayInRoom = GameManager.Instance.GetCurrentRoom().instantiatedRoom.enemySpawner.GetComponentsInChildren<Enemy>();

        for (int i = 0; i < enemyArrayInRoom.Length; i++)
        {
            float distance = Vector3.Distance(transform.position, enemyArrayInRoom[i].GetEnemyPosition());

            if (distance < closestDistance)
            {
                closestDistance = distance;
                closestEnemy = enemyArrayInRoom[i].transform;
            }
        }

        return closestEnemy;
    }

    private void OnHitEnemy(Enemy hitEnemy, bool isMultiplayer)
    {
        // Record this hit for the chain
        if (chainHitSet == null) chainHitSet = new HashSet<Enemy>();
        chainHitSet.Add(hitEnemy);

        if (isFireBlast)
        {
            if (explosionRoutine == null)
            {
                velocity = new Vector3(0f, 0f, 1f);
                explosionRoutine = StartCoroutine(ExplosionRoutine(isMultiplayer));
            }
            return;
        }

        // Chain logic: First -> Second -> Third
        if (isChainLightning && (chainLightningPhase == ChainLightningPhase.First || chainLightningPhase == ChainLightningPhase.Second))
        {
            Vector2 origin = hitEnemy.transform.position;

            // Find next target excluding those already hit
            Enemy next = FindClosestEnemyFrom(origin, chainHitSet, chainJumpRadius, hitEnemy);

            if (next != null)
            {
                // Spawn next hop at the hit position
                GameObject projectilePrefab = null;

                if(!NetworkServer.active && !NetworkClient.active) projectilePrefab = projectileDetails.projectilePrefabArray[0];
                else projectilePrefab = projectileDetails.projectilePrefabArray[1];

                GameObject projGameObject = Instantiate(projectilePrefab, origin, Quaternion.identity);
                IFireable projIFireable = projGameObject.GetComponent<IFireable>();

                Projectile nextProj = projIFireable as Projectile;

                // Compute direction/angle from origin to next
                Vector3 toNext = (next.transform.position + new Vector3(0f, 0.7f, 0f) - (Vector3)origin).normalized;
                float aimAngle = HelperUtilities.GetAngleFromVector(toNext);

                // Carry state forward
                nextProj.isChainLightning = true;
                nextProj.chainLightningPhase = chainLightningPhase + 1;
                nextProj.isGuided = true;
                nextProj.target = next.transform;
                nextProj.chainHitSet = chainHitSet;       // share the same set
                nextProj.projectileDetails = projectileDetails;

                // Initialize with the incremented phase (important so Start() doesn�t clobber it)
                nextProj.InitializeProjectile(aimAngle, aimAngle, toNext, projectileSpeed: 35f, ProjectileKind.ChainLightning, projectileDetails, new AttackContext { chainLightningPhase = nextProj.chainLightningPhase },
                    overrideProjectileMovement: false, fallingFromSkies: false, projectileCounter: 1, projectilePerShot: 1, WartheonDatabase.Instance.GetProjectileId(projectileDetails),
                    player.NetAuth.netId, targetNetId: 0, null);  // pass the incremented phase

                // Optional damage falloff
                nextProj.damageDone = Mathf.RoundToInt(damageDone * chainDamageFalloff);

                // Optional small hop delay
                if (chainDelay > 0f) nextProj.StartCoroutine(nextProj.DelayArming(chainDelay));
            }
        }

        // Current projectile is done after resolving the hit/chain
        DestroyProjectile(isMultiplayer);
    }

    private Enemy FindClosestEnemyFrom(Vector2 origin, HashSet<Enemy> exclude, float radius, Enemy ignoreEnemy)
    {
        // Use your layer mask/filter
        Collider2D[] hits = Physics2D.OverlapCircleAll(origin, radius, LayerMask.GetMask("Enemy"));
        Enemy best = null;
        float bestDist = float.MaxValue;

        for (int i = 0; i < hits.Length; i++)
        {
            // Make sure this collider belongs to an Enemy
            Enemy e = hits[i].GetComponent<Enemy>();
            if (e == null || !e.isActiveAndEnabled) continue; // Ignore the same enemy we just hit
            if (exclude != null && exclude.Contains(e)) continue; // Skip any already-hit enemies in this chain
            if (!(hits[i] is PolygonCollider2D)) continue; // Only accept polygon colliders

            // Distance check
            float d = Vector2.SqrMagnitude((Vector2)e.transform.position - origin);
            if (d < bestDist)
            {
                bestDist = d;
                best = e;
            }
        }

        return best;
    }

    // Optional small arming delay so the guided step feels like a hop
    private IEnumerator DelayArming(float delay)
    {
        // Prevent immediate movement if you want (e.g., zero velocity)
        Vector3 cachedVel = velocity;
        velocity = Vector3.zero;
        yield return new WaitForSeconds(delay);
        velocity = cachedVel;
    }

    public void ResetProjectileState()
    {
        StopAllCoroutines();

        enabled = true;

        isStopped = false;
        isColliding = false;
        isProjectileMaterialSet = false;
        directionInitialized = false;
        isHittingWall = false;
        lightningStroke = false;

        hasGrappled = false;
        isGrappleReleased = false;
        grappleHasDealtDamage = false;

        velocity = Vector3.zero;

        projectileRange = 0f;
        projectileSpeed = 0f;

        lifeTimeCountdownTimer = 0f;
        projectileChargeTimer = 0f;
        eventIsSubscribed = false;
        updateGuidedMissleTimer = 0f;

        target = null;
        targetPlayer = null;

        if (trailRenderer != null)
        {
            trailRenderer.Clear();
            trailRenderer.emitting = false;
        }
    }

    private void SetUniqueProjectiles(ProjectileKind projectileKind, AttackContext attackContext)
    {
        isPenetrationArrow = attackContext.isPenetrationArrow;
        isTripleThreat = attackContext.isTripleThreat;
        isBindingArrow = attackContext.isBindingArrow;
        isArrowOfTheSeven = player != null ? player.isArrowOfTheSevenActive : false;
        isIceBreaker = projectileKind == ProjectileKind.IceBreaker;
        isFireBlast = projectileKind == ProjectileKind.FireBlast;
        isBlazingCyclone = projectileKind == ProjectileKind.BlazingCyclone;
        isThrowingAxe = projectileKind == ProjectileKind.ThrowingAxe;
        isShiruken = projectileKind == ProjectileKind.Shiruken;
        isChainLightning = projectileKind == ProjectileKind.ChainLightning;
        chainLightningPhase = attackContext.chainLightningPhase;
    }

    private void SetProjectileChargeSettings(ProjectileDetailsSO projectileDetails)
    {
        if (projectileDetails.projectileChargeTime > 0f)
        {
            // Set projectile charge timer
            projectileChargeTimer = projectileDetails.projectileChargeTime;
            SetProjectileMaterial(projectileDetails.projectileChargeMaterial);
            isProjectileMaterialSet = false;
        }
        else
        {
            projectileChargeTimer = 0f;
            SetProjectileMaterial(projectileDetails.projectileMaterial);
            isProjectileMaterialSet = true;
        }
    }

    private void SetTrailSettings(ProjectileDetailsSO projectileDetails)
    {
        if (projectileDetails.isProjectileTrail)
        {
            trailRenderer.gameObject.SetActive(true);
            trailRenderer.emitting = true;
            trailRenderer.material = projectileDetails.projectileTrailMaterial;
            trailRenderer.startWidth = projectileDetails.projectileTrailStartWidth;
            trailRenderer.endWidth = projectileDetails.projectileTrailEndWidth;
            trailRenderer.time = projectileDetails.projectileTrailTime;
        }
        else
        {
            trailRenderer.emitting = false;
            trailRenderer.gameObject.SetActive(false);
        }
    }

    public void SetProjectileMaterial(Material material)
    {
        if (!isGrappleHook) spriteRenderer.material = material;
        else ropeSpriteRenderer.material = material;
    }

    public GameObject GetGameObject()
    {
        return gameObject;
    }

    public void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.blue;
        Vector3 position = this == null ? Vector3.zero : transform.position;
        Gizmos.DrawWireSphere(position, blastRadius);
    }

    #endregion
}
