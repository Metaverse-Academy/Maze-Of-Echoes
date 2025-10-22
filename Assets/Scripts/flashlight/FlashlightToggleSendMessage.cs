using UnityEngine;
using System;

#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif
using System.Collections;

public class FlashlightToggleSendMessage : MonoBehaviour
{

    public static Action<bool>OnFlashlightClick;
    [Header("Flashlight")]
    [SerializeField] private GameObject flashlight;          // child with Light component
    [SerializeField] private AudioSource clickAudio;         // optional: normal toggle sound
    [SerializeField] private AudioSource scareAudio;         // optional: plays on the blocked (5th) attempt

    [Header("Twist Settings")]
    [Tooltip("Allow turning OFF this many times before the next OFF is blocked.")]
    [SerializeField] private int allowedOffsBeforeBlock = 4;

    [Tooltip("Optional small fake flicker on the blocked attempt, but end ON.")]
    [SerializeField] private bool flickerOnBlockedAttempt = true;

    [Tooltip("Total time for the fake flicker (if enabled).")]
    [SerializeField] private float flickerDuration = 0.35f;

    private bool isOn = false;
    private int offAttemptsSinceReset = 0;

#if ENABLE_INPUT_SYSTEM
    private bool _pressed; // edge detection for Send Messages
#endif

    void Awake()
    {
        if (flashlight != null) flashlight.SetActive(false);
        isOn = false;
        offAttemptsSinceReset = 0;
    }

#if ENABLE_INPUT_SYSTEM
    // Called by PlayerInput (Behavior = Send Messages) for an action named "Flashlight"
    public void OnFlashlight(InputValue value)
    {
        bool pressed = value.isPressed;
        if (pressed && !_pressed)
            HandlePress();
        _pressed = pressed;
    }
#endif

    private void HandlePress()
    {
        if (flashlight == null) return;

        if (isOn)
        {
            // Player is trying to turn it OFF
            offAttemptsSinceReset++;

            if (offAttemptsSinceReset <= allowedOffsBeforeBlock)
            {
                // Allowed OFF
                SetFlashlight(false);
                Play(clickAudio);
            }
            else
            {
                // Block this OFF (the scare)
                offAttemptsSinceReset = 0; // reset sequence
                // Stay ON, maybe do a quick fake flicker
                if (flickerOnBlockedAttempt)
                    StartCoroutine(FakeFlicker());
                Play(scareAudio);
            }
        }
        else
        {
            // Turning it ON is always allowed
            SetFlashlight(true);
            Play(clickAudio);
        }
        OnFlashlightClick?.Invoke(isOn);
    }

    private void SetFlashlight(bool on)
    {
        isOn = on;
        flashlight.SetActive(on);
    }

    private void Play(AudioSource src)
    {
        if (src != null) src.Play();
    }

    private IEnumerator FakeFlicker()
    {
        // brief: off-on-off-on ending ON
        float t = 0f;
        while (t < flickerDuration)
        {
            flashlight.SetActive(false);
            yield return new WaitForSeconds(0.06f);
            flashlight.SetActive(true);
            yield return new WaitForSeconds(0.07f);
            t += 0.13f;
        }
        flashlight.SetActive(true); // ensure it ends ON
        isOn = true;
    }
}
