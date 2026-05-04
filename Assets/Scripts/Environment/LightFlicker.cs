using UnityEngine;
using UnityEngine.Rendering.Universal; // keep this namespace in for when 2D lights become non experimental;


[DisallowMultipleComponent]
public class LightFlicker : MonoBehaviour
{
    Light2D light2D;
    float lightFlickerTimer;

    [SerializeField] float lightIntensityMin;
    [SerializeField] float lightIntensityMax;
    [SerializeField] float lightFlickerTimeMin;
    [SerializeField] float lightFlickerTimeMax;

    private void Awake()
    {
        light2D = GetComponentInChildren<Light2D>();
    }

    private void Start()
    {
        lightFlickerTimer = Random.Range(lightFlickerTimeMin, lightFlickerTimeMax);
    }

    private void Update()
    {
        if (light2D == null) return;

        lightFlickerTimer -= Time.deltaTime;

        if (lightFlickerTimer < 0f)
        {
            lightFlickerTimer = Random.Range(lightFlickerTimeMin, lightFlickerTimeMax);
            RandomizeLightIntensity();
        }
    }

    private void RandomizeLightIntensity()
    {
        light2D.intensity = Random.Range(lightIntensityMin, lightIntensityMax);
    }

    #region Validation
#if UNITY_EDITOR
    private void OnValidate()
    {
        HelperUtilities.ValidateCheckPositiveRange(this, nameof(lightIntensityMin), lightIntensityMin, nameof(lightIntensityMax), 
            lightIntensityMax, false);
        HelperUtilities.ValidateCheckPositiveRange(this, nameof(lightFlickerTimeMin), lightFlickerTimeMin, nameof(lightFlickerTimeMax), 
            lightFlickerTimeMax, false);
    }
#endif
    #endregion
}
