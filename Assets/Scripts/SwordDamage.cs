using UnityEngine;

public class SwordDamage : MonoBehaviour
{
    public float damage = 25f;          // How much damage the sword deals
    public string enemyTag = "Enemy";   // Only objects with this tag will take damage

    void OnTriggerEnter(Collider other)
    {
        // Ignore anything that isn’t tagged as an enemy
        if (!other.CompareTag(enemyTag)) return;

        // Try to find the enemy health script
        EnemyHealth enemy = other.GetComponentInParent<EnemyHealth>();
        if (enemy != null)
            enemy.TakeDamage(damage);
    }
}
