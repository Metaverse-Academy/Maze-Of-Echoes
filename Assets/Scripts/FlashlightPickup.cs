using UnityEngine;
using UnityEngine.UI;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;  // for Keyboard.current.eKey
#endif

[RequireComponent(typeof(Collider))]
public class FlashlightPickup : MonoBehaviour
{
    [Header("Pickup")]
    [Tooltip("Tag the player object with this tag.")]
    [SerializeField] private string playerTag = "Player";

    [Tooltip("Where to attach on the player once picked. Create a child on the player (e.g., 'FlashlightMount') and drag it here.")]
    [SerializeField] private Transform playerMount;

    [Tooltip("Position relative to the mount after pickup.")]
    [SerializeField] private Vector3 localPosition = new Vector3(0.1f, -0.05f, 0.25f);

    [Tooltip("Rotation (Euler) relative to the mount after pickup.")]
    [SerializeField] private Vector3 localEuler = new Vector3(0f, 0f, 0f);

    [Header("UI Prompt")]
    [Tooltip("UI GameObject that says 'Press E to pick up' (can be screen-space or world-space).")]
    [SerializeField] private GameObject promptUI;

    [Header("Audio")]
    [SerializeField] private AudioSource pickupSfx; // optional

    private bool playerInRange = false;
    private bool pickedUp = false;

    void Reset()
    {
        // Make the collider a trigger by default for proximity detection
        var c = GetComponent<Collider>();
        c.isTrigger = true;
    }

    void Start()
    {
        if (promptUI) promptUI.SetActive(false);
        // ensure flashlight light is on while on the floor
        var light = GetComponentInChildren<Light>(true);
        if (light) light.enabled = true;
    }

    void Update()
    {
        if (pickedUp || !playerInRange) return;

        bool pressed =
#if ENABLE_INPUT_SYSTEM
            (Keyboard.current != null && Keyboard.current.eKey.wasPressedThisFrame)
#else
            Input.GetKeyDown(KeyCode.E)
#endif
            ;

        if (pressed)
        {
            DoPickup();
        }
    }

    void OnTriggerEnter(Collider other)
    {
        if (pickedUp) return;
        if (other.CompareTag(playerTag))
        {
            playerInRange = true;
            if (promptUI) promptUI.SetActive(true);

            // if no mount assigned in inspector, try to find one under the player
            if (playerMount == null)
            {
                var mount = other.transform.Find("FlashlightMount");
                if (mount) playerMount = mount;
            }
        }
    }

    void OnTriggerExit(Collider other)
    {
        if (!other.CompareTag(playerTag)) return;
        playerInRange = false;
        if (!pickedUp && promptUI) promptUI.SetActive(false);
    }

    private void DoPickup()
    {
        if (pickedUp) return;
        if (playerMount == null)
        {
            Debug.LogWarning("FlashlightPickup: Player mount not set. Create a child on the player (e.g., 'FlashlightMount') and assign it.");
            return;
        }

        pickedUp = true;

        // Stop interacting with physics/world
        var rb = GetComponent<Rigidbody>();
        if (rb) Destroy(rb);
        var col = GetComponent<Collider>();
        if (col) col.enabled = false;

        // Attach to the player mount
        transform.SetParent(playerMount, worldPositionStays: false);
        transform.localPosition = localPosition;
        transform.localRotation = Quaternion.Euler(localEuler);

        // Hide prompt
        if (promptUI) promptUI.SetActive(false);

        // Sound
        if (pickupSfx) pickupSfx.Play();

        // (Optional) if you had a separate flashlight toggle script on the player/flashlight, enable it now.

        // Keep the light ON after pickup (you can toggle it later with your right-click script)
        var light = GetComponentInChildren<Light>(true);
        if (light) light.enabled = true;
    }
}
