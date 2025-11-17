using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(CharacterController))]
[RequireComponent(typeof(PlayerInput))]
public class PlayerControl : MonoBehaviour
{
    [Header("Input System (names must match your asset)")]
    public string actionMap = "Player";
    public string moveAction = "Move";
    public string sprintAction = "Sprint";   

    [Header("Move")]
    public float moveSpeed = 5f;
    public float sprintMultiplier = 1.6f;    // speed boost when sprinting
    public float turnSpeed = 540f;

    [Header("Gravity")]
    public float gravity = -25f;
    public float groundStick = -2f;

    CharacterController cc;
    PlayerInput pi;
    InputAction aMove;
    InputAction aSprint;                      // sprint input
    Animator anim;                            // drives animations
    float vY;

    void Awake()
    {
        cc = GetComponent<CharacterController>();
        pi = GetComponent<PlayerInput>();
        anim = GetComponentInChildren<Animator>(); 

        // controller setup
        cc.height = Mathf.Max(1.0f, cc.height <= 0 ? 1.8f : cc.height);
        cc.radius = Mathf.Max(0.2f, cc.radius <= 0 ? 0.4f : cc.radius);
        cc.center = new Vector3(0f, cc.height * 0.5f, 0f);
        cc.stepOffset = Mathf.Clamp(cc.height * 0.3f, 0.2f, 0.6f);
        cc.skinWidth = 0.06f;
        cc.minMoveDistance = 0f;
        cc.detectCollisions = true;

        var rb = GetComponent<Rigidbody>();
        if (rb) { rb.isKinematic = true; rb.useGravity = false; }

        // Input actions
        var map = pi.actions.FindActionMap(actionMap, false);
        aMove = map != null ? map.FindAction(moveAction, false) : null;
        aSprint = map != null ? map.FindAction(sprintAction, false) : null; 
    }

    void Start()
    {
        // start on ground
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
        // read move input
        Vector2 m = aMove != null ? aMove.ReadValue<Vector2>() : Vector2.zero;

        // camera direction
        Transform cam = Camera.main ? Camera.main.transform : transform;
        Vector3 f = Vector3.ProjectOnPlane(cam.forward, Vector3.up).normalized;
        Vector3 r = Vector3.ProjectOnPlane(cam.right,   Vector3.up).normalized;

        Vector3 moveDir = (r * m.x + f * m.y);
        if (moveDir.sqrMagnitude > 1f) moveDir.Normalize();

        // sprint
        bool isSprinting = (aSprint != null ? aSprint.IsPressed() : Input.GetKey(KeyCode.LeftShift))
                           && moveDir.sqrMagnitude > 0.0001f;

        // gravity
        if (cc.isGrounded && vY < 0f) vY = groundStick;
        vY += gravity * Time.deltaTime;

        // move 
        float speedThisFrame = moveSpeed * (isSprinting ? sprintMultiplier : 1f);
        Vector3 velocity = moveDir * speedThisFrame;
        velocity.y = vY;
        cc.Move(velocity * Time.deltaTime);

        // face move direction
        if (moveDir.sqrMagnitude > 0.0004f)
        {
            Quaternion targetRot = Quaternion.LookRotation(moveDir, Vector3.up);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRot, Time.deltaTime * turnSpeed);
        }

        // --- ANIMATION HOOKS ---
        float speed01 = Mathf.Clamp01(m.magnitude);
        if (anim)
        {
            anim.SetFloat("Speed", speed01);            // drives Idle Run
            anim.SetBool("IsSprinting", isSprinting);   // drives Run Sprint
        }
    }
}
