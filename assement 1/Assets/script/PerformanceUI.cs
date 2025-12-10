using UnityEngine;
using TMPro;

public class PerformanceUI : MonoBehaviour
{
    public TextMeshProUGUI performanceText;

    public float timeElapsed;
    public float distanceTravelled;
    public float currentSpeed;
    public int parcelCount;

    void Update()
    {
        if (performanceText != null)
        {
            performanceText.text =
                "Time: " + timeElapsed.ToString("F2") + " s\n" +
                "Distance: " + distanceTravelled.ToString("F2") + " m\n" +
                "Speed: " + currentSpeed.ToString("F2") + " m/s\n" +
                "Parcels: " + parcelCount;
        }
    }
}
