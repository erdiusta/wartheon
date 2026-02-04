using UnityEngine;
using UnityEngine.Rendering.Universal;

public class GlobalLightManager : SingletonMonobehaviour<GlobalLightManager>
{
    [SerializeField] Light2D light2D;

    protected override void Awake()
    {
        base.Awake();
    }

    private void Start()
    {
        light2D.intensity = 0.15f;
    }

    private void Update()
    {
        light2D.intensity = GetLightIntenstiy(GameManager.Instance.currentDungeonLevelListIndex);
    }

    private float GetLightIntenstiy(int index) => index switch
    {
        0 => 0.15f, 1 => 0.15f, 2 => 0.35f, 3 => 0.25f, 4 => 0.15f, 5 => 0.4f, 6 => 0.3f, 7 => 0.35f, 8 => 0.2f, _ => 0f
    }; 
}
