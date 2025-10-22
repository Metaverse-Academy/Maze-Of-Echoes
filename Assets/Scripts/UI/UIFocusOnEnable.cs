using UnityEngine;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

public class UIFocusOnEnable : MonoBehaviour
{
    [Header("Gameplay Freeze")]
    [SerializeField] private bool pauseWhileOpen = true;

    [Header("Switch Action Map (New Input System)")]
    [SerializeField] private string uiActionMap = "UI";
    [SerializeField] private string playerActionMap = "Player";

    [Header("Disable These While Open (optional)")]
    [Tooltip("Drag your Player movement scripts here (e.g., ThirdPersonController, StarterAssetsInputs).")]
    [SerializeField] private MonoBehaviour[] componentsToDisable;

#if ENABLE_INPUT_SYSTEM
    private PlayerInput _playerInput;
#endif
    private float _prevTimeScale = 1f;

    void Awake()
    {
#if ENABLE_INPUT_SYSTEM
        _playerInput = FindObjectOfType<PlayerInput>();
#endif
    }

    void OnEnable()
    {
        if (pauseWhileOpen)
        {
            _prevTimeScale = Time.timeScale;
            Time.timeScale = 0f;
        }

        // allow mouse to click UI
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;

        // stop movement scripts if provided
        foreach (var m in componentsToDisable)
            if (m) m.enabled = false;

#if ENABLE_INPUT_SYSTEM
        if (_playerInput && _playerInput.actions != null)
        {
            var ui = _playerInput.actions.FindActionMap(uiActionMap, false);
            if (ui != null) _playerInput.SwitchCurrentActionMap(uiActionMap);
        }
#endif
    }

    void OnDisable()
    {
        if (pauseWhileOpen)
            Time.timeScale = _prevTimeScale;

        foreach (var m in componentsToDisable)
            if (m) m.enabled = true;

#if ENABLE_INPUT_SYSTEM
        if (_playerInput && _playerInput.actions != null)
        {
            var player = _playerInput.actions.FindActionMap(playerActionMap, false);
            if (player != null) _playerInput.SwitchCurrentActionMap(playerActionMap);
        }
#endif
        // lock mouse back for gameplay
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;
    }
}
