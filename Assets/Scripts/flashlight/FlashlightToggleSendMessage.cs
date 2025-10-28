using UnityEngine;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif
using System.Collections;

public class FlashlightToggleSendMessage : MonoBehaviour
{
    [Header("Flashlight Root (model stays visible)")]
    [SerializeField] private GameObject flashlightRoot;

    [Header("What to toggle")]
    [SerializeField] private Light[] lights;              // auto-filled if left empty
    [SerializeField] private GameObject[] beamVisuals;    // optional: beam mesh/particles
    [SerializeField] private AudioSource clickAudio;      // optional

    [Header("TWIST: Off limit")]
    [Tooltip("Allow turning OFF this many times before the next OFF is blocked (stays ON).")]
    [SerializeField] private int allowedOffsBeforeBlock = 4;

    [Tooltip("Do a tiny fake flicker when the OFF is blocked, but end ON.")]
    [SerializeField] private bool flickerOnBlockedAttempt = true;

    [Tooltip("Total flicker time on the blocked attempt.")]
    [SerializeField] private float flickerDuration = 0.35f;

    [Tooltip("Use unscaled time so flicker works even if the game is paused.")]
    [SerializeField] private bool useUnscaledTime = true;

    private bool isOn = false;
    private int offAttemptsSinceReset = 0;
#if ENABLE_INPUT_SYSTEM
    private bool _pressed;
#endif

    void Awake()
    {
        if (flashlightRoot == null) flashlightRoot = gameObject;
        if (lights == null || lights.Length == 0)
            lights = flashlightRoot.GetComponentsInChildren<Light>(true);

        SetState(false); // start OFF (model still visible)
    }

    // Call this after pickup to bind the picked flashlight
    public void BindFlashlight(GameObject newRoot, bool startOn = false)
    {
        flashlightRoot = newRoot;
        lights = flashlightRoot.GetComponentsInChildren<Light>(true);
        offAttemptsSinceReset = 0; // reset the twist counter on new item
        SetState(startOn);
    }

#if ENABLE_INPUT_SYSTEM
    // PlayerInput (Send Messages) action named "Flashlight"
    public void OnFlashlight(InputValue v)
    {
        bool pressed = v.isPressed;
        if (pressed && !_pressed) Toggle();
        _pressed = pressed;
    }
#endif

    public void Toggle()
    {
        if (!lights?.Length.Equals(0) ?? false) { /* ok */ }

        if (isOn)
        {
            // Player is trying to turn it OFF
            offAttemptsSinceReset++;

            if (offAttemptsSinceReset <= allowedOffsBeforeBlock)
            {
                // Allowed OFF
                isOn = false;
                SetState(false);
                if (clickAudio) clickAudio.Play();
            }
            else
            {
                // Block this OFF (the scare). Reset counter and stay ON.
                offAttemptsSinceReset = 0;

                if (flickerOnBlockedAttempt)
                    StartCoroutine(FakeFlicker());

                // ensure it ends ON
                isOn = true;
                SetState(true);
                if (clickAudio) clickAudio.Play();
            }
        }
        else
        {
            // Turning ON is always allowed
            isOn = true;
            SetState(true);
            if (clickAudio) clickAudio.Play();
        }
    }

    private void SetState(bool on)
    {
        // Keep the model visible; only affect lights/beam
        if (lights != null)
            foreach (var l in lights) if (l) l.enabled = on;

        if (beamVisuals != null)
            foreach (var go in beamVisuals) if (go) go.SetActive(on);
    }

    private IEnumerator FakeFlicker()
    {
        // brief fake flicker but end ON
        if (lights == null || lights.Length == 0) yield break;

        float t = 0f;
        while (t < flickerDuration)
        {
            t += useUnscaledTime ? Time.unscaledDeltaTime : Time.deltaTime;

            // quick off
            foreach (var l in lights) if (l) l.enabled = false;
            yield return Wait(0.06f);

            // back on
            foreach (var l in lights) if (l) l.enabled = true;
            yield return Wait(0.07f);
        }

        // ensure ON at end
        foreach (var l in lights) if (l) l.enabled = true;
    }

    private WaitForSecondsRealtime _wsr = new WaitForSecondsRealtime(0.01f);
    private IEnumerator Wait(float s)
    {
        if (useUnscaledTime)
        {
            float t = 0f;
            while (t < s)
            {
                t += Time.unscaledDeltaTime;
                yield return null;
            }
        }
        else
        {
            yield return new WaitForSeconds(s);
        }
    }
}
