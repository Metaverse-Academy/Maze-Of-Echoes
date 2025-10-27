using UnityEngine;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

public class FlashlightToggleSendMessage : MonoBehaviour
{
    [Header("Flashlight Root (model stays visible)")]
    [SerializeField] private GameObject flashlightRoot;   // the picked-up flashlight object (mesh stays ON)

    [Header("What to toggle")]
    [SerializeField] private Light[] lights;              // auto-filled if left empty
    [SerializeField] private GameObject[] beamVisuals;    // optional: meshes/particles for the beam (enable with light)
    [SerializeField] private AudioSource clickAudio;      // optional

    private bool isOn = false;
#if ENABLE_INPUT_SYSTEM
    private bool _pressed;
#endif

    void Awake()
    {
        if (flashlightRoot == null) flashlightRoot = gameObject; // safe default
        if (lights == null || lights.Length == 0)
            lights = flashlightRoot.GetComponentsInChildren<Light>(true);

        SetState(false); // start OFF (model still visible)
    }

    // Call this after pickup to bind the newly picked flashlight
    public void BindFlashlight(GameObject newRoot, bool startOn = false)
    {
        flashlightRoot = newRoot;
        lights = flashlightRoot.GetComponentsInChildren<Light>(true);
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
        isOn = !isOn;
        SetState(isOn);
        if (clickAudio) clickAudio.Play();
    }

    private void SetState(bool on)
    {
        // NEVER SetActive(false) on the flashlightRoot — keep the mesh visible
        if (lights != null)
            foreach (var l in lights) if (l) l.enabled = on;

        if (beamVisuals != null)
            foreach (var go in beamVisuals) if (go) go.SetActive(on);
    }
}
