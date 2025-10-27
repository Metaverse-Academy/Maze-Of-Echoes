// Put this on the flashlight lying on the floor
using UnityEngine;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

[RequireComponent(typeof(Collider))]
public class FlashlightPickup : MonoBehaviour
{
    [SerializeField] private string playerTag = "Player";
    [SerializeField] private Transform playerSocket;           // Player/Head/Camera/FlashlightSocket
    [SerializeField] private GameObject promptUI;              // "Press E" UI (optional)
    [SerializeField] private AudioSource pickupSfx;            // optional

    private bool _inRange;
    private bool _picked;

    void Reset()
    {
        var c = GetComponent<Collider>();
        c.isTrigger = true;
    }

    void Start()
    {
        if (promptUI) promptUI.SetActive(false);
        var light = GetComponentInChildren<Light>(true);
        if (light) light.enabled = true; // stays ON on the floor
    }

    void Update()
    {
        if (_picked || !_inRange) return;

        bool pressed =
#if ENABLE_INPUT_SYSTEM
            Keyboard.current != null && Keyboard.current.eKey.wasPressedThisFrame
#else
            Input.GetKeyDown(KeyCode.E)
#endif
        ;
        if (pressed) DoPickup();
    }

    void OnTriggerEnter(Collider other)
    {
        if (_picked) return;
        if (!other.CompareTag(playerTag)) return;

        _inRange = true;
        if (promptUI) promptUI.SetActive(true);

        // auto-locate a socket if not set
        if (!playerSocket)
        {
            var tpc = other.transform;
            playerSocket = tpc.Find("FlashlightSocket"); // create this child on your player
            if (!playerSocket) playerSocket = tpc;       // fallback
        }
    }

    void OnTriggerExit(Collider other)
    {
        if (!other.CompareTag(playerTag)) return;
        _inRange = false;
        if (!_picked && promptUI) promptUI.SetActive(false);
    }

    void DoPickup()
    {
        if (_picked) return;
        if (!playerSocket)
        {
            Debug.LogWarning("FlashlightPickup: No player socket assigned/found.");
            return;
        }

        _picked = true;

        // stop physics
        var rb = GetComponent<Rigidbody>(); if (rb) Destroy(rb);
        var col = GetComponent<Collider>(); if (col) col.enabled = false;

        // parent and ZERO local pose (prevents jumping above head)
        transform.SetParent(playerSocket, false);
        transform.localPosition = Vector3.zero;
        transform.localRotation = Quaternion.identity;

        // bind this object to the player's toggle script (and disable any old one)
        var toggle = playerSocket.GetComponentInParent<FlashlightToggleSendMessage>();
        if (toggle) toggle.BindFlashlight(gameObject, false); // start OFF after pickup (change if you want)

        if (promptUI) promptUI.SetActive(false);
        if (pickupSfx) pickupSfx.Play();

        // keep light OFF initially after pickup (so player can toggle)
        var light = GetComponentInChildren<Light>(true);
        if (light) light.enabled = false;
        gameObject.SetActive(false); gameObject.SetActive(true); // refresh, if needed
    }
}
