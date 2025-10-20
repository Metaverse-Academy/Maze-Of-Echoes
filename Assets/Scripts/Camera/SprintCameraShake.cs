using UnityEngine;
using UnityEngine.InputSystem;

[DisallowMultipleComponent]
public class SprintCameraShake : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Transform cam;            // Leave empty to use this transform
    [SerializeField] private PlayerInput playerInput;  // Your PlayerInput on the player
    [SerializeField] private string sprintActionName = "Sprint";

    [Header("Shake While Sprinting")]
    [Tooltip("Position shake amplitude in local units (meters).")]
    [SerializeField] private Vector3 posAmplitude = new Vector3(0.015f, 0.02f, 0.0f);
    [Tooltip("Rotation shake amplitude in degrees around local axes (x=pitch, y=yaw, z=roll).")]
    [SerializeField] private Vector3 rotAmplitude = new Vector3(1.2f, 0.4f, 0.7f);
    [Tooltip("How fast the noise wiggles.")]
    [SerializeField] private float frequency = 12f;
    [Tooltip("Seconds to fade the shake in when sprint starts.")]
    [SerializeField] private float blendIn = 0.15f;
    [Tooltip("Seconds to fade the shake out when sprint stops.")]
    [SerializeField] private float blendOut = 0.2f;

    [Header("Extras")]
    [Tooltip("If your sprint is a toggle instead of a hold, call SetSprinting(true/false) from your movement script.")]
    [SerializeField] private bool useManualSprintFlag = false;

    float intensity;            // 0..1 blend
    bool isSprinting;           // when using manual flag
    Vector3 baseLocalPos;
    Quaternion baseLocalRot;

    InputAction sprintAction;
    float t;                    // time accumulator
    Vector3 noiseSeed;          // desync axes

    void Reset()
    {
        cam = transform;
    }

    void Awake()
    {
        if (cam == null) cam = transform;
        baseLocalPos = cam.localPosition;
        baseLocalRot = cam.localRotation;
        noiseSeed = new Vector3(Random.value * 10f, Random.value * 10f, Random.value * 10f);
    }

    void OnEnable()
    {
        if (!useManualSprintFlag && playerInput != null && !string.IsNullOrEmpty(sprintActionName))
        {
            sprintAction = playerInput.actions?[sprintActionName];
        }
    }

    void OnDisable()
    {
        // snap back cleanly
        cam.localPosition = baseLocalPos;
        cam.localRotation = baseLocalRot;
    }

    void Update()
    {
        // Determine sprinting state
        bool sprintPressed = useManualSprintFlag ? isSprinting
                          : sprintAction != null && sprintAction.IsPressed();

        // Smoothly blend intensity
        float target = sprintPressed ? 1f : 0f;
        float blendSpeed = 1f / Mathf.Max(0.0001f, sprintPressed ? blendIn : blendOut);
        intensity = Mathf.MoveTowards(intensity, target, blendSpeed * Time.deltaTime);

        // Advance time only when shaking (keeps noise stable when stopped)
        if (intensity > 0f) t += Time.deltaTime * frequency;

        // Generate Perlin-based offsets (stable, non-drifting)
        Vector3 posOff = new Vector3(
            (Mathf.PerlinNoise(noiseSeed.x, t) - 0.5f) * 2f * posAmplitude.x,
            (Mathf.PerlinNoise(noiseSeed.y, t) - 0.5f) * 2f * posAmplitude.y,
            (Mathf.PerlinNoise(noiseSeed.z, t) - 0.5f) * 2f * posAmplitude.z
        ) * intensity;

        Vector3 rotOff = new Vector3(
            (Mathf.PerlinNoise(noiseSeed.x + 20f, t) - 0.5f) * 2f * rotAmplitude.x,
            (Mathf.PerlinNoise(noiseSeed.y + 20f, t) - 0.5f) * 2f * rotAmplitude.y,
            (Mathf.PerlinNoise(noiseSeed.z + 20f, t) - 0.5f) * 2f * rotAmplitude.z
        ) * intensity;

        // Apply without accumulating drift
        cam.localPosition = baseLocalPos + posOff;
        cam.localRotation = baseLocalRot * Quaternion.Euler(rotOff);
    }

    /// <summary>
    /// Call this if your sprint is toggled in code instead of a hold Action.
    /// Example: cameraShake.SetSprinting(true) when sprint starts, false when it ends.
    /// </summary>
    public void SetSprinting(bool sprinting)
    {
        isSprinting = sprinting;
    }
}
