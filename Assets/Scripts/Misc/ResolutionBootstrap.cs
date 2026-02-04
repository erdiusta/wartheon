using UnityEngine;

[DefaultExecutionOrder(-1000)]
public class ResolutionBootstrap : MonoBehaviour
{
    void Awake()
    {
        Resolution[] resolutions = Screen.resolutions;

        int highestWidth = 0;
        int highestHeight = 0;
        int highestHz = 60;

        foreach (Resolution res in resolutions)
        {
            int totalPixels = res.width * res.height;
            if (totalPixels > highestWidth * highestHeight)
            {
                highestWidth = res.width;
                highestHeight = res.height;
                highestHz = (int)res.refreshRateRatio.value;
            }
        }

        // Create the RefreshRate struct directly
        RefreshRate refreshRate = new RefreshRate
        {
            numerator = (uint)highestHz,
            denominator = 1
        };

        Screen.SetResolution(highestWidth, highestHeight, FullScreenMode.FullScreenWindow, refreshRate);
    }
}
