using UnityEngine;
using UnityEngine.UI;

public class EnemyHealth : MonoBehaviour
{
    public float maxHealth = 100f;
    public Slider healthBar;
    public ItemData lootItem;
    public int lootAmount = 1;

    float currentHealth;
    bool dead;
    InventoryManager inventory;
    EnemySpawner spawner;
    Animator animator;

    [System.Obsolete]
    void Start()
    {
        // Set starting health
        currentHealth = maxHealth;

        if (healthBar != null)
        {
            healthBar.maxValue = maxHealth;
            healthBar.value = currentHealth;
        }

        // Find needed systems in the scene
        inventory = FindObjectOfType<InventoryManager>();
        spawner = FindObjectOfType<EnemySpawner>();
        animator = GetComponent<Animator>();
    }

    public void TakeDamage(float amount)
    {
        // Ignore damage if already dead
        if (dead) return;

        // Reduce health
        currentHealth -= amount;
        if (currentHealth < 0) currentHealth = 0;

        if (healthBar != null)
            healthBar.value = currentHealth;

        // If health hits zero kill the enemy
        if (currentHealth <= 0)
            Die();
    }

    void Die()
    {
        dead = true;

        // Play death animation
        if (animator != null)
            animator.SetTrigger("Die");

        // Give loot to the player
        if (inventory != null && lootItem != null && lootAmount > 0)
            inventory.AddItem(lootItem, lootAmount);

        // Notify the spawner so it can respawn enemies later
        if (spawner != null)
            spawner.NotifyEnemyDied(gameObject);

        Destroy(gameObject, 2f);
    }
}
