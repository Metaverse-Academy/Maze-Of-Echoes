using UnityEngine;
using System.Collections;

public class FlashlightAutoToggle : MonoBehaviour
{
    [Header("Light")]
    [SerializeField] private Light flashlight;            // assign, or auto-grab
    [SerializeField] private bool startOn = true;
    [SerializeField] private float onIntensity = 700f;
    [SerializeField] private float spotAngle = 45f;

    [Header("Timing")]
    [SerializeField] private Vector2 onDuration  = new Vector2(2.5f, 4.0f);
    [SerializeField] private Vector2 offDuration = new Vector2(1.2f, 2.0f);

    [Header("Flicker Before OFF")]
    [SerializeField] private bool enableFlicker = true;
    [SerializeField] private Vector2 flickerTotalTime = new Vector2(0.35f, 0.75f);
    [SerializeField] private Vector2 flickerStepTime  = new Vector2(0.03f, 0.12f);
    [SerializeField, Range(0f,1f)] private float flickerMinIntensityFactor = 0.25f;
    [SerializeField] private bool randomizeSpotAngle = true;
    [SerializeField] private float spotAngleJitter = 4f;

    [Header("Audio")]
    [SerializeField] private AudioSource sfx;            // auto-added if missing
    [SerializeField] private AudioClip clickOn;          // plays when turning ON
    [SerializeField] private AudioClip clickOff;         // plays when turning OFF (after flicker or direct)
    [SerializeField] private AudioClip flickerClick;     // small tick during flicker steps
    [SerializeField, Range(0f,1f)] private float volOn   = 1f;
    [SerializeField, Range(0f,1f)] private float volOff  = 1f;
    [SerializeField, Range(0f,1f)] private float volFlick = 0.7f;
    [SerializeField] private bool randomizePitch = true;
    [SerializeField] private Vector2 pitchRange = new Vector2(0.96f, 1.04f);

    float timer;
    bool isOn;
    bool isFlickering;
    float baseIntensity;
    float baseSpotAngle;

    void Awake()
    {
        if (!flashlight) flashlight = GetComponent<Light>();
        if (!sfx)
        {
            sfx = GetComponent<AudioSource>();
            if (!sfx) sfx = gameObject.AddComponent<AudioSource>();
            sfx.playOnAwake = false;
            sfx.spatialBlend = 1f;
            sfx.rolloffMode = AudioRolloffMode.Linear;
            sfx.minDistance = 2f;
            sfx.maxDistance = 15f;
        }

        baseIntensity = onIntensity;
        baseSpotAngle = spotAngle;

        isOn = startOn;
        ApplyLight();
        timer = NextSpan();
    }

    void Update()
    {
        if (isFlickering) return;

        timer -= Time.deltaTime;
        if (timer > 0f) return;

        // Time to change state
        if (isOn && enableFlicker)
        {
            // About to turn OFF → flicker first
            StartCoroutine(FlickerThenTurnOff());
        }
        else
        {
            // Simple toggle without flicker
            bool wasOn = isOn;
            isOn = !isOn;
            ApplyLight();

            if (!wasOn && isOn) PlayOnClick();   // OFF -> ON
            if (wasOn && !isOn) PlayOffClick();  // ON  -> OFF

            timer = NextSpan();
        }
    }

    IEnumerator FlickerThenTurnOff()
    {
        isFlickering = true;

        float total = Random.Range(flickerTotalTime.x, flickerTotalTime.y);
        float t = 0f;

        float origIntensity = baseIntensity;
        float origAngle = baseSpotAngle;

        while (t < total)
        {
            float step = Random.Range(flickerStepTime.x, flickerStepTime.y);
            t += step;

            // Dip intensity
            float factor = Random.Range(flickerMinIntensityFactor, 1f);
            flashlight.enabled = true;
            flashlight.intensity = origIntensity * factor;

            // Cone jitter
            if (randomizeSpotAngle)
                flashlight.spotAngle = Mathf.Clamp(origAngle + Random.Range(-spotAngleJitter, spotAngleJitter), 5f, 120f);

            PlayFlickerTick(); // SFX only during flicker

            // Half step lit
            yield return new WaitForSeconds(step * 0.5f);

            // Occasional blackout
            bool blackout = Random.value < 0.35f;
            if (blackout)
            {
                flashlight.enabled = false;
                PlayFlickerTick();
                yield return new WaitForSeconds(step * 0.5f);
            }
            else
            {
                yield return new WaitForSeconds(step * 0.5f);
            }
        }

        // Finally OFF
        flashlight.enabled = false;
        flashlight.intensity = origIntensity;
        flashlight.spotAngle = origAngle;

        // Stop any lingering audio and play OFF click once
        if (sfx) sfx.Stop();
        PlayOffClick();

        isOn = false;
        isFlickering = false;
        timer = NextSpan();
    }

    float NextSpan()
    {
        return isOn
            ? Random.Range(onDuration.x, onDuration.y)
            : Random.Range(offDuration.x, offDuration.y);
    }

    void ApplyLight()
    {
        if (!flashlight) return;
        flashlight.spotAngle = baseSpotAngle;
        flashlight.intensity = baseIntensity;
        flashlight.enabled = isOn;
    }

    // --- Audio helpers ---
    void PlayOnClick()
    {
        if (!sfx || !clickOn) return;
        sfx.pitch = randomizePitch ? Random.Range(pitchRange.x, pitchRange.y) : 1f;
        sfx.PlayOneShot(clickOn, volOn);
    }

    void PlayOffClick()
    {
        if (!sfx || !clickOff) return;
        sfx.pitch = randomizePitch ? Random.Range(pitchRange.x, pitchRange.y) : 1f;
        sfx.PlayOneShot(clickOff, volOff);
    }

    void PlayFlickerTick()
    {
        if (!sfx || !flickerClick) return;
        sfx.pitch = randomizePitch ? Random.Range(pitchRange.x, pitchRange.y) : 1f;
        sfx.PlayOneShot(flickerClick, volFlick);
    }
}
