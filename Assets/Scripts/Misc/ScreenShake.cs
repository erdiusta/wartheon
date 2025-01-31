using UnityEngine;
using Cinemachine;
using System;

public class ScreenShake : MonoBehaviour
{
    CinemachineVirtualCamera virtualCamera;
    CinemachineBasicMultiChannelPerlin noise;

    private void Awake()
    {
        virtualCamera = GetComponent<CinemachineVirtualCamera>();

        noise = virtualCamera.GetComponentInChildren<CinemachineBasicMultiChannelPerlin>();

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
            noise.m_AmplitudeGain = intensity;
            Invoke("StopShaking", duration);
        }
    }

    private void StopShaking()
    {
        if (noise != null)
        {
            noise.m_AmplitudeGain = 0f;
        }
    }
}
