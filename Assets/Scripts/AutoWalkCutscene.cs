using UnityEngine;
using UnityEngine.Playables;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif
using StarterAssets;

public class AutoWalkCutscene : MonoBehaviour
{
    [Header("Who/Where")]
    [SerializeField] private ThirdPersonController playerTPC;      // your player (Starter Assets)
    [SerializeField] private StarterAssetsInputs inputs;           // on the player
#if ENABLE_INPUT_SYSTEM
    [SerializeField] private PlayerInput playerInput;              // on the player
#endif
    [SerializeField] private Transform cameraTarget;               // playerTPC.CinemachineCameraTarget.transform
    [SerializeField] private Transform walkTarget;                 // a transform on the floor in front of the door
    [SerializeField] private float stopDistance = 0.7f;            // how close before we stop

    [Header("Walk feel")]
    [SerializeField] private bool sprintDuringCutscene = false;    // true = run
    [SerializeField] private float faceTurnSpeed = 360f;           // deg/sec for turning camera target toward door

    [Header("Door triggering")]
    [SerializeField] private Animator doorAnimator;                // optional: has "Open" trigger
    [SerializeField] private string doorOpenTrigger = "Open";
    [SerializeField] private float doorOpenDelay = 0.2f;           // after reach target

    [Header("Lifecycle")]
    [SerializeField] private bool playOnStart = true;

    bool playing;
    float savedTimeScale = 1f;

    void Reset()
    {
        // Try to auto-fill when added
        if (!playerTPC) playerTPC = FindObjectOfType<ThirdPersonController>();
        if (!inputs && playerTPC) inputs = playerTPC.GetComponent<StarterAssetsInputs>();
#if ENABLE_INPUT_SYSTEM
        if (!playerInput && playerTPC) playerInput = playerTPC.GetComponent<PlayerInput>();
#endif
        if (!cameraTarget && playerTPC && playerTPC.CinemachineCameraTarget)
            cameraTarget = playerTPC.CinemachineCameraTarget.transform;
    }

    void Start()
    {
        if (playOnStart) Play();
    }

    public void Play()
    {
        if (playing || !playerTPC || !inputs || !walkTarget) return;
        playing = true;

        // disable gameplay input so the player can’t fight the script
#if ENABLE_INPUT_SYSTEM
        if (playerInput && playerInput.actions != null)
        {
            var ui = playerInput.actions.FindActionMap("UI", false);
            if (ui != null) playerInput.SwitchCurrentActionMap("UI");
        }
#endif
        inputs.move = Vector2.zero;
        inputs.sprint = sprintDuringCutscene;
        inputs.jump = false;

        // ensure controller is enabled so it still handles movement/anim/footsteps
        if (!playerTPC.enabled) playerTPC.enabled = true;

        // unlock cursor or keep it locked—your call. Lock is typical for 3rd person.
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;

        // start walking
        StartCoroutine(CutsceneRoutine());
    }

    System.Collections.IEnumerator CutsceneRoutine()
    {
        // We’ll keep feeding forward input until we’re close enough.
        while (true)
        {
            // flat direction to target (XZ only)
            Vector3 to = walkTarget.position - playerTPC.transform.position;
            to.y = 0f;
            float dist = to.magnitude;

            if (dist <= stopDistance) break;

            if (to.sqrMagnitude > 0.0001f)
            {
                // Rotate the CINEMACHINE CAMERA TARGET toward the door so that
                // StarterAssets "forward" input walks straight to it.
                if (cameraTarget)
                {
                    // Only yaw toward the target
                    Quaternion targetYaw = Quaternion.LookRotation(to.normalized, Vector3.up);
                    cameraTarget.rotation = Quaternion.RotateTowards(
                        cameraTarget.rotation,
                        targetYaw,
                        faceTurnSpeed * Time.unscaledDeltaTime
                    );
                }
            }

            // Feed "move forward" into the controller (keeps footsteps/anim working)
            inputs.move = Vector2.up;      // (0,1) = forward
            inputs.sprint = sprintDuringCutscene;

            yield return null; // next frame
        }

        // Arrived: stop moving
        inputs.move = Vector2.zero;
        inputs.sprint = false;

        // Small delay, then open the door
        if (doorOpenDelay > 0f) yield return new WaitForSeconds(doorOpenDelay);
        if (doorAnimator && !string.IsNullOrEmpty(doorOpenTrigger))
            doorAnimator.SetTrigger(doorOpenTrigger);

        // Optional: wait for the door to finish (or a fixed time). Here we wait 1s.
        yield return new WaitForSeconds(1f);

        Finish();
    }

    public void Finish()
    {
        if (!playing) return;
        playing = false;

        // Hand control back to the player
#if ENABLE_INPUT_SYSTEM
        if (playerInput && playerInput.actions != null)
        {
            var player = playerInput.actions.FindActionMap("Player", false);
            if (player != null) playerInput.SwitchCurrentActionMap("Player");
        }
#endif
        // nothing else needed; ThirdPersonController was kept enabled the whole time
    }
}
