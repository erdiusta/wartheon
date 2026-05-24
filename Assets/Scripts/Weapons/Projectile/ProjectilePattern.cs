using Mirror;
using Mirror.Examples.Tanks;
using System.Collections;
using UnityEngine;

public class ProjectilePattern : MonoBehaviour, IFireable
{
    #region Tooltip
    [Tooltip("Populate the array with the child ammo gameobjects")]
    #endregion
    [SerializeField] Projectile[] projectileArray;

    Vector3[] cachedLocalPositions;
    Vector3[] cachedLocalRotations;
    Vector3[] cachedLocalScales;

    [HideInInspector] public BoomerangPhase boomerangPhase = BoomerangPhase.Aim;
    [HideInInspector] public ShirukenPhase shirukenPhase = ShirukenPhase.Fire;

    float projectileRange;
    float projectileSpeed;
    float projectileRotationSpeed;
    Vector3 fireDirectionVector;
    float fireDirectionAngle;
    ProjectileDetailsSO projectileDetails;
    float projectileChargeTimer;
    Vector3 velocity;

    public GameObject GetGameObject()
    {
        return gameObject;
    }

    private void Awake()
    {
        // Cache initial local transforms of all projectiles
        cachedLocalPositions = new Vector3[projectileArray.Length];
        cachedLocalRotations = new Vector3[projectileArray.Length];
        cachedLocalScales = new Vector3[projectileArray.Length];

        for (int i = 0; i < projectileArray.Length; i++)
        {
            cachedLocalPositions[i] = projectileArray[i].transform.localPosition;
            cachedLocalRotations[i] = projectileArray[i].transform.localEulerAngles;
            cachedLocalScales[i] = projectileArray[i].transform.localScale;
        }
    }


    // FOR PROJECTILE
    public void InitializeProjectile(float aimAngle, float weaponAimAngle, Vector3 weaponAimDirectionVector, float projectileSpeed, ProjectileKind projectileKind, ProjectileDetailsSO projectileDetails, 
        AttackContext attackContext, bool overrideProjectileMovement, bool fallingFromSkies, int projectileCounter, int projectilePerShot, int projectileIndex, uint ownerNetId, uint targetNetId, Enemy ownerEnemyForSp)
    {
        if (NetworkServer.active || NetworkClient.active) projectileDetails = WartheonDatabase.Instance.GetProjectile(projectileIndex);

        this.projectileDetails = projectileDetails;
        this.projectileSpeed = projectileSpeed;

        // Set fire direction
        SetFireDirection(projectileDetails, aimAngle, weaponAimAngle, weaponAimDirectionVector);

        // Set projectile range
        projectileRange = projectileDetails.projectileRange;

        // Activate projectile pattern gameobject
        gameObject.SetActive(true);

        // Loop through all child projectiles and initialize it
        for (int i = 0; i < projectileArray.Length; i++)
        {
            // Reset transform
            projectileArray[i].transform.localPosition = cachedLocalPositions[i];
            projectileArray[i].transform.localEulerAngles = cachedLocalRotations[i];
            projectileArray[i].transform.localScale = cachedLocalScales[i];

            projectileArray[i].ResetProjectileState();

            projectileArray[i].InitializeProjectile(aimAngle, weaponAimAngle, weaponAimDirectionVector, projectileSpeed, projectileKind, projectileDetails, attackContext, overrideProjectileMovement: true, fallingFromSkies,
                projectileCounter, projectilePerShot, projectileIndex, ownerNetId, targetNetId, ownerEnemyForSp);
        }

        // Set projectile charge timer - this will hold the projectile briefly
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
        if (projectileDetails == null) return;

        // Projectile charge effect
        if (projectileChargeTimer > 0f)
        {
            projectileChargeTimer -= Time.deltaTime;
            return;
        }

        // Calculate distance vector to move projectile
        Vector3 distanceVector = fireDirectionVector * projectileSpeed * Time.deltaTime;

        transform.position += distanceVector;

        // Rotate projectile for projectiles and active items
        transform.Rotate(new Vector3(0f, 0f, projectileDetails.projectileRotationSpeed * Time.deltaTime));

        // Disable after max range reached
        projectileRange -= distanceVector.magnitude;

        if (projectileRange < 0f)
        {
            StartCoroutine(DisableProcess());
        }
    }

    // This is for bouncing projectiles
    private void OnCollisionEnter2D(Collision2D collision)
    {
        // BOUNCING PROCESS
        if (projectileDetails != null && projectileDetails.isBouncing)
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
    /// Disable the projectile - thus returning it to the object pool
    /// </summary>
    IEnumerator DisableProcess()
    {
        yield return new WaitForSeconds(0.3f);

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
