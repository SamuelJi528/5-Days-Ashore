using UnityEngine;

public class SwordHitbox : MonoBehaviour
{
    public float damage = 25f;       // How much damage the sword deals
    public bool active;              // Only deals damage when this is true 
    public LayerMask hitLayers = ~0; // Which layers the sword is allowed to hit

    void OnTriggerEnter(Collider other)
    {
        // Only deal damage if the sword’s hitbox is currently active
        if (!active) return;

        // Check if the object’s layer is one we are allowed to hit
        if (((1 << other.gameObject.layer) & hitLayers) == 0)
            return;

        // Try to find an enemy to damage
        EnemyHealth eh = other.GetComponentInParent<EnemyHealth>();
        if (eh != null)
            eh.TakeDamage(damage);
    }
}
