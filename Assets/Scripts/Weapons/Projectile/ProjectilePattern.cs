using System.Collections;
using UnityEngine;

public class ProjectilePattern : MonoBehaviour, IFireable
{
    #region Tooltip
    [Tooltip("Populate the array with the child ammo gameobjects")]
    #endregion
    [SerializeField] Projectile[] projectileArray;

    [HideInInspector] public BoomerangPhase boomerangPhase = BoomerangPhase.Aim;
    [HideInInspector] public ShirukenPhase shirukenPhase = ShirukenPhase.Fire;

    float projectileRange;
    float projectileSpeed;
    Vector3 fireDirectionVector;
    float fireDirectionAngle;
    ProjectileDetailsSO projectileDetails;
    ActiveItemDetailsSO activeItemDetails;
    float projectileChargeTimer;

    public GameObject GetGameObject()
    {
        return gameObject;
    }

    // FOR PROJECTILE
    public void InitializeProjectile(Enemy belongingEnemy, bool headShotHappened, ProjectileDetailsSO projectileDetails, float aimAngle, float weaponAimAngle,
        float projectileSpeed, Vector3 weaponAimDirectionVector, bool overrideProjectileMovement, bool fallingFromSkies = false, bool isPenetrationArrow = false,
        int projectileCounter = 0, int totalProjectiles = 0, CentaurPhase centaurPhase = CentaurPhase.None, TreantPhase treantPhase = TreantPhase.None,
        GalvanusPhase galvanusPhase = GalvanusPhase.None, SepharothPhase sepharothPhase = SepharothPhase.None, FrostWrymPhase frostWrymPhase = FrostWrymPhase.None,
        VenomancerPhase venomancerPhase = VenomancerPhase.None, FireWrymPhase fireWrymPhase = FireWrymPhase.None, MoldranPhase moldranPhase = MoldranPhase.None)
    {
        this.projectileDetails = projectileDetails;
        this.projectileSpeed = projectileSpeed;

        // Set fire direction
        SetFireDirection(projectileDetails, aimAngle, weaponAimAngle, weaponAimDirectionVector);

        // Set ammo range
        projectileRange = projectileDetails.projectileRange;

        // Activate ammo pattern gameobject
        gameObject.SetActive(true);

        // Loop through all child ammo and initialise it
        foreach (Projectile projectile in projectileArray)
        {
            projectile.InitializeProjectile(belongingEnemy, false, projectileDetails, aimAngle, weaponAimAngle, projectileSpeed, weaponAimDirectionVector, true);
        }

        // Set ammo charge timer - this will hold the ammo briefly
        if (projectileDetails.projectileChargeTime > 0f)
        {
            projectileChargeTimer = projectileDetails.projectileChargeTime;
        }
        else
        {
            projectileChargeTimer = 0f;
        }
    }

    // FOR ACTIVE ITEM
    public void InitializeProjectile(bool headShotHappened, ActiveItemDetailsSO activeItemDetails, float aimAngle, 
        float weaponAimAngle, float projectileSpeed, Vector3 weaponAimDirectionVector, bool overrideProjectileMovement)
    {
        this.activeItemDetails = activeItemDetails;
        this.projectileSpeed = projectileSpeed;

        // Set fire direction
        SetFireDirection(activeItemDetails, aimAngle, weaponAimAngle, weaponAimDirectionVector);

        // Set ammo range
        projectileRange = activeItemDetails.projectileRange;

        // Activate ammo pattern gameobject
        gameObject.SetActive(true);

        // Loop through all child ammo and initialise it
        foreach (Projectile projectile in projectileArray)
        {
            projectile.InitializeProjectile(false, activeItemDetails, aimAngle, weaponAimAngle, projectileSpeed, weaponAimDirectionVector, true);
        }

        // Set ammo charge timer - this will hold the ammo briefly
        if (activeItemDetails.projectileChargeTime > 0f)
        {
            projectileChargeTimer = activeItemDetails.projectileChargeTime;
        }
        else
        {
            projectileChargeTimer = 0f;
        }
    }

    private void Start()
    {
        if (activeItemDetails != null)
        {
            if (activeItemDetails.activeItemType == ActiveItemType.Boomerang)
            {
                boomerangPhase = BoomerangPhase.Fire;
                SoundEffectManager.Instance.PlaySoundEffect(activeItemDetails.activeItemSwingSoundEffect);
            }
        }
    }

    private void Update()
    {
        // Projectile charge effect
        if (projectileChargeTimer > 0f)
        {
            projectileChargeTimer -= Time.deltaTime;
            return;
        }

        // Calculate distance vector to move projectile
        Vector3 distanceVector = fireDirectionVector * projectileSpeed * Time.deltaTime;

        if (activeItemDetails != null)
        {
            if (activeItemDetails.activeItemType == ActiveItemType.Boomerang)
            {
                if (boomerangPhase == BoomerangPhase.Fire)
                {
                    transform.position += distanceVector;
                }
                else
                {
                    transform.position = Vector3.MoveTowards(transform.position, GameManager.Instance.GetPlayer().GetPlayerPosition(),
                        projectileSpeed * Time.deltaTime);

                    boomerangPhase = BoomerangPhase.Return;

                    if (transform.position == GameManager.Instance.GetPlayer().GetPlayerPosition())
                    {
                        // Boomerang has returned to the player
                        ResetBoomerang();
                        gameObject.SetActive(false);
                    }
                }

            }
            else if (activeItemDetails.activeItemType == ActiveItemType.Shiruken)
            {
                if (shirukenPhase == ShirukenPhase.Fire)
                {
                    transform.position += distanceVector;
                }
                else if(shirukenPhase == ShirukenPhase.Ricochet)
                {
                    // Calculate angle perpendicular to the distance vector
                    float angle = Mathf.Atan2(fireDirectionVector.y, fireDirectionVector.x) + Mathf.PI / 2f;

                    // Calculate normal vector
                    Vector2 normalVector = new Vector2(Mathf.Cos(angle), Mathf.Sin(angle));

                    // Calculate reflection direction
                    transform.position += Vector3.Reflect(fireDirectionVector.normalized, normalVector);

                    // Set shuriken phase to Fire
                    shirukenPhase = ShirukenPhase.Fire;
                }
            }
            else
            {
                transform.position += distanceVector;
            }
        }
        else
        {
            transform.position += distanceVector;
        }

        // Rotate projectile for projectiles and active items
        if (activeItemDetails == null)
        {
            transform.Rotate(new Vector3(0f, 0f, projectileDetails.projectileRotationSpeed * Time.deltaTime));
        }
        else
        {
            transform.Rotate(new Vector3(0f, 0f, activeItemDetails.projectileRotationSpeed * Time.deltaTime));
        }

        // Disable after max range reached
        projectileRange -= distanceVector.magnitude;

        if (projectileRange < 0f)
        {
            if (activeItemDetails != null)
            {
                if (activeItemDetails.activeItemType == ActiveItemType.Boomerang)
                {
                    boomerangPhase = BoomerangPhase.Return;
                }
                else
                {
                    StartCoroutine(DisableProcess());
                }
            }
            else
            {
                StartCoroutine(DisableProcess());
            }
        }
    }

    /// <summary>
    /// Set projectile fire direction based on the input angle and direction adjusted by the random spread - PROJECTILE
    /// </summary>
    private void SetFireDirection(ProjectileDetailsSO projectileDetails, float aimAngle, float weaponAimAngle, Vector3 weaponAimDirectionVector)
    {
        // calculate random spread angle between min and max
        float randomSpread = Random.Range(projectileDetails.projectileSpreadMin, projectileDetails.projectileSpreadMax);

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

        // Adjust projectile fire angle angle by random spread
        fireDirectionAngle += spreadToggle * randomSpread;

        // Set projectile fire direction
        fireDirectionVector = HelperUtilities.GetDirectionVectorFromAngle(fireDirectionAngle);
    }

    /// <summary>
    /// Set projectile fire direction based on the input angle and direction adjusted by the random spread - ACTIVE ITEM
    /// </summary>
    private void SetFireDirection(ActiveItemDetailsSO activeItemDetails, float aimAngle, float weaponAimAngle, Vector3 weaponAimDirectionVector)
    {
        // calculate random spread angle between min and max
        float randomSpread = Random.Range(activeItemDetails.projectileSpreadMin, activeItemDetails.projectileSpreadMax);

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

        // Adjust projectile fire angle angle by random spread
        fireDirectionAngle += spreadToggle * randomSpread;

        // Set projectile fire direction
        fireDirectionVector = HelperUtilities.GetDirectionVectorFromAngle(fireDirectionAngle);
    }

    /// <summary>
    /// Disable the projectile - thus returning it to the object pool
    /// </summary>
    IEnumerator DisableProcess()
    {
        yield return new WaitForSeconds(0.3f);

        gameObject.SetActive(false);
    }

    private void ResetBoomerang()
    {
        boomerangPhase = BoomerangPhase.Aim;

        GameManager.Instance.GetPlayer().selectedActiveItem.IncreaseRemainingProjectile(1);
    }

    #region Validation
#if UNITY_EDITOR
    private void OnValidate()
    {
        HelperUtilities.ValidateCheckEnumerableValues(this, nameof(projectileArray), projectileArray);
    }
#endif
    #endregion
}
