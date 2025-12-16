using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class PlayerStats : MonoBehaviour
{
    [Header("Stats")]
    public float maxHealth = 100;
    public float maxStamina = 100;
    public float maxHunger = 100;
    public float maxHydration = 100;

    public float CurrentStamina { get; private set; }
    public float CurrentHealth { get; private set; }
    public float CurrentHunger { get; private set; }
    public float CurrentHydration { get; private set; }

    [Header("UI References")]
    public Slider healthSlider;
    public Slider staminaSlider;
    public Slider hungerSlider;
    public Slider hydrationSlider;

    [Header("Survival Durations (in game days)")]
    public float hungerDurationDays = 2f;         // How long player survive without food
    public float hydrationDurationDays = 1f;      // How long player survive without water
    public float hydrationNoonMultiplier = 1.5f;  // Faster water drain at noon
    public float noonStartHour = 11f;             // Start of hotter/noon period
    public float noonEndHour = 15f;               // End of hotter/noon period

    [Header("Health Drain")]
    public float healthDrainWhenStarvingOrDehydrated = 1f; // Health lost when hunger/hydration = 0

    [Header("Time Reference")]
    public DayNightCycle timeSystem;               

    [Header("Health Regen Potion")]
    public float regenTickInterval = 0.1f;        // How often regen ticks while healing over time

    [Header("Death")]
    public GameObject deathScreenUI;
    bool isDead = false;

    Coroutine healthRegenRoutine;

    [System.Obsolete]
    void Start()
    {
        // Start with full stats
        CurrentHealth = maxHealth;
        CurrentStamina = maxStamina;
        CurrentHunger = maxHunger;
        CurrentHydration = maxHydration;

        // Hook stats to UI sliders
        if (healthSlider) healthSlider.maxValue = maxHealth;
        if (staminaSlider) staminaSlider.maxValue = maxStamina;
        if (hungerSlider) hungerSlider.maxValue = maxHunger;
        if (hydrationSlider) hydrationSlider.maxValue = maxHydration;

        // Auto-find time system if not set
        if (timeSystem == null)
            timeSystem = FindObjectOfType<DayNightCycle>();

        UpdateUI();
    }

    [System.Obsolete]
    void Update()
    {
        // Survival systems updated every frame
        HandleHunger();
        HandleHydration();
        HandleHealthFromStarvationAndThirst();
        UpdateUI();
        CheckDeath();
    }

    // Uses stamina over time  
    public void DrainStamina(float amount)
    {
        CurrentStamina -= amount * Time.deltaTime;
        if (CurrentStamina < 0) CurrentStamina = 0;
    }

    // Regenerates stamina over time
    public void RegenStamina(float amount)
    {
        CurrentStamina += amount * Time.deltaTime;
        if (CurrentStamina > maxStamina) CurrentStamina = maxStamina;
    }

    void HandleHunger()
    {
        // Hunger drains based on game day length
        if (timeSystem != null && hungerDurationDays > 0f)
        {
            float secondsPerGameDay = timeSystem.SecondsPerGameDay;
            float drainPerSecond = maxHunger / (hungerDurationDays * secondsPerGameDay);
            CurrentHunger -= drainPerSecond * Time.deltaTime;
        }

        if (CurrentHunger < 0) CurrentHunger = 0;
    }

    // Eating food restores hunger
    public void RestoreHunger(float amount)
    {
        CurrentHunger += amount;
        if (CurrentHunger > maxHunger) CurrentHunger = maxHunger;
    }

    void HandleHydration()
    {
        // Hydration drains based on game day length and time of day
        if (timeSystem != null && hydrationDurationDays > 0f)
        {
            float secondsPerGameDay = timeSystem.SecondsPerGameDay;
            float baseDrainPerSecond = maxHydration / (hydrationDurationDays * secondsPerGameDay);

            float hour = timeSystem.CurrentHour;
            float noonMultiplier = 1f;

            // Extra drain during hottest hours
            if (hour >= noonStartHour && hour <= noonEndHour)
                noonMultiplier = hydrationNoonMultiplier;

            CurrentHydration -= baseDrainPerSecond * noonMultiplier * Time.deltaTime;
        }

        if (CurrentHydration < 0) CurrentHydration = 0;
    }

    // Drinking restores hydration
    public void DrinkWater(float amount)
    {
        CurrentHydration += amount;
        if (CurrentHydration > maxHydration) CurrentHydration = maxHydration;
    }

    // Instant heal
    public void Heal(float amount)
    {
        CurrentHealth += amount;
        if (CurrentHealth > maxHealth) CurrentHealth = maxHealth;
    }

    // Kills the player immediately
    public void Kill()
    {
        CurrentHealth = 0;
    }

    // Direct damage taken 
    public void TakeDamage(float amount)
    {
        CurrentHealth -= amount;
        if (CurrentHealth < 0) CurrentHealth = 0;
    }

    // If starving or dehydrated, slowly lose health
    void HandleHealthFromStarvationAndThirst()
    {
        if (CurrentHunger <= 0f || CurrentHydration <= 0f)
        {
            CurrentHealth -= healthDrainWhenStarvingOrDehydrated * Time.deltaTime;
            if (CurrentHealth < 0) CurrentHealth = 0;
        }
    }

    // Push the latest stat values into UI
    void UpdateUI()
    {
        if (healthSlider) healthSlider.value = CurrentHealth;
        if (staminaSlider) staminaSlider.value = CurrentStamina;
        if (hungerSlider) hungerSlider.value = CurrentHunger;
        if (hydrationSlider) hydrationSlider.value = CurrentHydration;
    }

    // Check if the player should die this frame
    [System.Obsolete]
    void CheckDeath()
    {
        if (!isDead && CurrentHealth <= 0f)
        {
            isDead = true;
            CurrentHealth = 0f;

            if (deathScreenUI != null)
                deathScreenUI.SetActive(true);

            MusicManager m = FindObjectOfType<MusicManager>();
            if (m != null)
                m.PlayDeathMusic();

            // Pause game and free the mouse
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
            Time.timeScale = 0f;
        }
    }

    // Reloads the current scene and resets time/mouse
    public void RestartGame()
    {
        Time.timeScale = 1f;
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        Scene scene = SceneManager.GetActiveScene();
        SceneManager.LoadScene(scene.name);
    }

    // Loads the main menu scene
    public void LoadMainMenu()
    {
        Time.timeScale = 1f;
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        SceneManager.LoadScene("MainMenu");
    }

    // Start a healing over time effect
    public void StartHealthRegen(float totalAmount, float duration)
    {
        if (healthRegenRoutine != null)
            StopCoroutine(healthRegenRoutine);

        healthRegenRoutine = StartCoroutine(HealthRegenCoroutine(totalAmount, duration));
    }

    IEnumerator HealthRegenCoroutine(float totalAmount, float duration)
    {
        // Instant heal if duration is 0 or less
        if (duration <= 0f)
        {
            Heal(totalAmount);
            healthRegenRoutine = null;
            yield break;
        }

        // Break regen into small ticks over time
        float tick = regenTickInterval <= 0f ? 0.1f : regenTickInterval;
        int steps = Mathf.Max(1, Mathf.RoundToInt(duration / tick));
        float amountPerStep = totalAmount / steps;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            Heal(amountPerStep);
            elapsed += tick;
            yield return new WaitForSeconds(tick);
        }

        healthRegenRoutine = null;
    }
}
