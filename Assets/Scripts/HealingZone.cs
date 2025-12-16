using UnityEngine;

public class HealingZone : MonoBehaviour
{
    public float healPerSecond = 5f;
    public DayNightCycle timeSystem;

    [System.Obsolete]
    void Start()
    {
        if (timeSystem == null)
            timeSystem = FindObjectOfType<DayNightCycle>();
    }

    void OnTriggerStay(Collider other)
    {
        PlayerStats stats = other.GetComponent<PlayerStats>();
        if (stats == null) return;

        if (timeSystem == null) return;
        // only avaliable at this time
        float hour = timeSystem.CurrentHour;
        bool isNight = hour < 5f || hour >= 18f;
        if (!isNight) return;

        stats.Heal(healPerSecond * Time.deltaTime);
    }
}
