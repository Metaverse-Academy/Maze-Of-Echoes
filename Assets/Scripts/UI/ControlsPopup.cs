using UnityEngine;
using UnityEngine.UI;

#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

namespace StarterAssets
{
    public class ControlsPopup : MonoBehaviour
    {
        [Header("Popup")]
        [Tooltip("Root GameObject of your controls UI (panel).")]
        [SerializeField] private GameObject popupRoot;

        [Tooltip("Assign your Close button here.")]
        [SerializeField] private Button closeButton;

        [Header("Behavior")]
        [Tooltip("Pause the game while the popup is visible.")]
        [SerializeField] private bool pauseGame = true;

        [Tooltip("Disable player movement/look while the popup is visible.")]
        [SerializeField] private bool disablePlayerControls = true;

        [Tooltip("Lock and hide the cursor after closing (typical for FPS/3rd person).")]
        [SerializeField] private bool relockCursorOnClose = true;

        // Optional references if you use Starter Assets
        private ThirdPersonController _tpc;
        private StarterAssetsInputs _inputs;
#if ENABLE_INPUT_SYSTEM
        private PlayerInput _playerInput;
#endif
        private float _prevTimeScale = 1f;

        void Awake()
        {
            if (closeButton != null) closeButton.onClick.AddListener(ClosePopup);

            // Find components on the same object (if this is on the Player), otherwise you can drag them in if you want
            _tpc = GetComponent<ThirdPersonController>();
            _inputs = GetComponent<StarterAssetsInputs>();
#if ENABLE_INPUT_SYSTEM
            _playerInput = GetComponent<PlayerInput>();
#endif
        }

        void Start()
        {
            ShowPopup();
        }

        public void ShowPopup()
        {
            if (popupRoot != null) popupRoot.SetActive(true);

            if (pauseGame)
            {
                _prevTimeScale = Time.timeScale;
                Time.timeScale = 0f;
            }

            if (disablePlayerControls)
            {
                if (_tpc != null) _tpc.enabled = false;
                if (_inputs != null) _inputs.enabled = false;

#if ENABLE_INPUT_SYSTEM
                // Optional: switch to a UI action map if you use one
                if (_playerInput != null && _playerInput.actions != null)
                {
                    if (_playerInput.actions.FindActionMap("UI", true) != null)
                        _playerInput.SwitchCurrentActionMap("UI");
                }
#endif
            }

            // Show cursor for clicking Close
            Cursor.visible = true;
            Cursor.lockState = CursorLockMode.None;
        }

        public void ClosePopup()
        {
            if (popupRoot != null) popupRoot.SetActive(false);

            if (pauseGame)
                Time.timeScale = _prevTimeScale;

            if (disablePlayerControls)
            {
                if (_tpc != null) _tpc.enabled = true;
                if (_inputs != null) _inputs.enabled = true;

#if ENABLE_INPUT_SYSTEM
                if (_playerInput != null && _playerInput.actions != null)
                {
                    // Switch back to your gameplay map (Starter Assets default: "Player")
                    if (_playerInput.actions.FindActionMap("Player", true) != null)
                        _playerInput.SwitchCurrentActionMap("Player");
                }
#endif
            }

            if (relockCursorOnClose)
            {
                Cursor.visible = false;
                Cursor.lockState = CursorLockMode.Locked;
            }
        }
    }
}
