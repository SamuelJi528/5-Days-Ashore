using UnityEngine;

[ExecuteAlways]
[DisallowMultipleComponent]
[RequireComponent(typeof(Camera))]
public class IsoFollowCamera : MonoBehaviour
{
    [Header("Target")]
    public Transform target;                 // If null we'll find tag=Player on Awake

    [Header("View (Don't Starve style)")]
    [Range(20f, 70f)] public float pitch = 45f;     // tilt down toward ground
    [Range(0f, 360f)] public float yaw = 45f;       // fixed isometric yaw
    [Min(0.5f)] public float distance = 12f;        // boom length
    public Vector3 pivotOffset = new Vector3(0f, 1.4f, 0f); // where we look on the target

    [Header("Camera")]
    [Range(15f, 60f)] public float fieldOfView = 30f;  // narrow FOV = subtle perspective
    [Range(0f, 0.5f)] public float damping = 0.12f;    // smoothing time (seconds)

    [Header("Collision")]
    public LayerMask clipMask = ~0;
    [Min(0f)] public float collisionRadius = 0.25f;
    [Min(0f)] public float collisionPadding = 0.08f;   // keep camera off the wall a bit
    [Min(0.3f)] public float minDistance = 1.2f;       // never get closer than this

    Camera _cam;
    Vector3 _vel; // for SmoothDamp

    void Awake()
    {
        _cam = GetComponent<Camera>();
        ApplyFov();

        if (!target)
        {
            var p = GameObject.FindWithTag("Player");
            if (p) target = p.transform;
        }
    }

#if UNITY_EDITOR
    void OnValidate()
    {
        if (!_cam) _cam = GetComponent<Camera>();
        ApplyFov();
        pitch = Mathf.Clamp(pitch, 20f, 70f);
    }
#endif

    void LateUpdate()
    {
        if (!target) return;

        Vector3 pivot = GetPivot(target) + pivotOffset;

        // Ideal orbit position at fixed isometric angle
        Quaternion rot = Quaternion.Euler(pitch, yaw, 0f);
        Vector3 boomDir = rot * Vector3.back;                 // points from pivot to camera
        Vector3 idealPos = pivot + boomDir * distance;

        // Collision: shorten boom if obstructed
        Vector3 toCam = idealPos - pivot;
        float rayLen = toCam.magnitude;
        Vector3 dir = rayLen > 1e-4f ? toCam / rayLen : boomDir;

        if (Physics.SphereCast(pivot, collisionRadius, dir, out RaycastHit hit, rayLen, clipMask, QueryTriggerInteraction.Ignore))
        {
            float d = Mathf.Max(minDistance, hit.distance - collisionPadding);
            idealPos = pivot + dir * d;
        }

        // Smooth follow for position; locked look-at for that “tabletop” feel
        transform.position = Vector3.SmoothDamp(transform.position, idealPos, ref _vel, damping);
        transform.rotation = Quaternion.LookRotation(pivot - transform.position, Vector3.up);
    }

   Vector3 GetPivot(Transform t)
{
    var anchor = t.Find("CameraAnchor");
    if (anchor) return anchor.position;

    var cc = t.GetComponent<CharacterController>();
    if (cc) return t.position + Vector3.up * Mathf.Max(1f, cc.height * 0.5f);

    var rend = t.GetComponentInChildren<Renderer>();
    if (rend) return rend.bounds.center;

    return t.position;
}


    void ApplyFov()
    {
        if (_cam)
        {
            _cam.orthographic = false;  // DS uses perspective
            _cam.fieldOfView = fieldOfView;
        }
    }

    // Optional nice editor gizmo so you can see the boom in Scene view
    void OnDrawGizmosSelected()
    {
        if (!target) return;
        Vector3 pivot = GetPivot(target) + pivotOffset;
        Quaternion rot = Quaternion.Euler(pitch, yaw, 0f);
        Vector3 boomDir = rot * Vector3.back;
        Vector3 idealPos = pivot + boomDir * distance;

        Gizmos.color = Color.white;
        Gizmos.DrawLine(pivot, idealPos);
        Gizmos.DrawWireSphere(idealPos, collisionRadius);
    }
}
