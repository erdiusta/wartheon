using UnityEngine;

public class ProjectilePattern : MonoBehaviour
{
    #region Tooltip
    [Tooltip("Populate the array with the child ammo gameobjects")]
    #endregion
    [SerializeField] Projectile[] projectileArray;

    float projectileRange;
    float projectileSpeed;
    Vector3 fireDirectionVector;
    float fireDirectionAngle;
    ProjectileDetailsSO projectileDetails;
    float projectileChargeTimer;

    public GameObject GetGameObject()
    {
        return gameObject;
    }

    public void InitializeProjectile(ProjectileDetailsSO projectileDetails, float aimAngle, float weaponAimAngle, float projectileSpeed, 
        Vector3 weaponAimDirectionVector, bool overrideProjectileMovement)
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
            projectile.InitializeProjectile(projectileDetails, aimAngle, weaponAimAngle, projectileSpeed, weaponAimDirectionVector, true);
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

        transform.position += distanceVector;

        // Rotate projectile
        transform.Rotate(new Vector3(0f, 0f, projectileDetails.projectileRotationSpeed * Time.deltaTime));

        // Disable after max range reached
        projectileRange -= distanceVector.magnitude;

        if (projectileRange < 0f)
        {
            DisableProjectile();
        }
    }

    /// <summary>
    /// Set projectile fire direction based on the input angle and direction adjusted by the
    /// random spread
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
    /// Disable the projectile - thus returning it to the object pool
    /// </summary>
    private void DisableProjectile()
    {
        // Disable the projectile pattern game object
        gameObject.SetActive(false);
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
