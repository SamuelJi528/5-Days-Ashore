using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(CharacterController))]
[RequireComponent(typeof(PlayerInput))]
public class PlayerControl : MonoBehaviour
{
    [Header("Input System")]
    public string actionMap = "Player";
    public string moveAction = "Move";
    public string sprintAction = "Sprint";   

    [Header("Move")]
    public float moveSpeed = 5f;
    public float sprintMultiplier = 1.6f;    
    public float turnSpeed = 540f;

    [Header("Stamina Settings")]
    public float staminaCost = 15f;
    public float staminaRegen = 10f; 

    [Header("Gravity")]
    public float gravity = -25f;
    public float groundStick = -2f;

    CharacterController cc;
    PlayerInput pi;
    InputAction aMove;
    InputAction aSprint;                      
    Animator anim;                            
    PlayerStats stats; 
    float vY;

    // Track if player ran out of breath
    private bool isExhausted = false; 

    void Awake()
    {
        cc = GetComponent<CharacterController>();
        pi = GetComponent<PlayerInput>();
        anim = GetComponentInChildren<Animator>(); 
        stats = GetComponent<PlayerStats>(); 

        // basic controller sanity
        cc.height = Mathf.Max(1.0f, cc.height <= 0 ? 1.8f : cc.height);
        cc.radius = Mathf.Max(0.2f, cc.radius <= 0 ? 0.4f : cc.radius);
        cc.center = new Vector3(0f, cc.height * 0.5f, 0f);
        cc.stepOffset = Mathf.Clamp(cc.height * 0.3f, 0.2f, 0.6f);
        cc.skinWidth = 0.06f;
        cc.minMoveDistance = 0f;
        cc.detectCollisions = true;

        // we drive movement via CharacterController, so make any rb kinematic
        var rb = GetComponent<Rigidbody>();
        if (rb) { rb.isKinematic = true; rb.useGravity = false; }

        var map = pi.actions.FindActionMap(actionMap, false);
        aMove = map != null ? map.FindAction(moveAction, false) : null;
        aSprint = map != null ? map.FindAction(sprintAction, false) : null; 
    }

    void Start()
    {
        // snap player to ground on start
        if (Physics.Raycast(transform.position + Vector3.up * 4f, Vector3.down, out var hit, 1000f, ~0, QueryTriggerInteraction.Ignore))
        {
            float bottomY = transform.position.y + cc.center.y - cc.height * 0.5f;
            float wantBottomY = hit.point.y + cc.skinWidth + 0.01f;
            transform.position += Vector3.up * (wantBottomY - bottomY);
            vY = groundStick;
        }
    }

    void Update()
    {
        // if inventory UI is open, skip movement input
        if (InventoryManager.IsInventoryOpen)
        {
            // keep anim in idle while UI is up
            if (anim)
            {
                anim.SetFloat("Speed", 0f);
                anim.SetBool("Sprint", false);
            }
            return; 
        }

        Vector2 m = aMove != null ? aMove.ReadValue<Vector2>() : Vector2.zero;

        Transform cam = Camera.main ? Camera.main.transform : transform;
        Vector3 f = Vector3.ProjectOnPlane(cam.forward, Vector3.up).normalized;
        Vector3 r = Vector3.ProjectOnPlane(cam.right,   Vector3.up).normalized;

        Vector3 moveDir = (r * m.x + f * m.y);
        if (moveDir.sqrMagnitude > 1f) moveDir.Normalize();

        bool isMoving = moveDir.sqrMagnitude > 0.0001f;
        bool sprintInput = (aSprint != null ? aSprint.IsPressed() : Input.GetKey(KeyCode.LeftShift));
        
        // Check Exhaustion State
        if (stats != null)
        {
            // If we hit 0 stamina player is exhausted
            if (stats.CurrentStamina <= 0) 
            {
                isExhausted = true;
            }
            
            // Stay exhausted until we recover at least 15 stamina
            if (isExhausted && stats.CurrentStamina > 15f) 
            {
                isExhausted = false;
            }
        }

        // Condition to check if player can sprint
        bool isSprinting = sprintInput && isMoving && !isExhausted && (stats != null);

        // Drain or Regen
        if (stats != null)
        {
            if (isSprinting)
                stats.DrainStamina(staminaCost);
            else
                stats.RegenStamina(staminaRegen);
        }

        // Gravity
        if (cc.isGrounded && vY < 0f) vY = groundStick;
        vY += gravity * Time.deltaTime;

        // Apply Speed
        float speed = moveSpeed * (isSprinting ? sprintMultiplier : 1f);
        Vector3 velocity = moveDir * speed;
        velocity.y = vY;
        cc.Move(velocity * Time.deltaTime);

        // Rotation
        if (isMoving)
        {
            Quaternion targetRot = Quaternion.LookRotation(moveDir, Vector3.up);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRot, Time.deltaTime * turnSpeed);
        }

        // Animations
        if (anim)
        {
            anim.SetFloat("Speed", Mathf.Clamp01(m.magnitude));            
            anim.SetBool("Sprint", isSprinting);   
        }
    }

    public void PlayInteractionAnim(string triggerName)
    {
        if (anim != null)
        {
            anim.ResetTrigger("Chop");
            anim.ResetTrigger("Gather"); 
            anim.SetTrigger(triggerName);
        }
    }
}
