using UnityEngine;
using Unity.Cinemachine;
using System;

public class ScreenShake : MonoBehaviour
{
    CinemachineCamera virtualCamera;
    CinemachineBasicMultiChannelPerlin noise;

    private void Awake()
    {
        virtualCamera = GetComponent<CinemachineCamera>();

        noise = virtualCamera.GetComponent<CinemachineBasicMultiChannelPerlin>();

        // If still not found, log an error
        if (noise == null)
        {
            Debug.LogError("CinemachineBasicMultiChannelPerlin component not found on the CinemachineVirtualCamera.");
        }

        StopShaking();
    }

    private void OnEnable()
    {
        StaticEventHandler.OnCameraShaken += CallCameraShakeEvent_OnCameraShaken;
    }

    private void OnDisable()
    {
        StaticEventHandler.OnCameraShaken -= CallCameraShakeEvent_OnCameraShaken;
    }

    private void CallCameraShakeEvent_OnCameraShaken(CameraShakeArgs cameraShakeArgs)
    {
        ShakeScreen(cameraShakeArgs.shakeIntensity, cameraShakeArgs.shakeDuration);
    }

    public void ShakeScreen(float intensity, float duration)
    {
        if (noise != null)
        {
            noise.AmplitudeGain = intensity;
            Invoke("StopShaking", duration);
        }
    }

    private void StopShaking()
    {
        if (noise != null)
        {
            noise.AmplitudeGain = 0f;
        }
    }
}
