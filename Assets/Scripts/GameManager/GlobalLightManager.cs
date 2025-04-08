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
        switch (GameManager.Instance.currentDungeonLevelListIndex)
        {
            case 0:
                light2D.intensity = 0.15f;
                break;
            case 1:
                light2D.intensity = 0.35f;
                break;
            case 2:
                light2D.intensity = 0.25f;
                break;
            case 3:
                light2D.intensity = 0.15f;
                break;
            case 4:
                light2D.intensity = 0.4f;
                break;
            case 5:
                light2D.intensity = 0.3f;
                break;
            case 6:
                light2D.intensity = 0.35f;
                break;
            case 7:
                light2D.intensity = 0.2f;
                break;
            default:
                break;
        }
    }
}
