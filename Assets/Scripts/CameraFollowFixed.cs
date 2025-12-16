using UnityEngine;

[ExecuteAlways]
[DisallowMultipleComponent]
[RequireComponent(typeof(Camera))]
public class IsoFollowCamera : MonoBehaviour
{
    [Header("Target")]
    public Transform target;

    [Header("View")]
    [Range(20f, 70f)] public float pitch = 45f;
    [Range(0f, 360f)] public float yaw = 45f;
    [Min(0.5f)] public float distance = 12f;
    public Vector3 pivotOffset = new Vector3(0f, 1.4f, 0f);

    [Header("Camera")]
    [Range(15f, 60f)] public float fieldOfView = 30f;
    [Range(0f, 0.5f)] public float damping = 0.12f;

    [Header("Collision")]
    public LayerMask clipMask = ~0;
    [Min(0f)] public float collisionRadius = 0.25f;
    [Min(0f)] public float collisionPadding = 0.08f;
    [Min(0.3f)] public float minDistance = 1.2f;

    [Header("Rotate View")]
    public KeyCode rotateKey = KeyCode.T;
    public float rotateStep = 180f;

    Camera _cam;
    Vector3 _vel;

    void Awake()
    {
        // Get the camera on this object
        _cam = GetComponent<Camera>();
        ApplyFov();

        // If no target is set, try to find the player
        if (!target)
        {
            var p = GameObject.FindWithTag("Player");
            if (p) target = p.transform;
        }
    }

#if UNITY_EDITOR
    void OnValidate()
    {
        // Update camera settings while editing in Unity
        if (!_cam) _cam = GetComponent<Camera>();
        ApplyFov();
        pitch = Mathf.Clamp(pitch, 20f, 70f);
    }
#endif

    void LateUpdate()
    {
        if (!target) return;

        // Rotate the view when the key is pressed
        if (Application.isPlaying && Input.GetKeyDown(rotateKey))
            yaw = Mathf.Repeat(yaw + rotateStep, 360f);

        // Find the point the camera should look at
        Vector3 pivot = GetPivot(target) + pivotOffset;

        // Figure out where the camera wants to be
        Quaternion rot = Quaternion.Euler(pitch, yaw, 0f);
        Vector3 boomDir = rot * Vector3.back;
        Vector3 idealPos = pivot + boomDir * distance;

        // Check if something blocks the camera
        Vector3 toCam = idealPos - pivot;
        float rayLen = toCam.magnitude;
        Vector3 dir = rayLen > 0f ? toCam / rayLen : boomDir;

        if (Physics.SphereCast(pivot, collisionRadius, dir, out RaycastHit hit, rayLen, clipMask, QueryTriggerInteraction.Ignore))
        {
            // Move the camera closer so it doesn't clip through walls
            float d = Mathf.Max(minDistance, hit.distance - collisionPadding);
            idealPos = pivot + dir * d;
        }

        // Smoothly move the camera toward the final spot
        transform.position = Vector3.SmoothDamp(transform.position, idealPos, ref _vel, damping);

        // Make the camera face the target point
        transform.rotation = Quaternion.LookRotation(pivot - transform.position, Vector3.up);
    }

    Vector3 GetPivot(Transform t)
    {
        // If there is a CameraAnchor child, use that
        var anchor = t.Find("CameraAnchor");
        if (anchor) return anchor.position;

        // If the object has a CharacterController, use its height
        var cc = t.GetComponent<CharacterController>();
        if (cc) return t.position + Vector3.up * Mathf.Max(1f, cc.height * 0.5f);

        // Otherwise try using the renderer's center
        var rend = t.GetComponentInChildren<Renderer>();
        if (rend) return rend.bounds.center;

        // Last fallback: just use the object's position
        return t.position;
    }

    void ApplyFov()
    {
        // Make sure the camera uses perspective mode and correct FOV
        if (_cam)
        {
            _cam.orthographic = false;
            _cam.fieldOfView = fieldOfView;
        }
    }

    void OnDrawGizmosSelected()
    {
        // Draw a line and sphere in the editor so we can see the camera's setup
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
