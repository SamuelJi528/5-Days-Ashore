using UnityEngine;
using TMPro;

public class DayNightCycle : MonoBehaviour
{
    public float dayDuration = 300f;
    public float nightDuration = 300f;

    public Light sun;
    public TextMeshProUGUI clockText;
    public Gradient sunColorOverTime;

    public float maxIntensity = 1.3f;
    public float nightIntensity = 0.15f;
    public int startDay = 1;
    public MusicManager musicManager;

    public float dayAmbientIntensity = 1.5f;
    public float nightAmbientIntensity = 0.4f;
    public float dayReflectionIntensity = 1f;
    public float nightReflectionIntensity = 0.5f;

    public int winDay = 5;
    public float winHour = 5f;
    public GameObject winPanel;

    float cycleTime;
    float totalCycleLength;
    int currentDay;
    bool lastIsDay;
    bool hasWon;

    public float SecondsPerGameDay => totalCycleLength;
    public float CurrentHour { get; private set; }
    public bool IsNight { get; private set; }

    void Start()
    {
        // Figure out how long a full day-night cycle is
        totalCycleLength = dayDuration + nightDuration;

        // Set the starting day
        currentDay = startDay;

        // Start time around early morning (5 AM)
        cycleTime = totalCycleLength * (5f / 24f);

        // Convert cycle time into a 24-hour time value
        float t = cycleTime / totalCycleLength;
        float hours24 = t * 24f;
        CurrentHour = hours24;

        // Check if it's day or night at the start
        bool isDay = hours24 >= 5f && hours24 < 18f;
        IsNight = !isDay;
        lastIsDay = isDay;

        // Tell the music system whether it's day or night
        if (musicManager != null)
            musicManager.SetDayState(isDay);
    }

    [System.Obsolete]
    void Update()
    {
        // Stop updating after the player wins
        if (hasWon) return;

        // Move the time forward
        cycleTime += Time.deltaTime;

        // If the day finished, move to the next one
        if (cycleTime > totalCycleLength)
        {
            cycleTime -= totalCycleLength;
            currentDay++;
        }

        float t = cycleTime / totalCycleLength;

        // Update lighting, the clock text, and check for win
        UpdateSunAndAmbient(t);
        UpdateClock(t);
        CheckWinCondition();
    }

    void UpdateSunAndAmbient(float t)
    {
        float dayCurve = Mathf.Clamp01(-4f * (t - 0.5f) * (t - 0.5f) + 1f);

        // the sun’s rotation, color, and brightness
        if (sun != null)
        {
            float azimuth = t * 360f;   
            float elevation = Mathf.Lerp(-20f, 80f, dayCurve); 

            sun.transform.rotation = Quaternion.Euler(elevation, azimuth, 0f);

            if (sunColorOverTime != null)
                sun.color = sunColorOverTime.Evaluate(dayCurve);

            sun.intensity = Mathf.Lerp(nightIntensity, maxIntensity, dayCurve);
        }

        // Change ambient and reflection lighting 
        RenderSettings.ambientIntensity =
            Mathf.Lerp(nightAmbientIntensity, dayAmbientIntensity, dayCurve);

        RenderSettings.reflectionIntensity =
            Mathf.Lerp(nightReflectionIntensity, dayReflectionIntensity, dayCurve);
    }

    void UpdateClock(float t)
    {
        // Convert cycle progress into a 24-hour time
        float hours24 = t * 24f;
        CurrentHour = hours24;

        int hour = Mathf.FloorToInt(hours24);
        float minuteFloat = (hours24 - hour) * 60f;
        int minute = Mathf.FloorToInt(minuteFloat);

        // Update the clock text on screen
        if (clockText != null)
            clockText.text = $"Day {currentDay}  {hour:00}:{minute:00}";

        // Determine if it's day or night
        bool isDay = hours24 >= 5f && hours24 < 18f;
        IsNight = !isDay;

        // If we switched day ↔ night, update the music
        if (isDay != lastIsDay)
        {
            lastIsDay = isDay;

            if (musicManager != null)
                musicManager.SetDayState(isDay);
        }
    }

    [System.Obsolete]
    void CheckWinCondition()
    {
        // Make sure we only win once
        if (hasWon) return;

        // Winning happens after reaching a certain day and hour
        if (currentDay >= winDay && CurrentHour >= winHour)
        {
            hasWon = true;

            // Show the win screen
            if (winPanel != null)
                winPanel.SetActive(true);

            // Play win music
            MusicManager m = FindObjectOfType<MusicManager>();
            if (m != null)
                m.PlayWinMusic();

            // Unlock and show the mouse
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;

            // Pause the game
            Time.timeScale = 0f;
        }
    }
}
