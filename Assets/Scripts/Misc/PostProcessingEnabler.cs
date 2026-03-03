using UnityEngine;
using UnityEngine.Rendering;

public class PostProcessingEnabler : SingletonMonobehaviour<PostProcessingEnabler>
{
    public bool isOn;

    Volume volume;

    protected override void Awake()
    {
        base.Awake();

        volume = GetComponent<Volume>();
    }

    private void Update()
    {
        volume.enabled = isOn ? true : false;
    }
}
