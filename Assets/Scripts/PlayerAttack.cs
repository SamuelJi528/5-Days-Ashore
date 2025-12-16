using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerAttack : MonoBehaviour
{
    public InventoryManager inventory;   
    public ItemData sword;              
    public Animator animator;            

    public float attackRadius = 1.5f;    // How far the attack can reach
    public float attackDamage = 25f;     // How much damage each attack deals
    public float attackCooldown = 0.9f;  // Time between attacks

    bool isAttacking;
    float nextAttackTime;

    void Update()
    {
        // Press F to attack
        if (Keyboard.current.fKey.wasPressedThisFrame)
            TryAttack();
    }

    void TryAttack()
    {
        // Make sure the attack is allowed
        if (Time.time < nextAttackTime) return;
        if (isAttacking) return;
        if (inventory == null) return;

        // Check if the current hotbar slot actually has the sword
        int index = inventory.currentHotbarIndex;
        if (index < 0 || index >= inventory.inventorySlots.Count) return;

        InventorySlot slot = inventory.inventorySlots[index];
        if (slot == null || slot.item != sword) return;

        // Start cooldown
        nextAttackTime = Time.time + attackCooldown;
        isAttacking = true;

        // Play attack animation
        if (animator != null)
            animator.SetTrigger("Attack");

        // Apply damage 
        Hit();
    }

    public void Hit()
    {
        // Check everything within the hit radius
        Vector3 center = transform.position;

        Collider[] hits = Physics.OverlapSphere(center, attackRadius);
        for (int i = 0; i < hits.Length; i++)
        {
            // Find an EnemyHealth script on whatever got hit
            EnemyHealth eh = hits[i].GetComponentInParent<EnemyHealth>();
            if (eh != null)
                eh.TakeDamage(attackDamage);
        }

        isAttacking = false;
    }

    void OnDrawGizmosSelected()
    {
        // Show the attack radius in the editor
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRadius);
    }
}
