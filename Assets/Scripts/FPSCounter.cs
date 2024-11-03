using UnityEngine;
using TMPro;
public class FPSCounter : MonoBehaviour
{
    float fps;
    [SerializeField] float updateFrequency = 0.2f;
    float updateTimer;

    [SerializeField] TextMeshProUGUI fpsTitle;

    private void UpdateFPSDisplay()
    {
        updateTimer -= Time.deltaTime;
        if (updateTimer <= 0f)
        {
            fps = 1f / Time.unscaledDeltaTime;
            fpsTitle.text = "FPS: " + Mathf.Round(fps);
            updateTimer = updateFrequency;
        }
    }

    void Start()
    {
        updateTimer = updateFrequency;
    }

    void Update()
    {
        UpdateFPSDisplay();
    }
}
