using UnityEngine;
using UnityEngine.Rendering;

public class PostProcessingEnabler : SingletonMonobehaviour<PostProcessingEnabler>
{
    public bool isOn;

    Volume volume;

    private void Start()
    {
        volume = GetComponent<Volume>();
    }

    private void Update()
    {
        volume.enabled = isOn ? true : false;
    }
}
