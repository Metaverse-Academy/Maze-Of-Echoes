using UnityEngine;
using UnityEngine.UI;

#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif
using StarterAssets;

public class ControlsPopupTimed : MonoBehaviour
{
    [Header("Popup")]
    [Tooltip("Root GameObject of your controls UI (panel).")]
    [SerializeField] private GameObject popupRoot;

    [Tooltip("Optional Close button (not required).")]
    [SerializeField] private Button closeButton; // safe to leave null

    [Tooltip("Optional CanvasGroup for fade in/out.")]
    [SerializeField] private CanvasGroup canvasGroup; // optional

    [Header("Timing")]
    [Tooltip("Delay before the popup appears (seconds).")]
    [SerializeField] private float showDelay = 1.0f;

    [Tooltip("How long the popup stays visible before it auto-hides (seconds).")]
    [SerializeField] private float visibleSeconds = 3.0f;

    [Tooltip("Fade duration for show/hide (seconds). Set 0 for instant.")]
    [SerializeField] private float fadeSeconds = 0.25f;

    [Header("Gameplay while visible")]
    [Tooltip("Pause the game while the popup is visible (timer uses realtime).")]
    [SerializeField] private bool pauseGameWhileVisible = false;

    [Tooltip("Disable player movement/look while popup is visible.")]
    [SerializeField] private bool disablePlayerControls = false;

    [Tooltip("Lock & hide cursor after popup hides again (typical for 3rd-person/FPS).")]
    [SerializeField] private bool relockCursorOnHide = true;

    // Optional Starter Assets refs (only used if disablePlayerControls = true)
    private ThirdPersonController _tpc;
    private StarterAssetsInputs _inputs;
#if ENABLE_INPUT_SYSTEM
    private PlayerInput _playerInput;
#endif

    float _prevTimeScale = 1f;
    bool _running;

    void Awake()
    {
        // Optional wiring if this is placed on the Player:
        _tpc = GetComponent<ThirdPersonController>();
        _inputs = GetComponent<StarterAssetsInputs>();
#if ENABLE_INPUT_SYSTEM
        _playerInput = GetComponent<PlayerInput>();
#endif
        if (closeButton != null) closeButton.onClick.AddListener(CloseEarly);

        if (popupRoot != null) popupRoot.SetActive(false);
        if (canvasGroup != null) canvasGroup.alpha = 0f;
    }

    void Start()
    {
        // Kick off the timed sequence
        StartCoroutine(RunSequence());
    }

    System.Collections.IEnumerator RunSequence()
    {
        _running = true;

        // Wait before showing
        if (pauseGameWhileVisible) yield return new WaitForSecondsRealtime(showDelay);
        else yield return new WaitForSeconds(showDelay);

        ShowPopup();

        // Stay visible
        if (pauseGameWhileVisible) yield return new WaitForSecondsRealtime(visibleSeconds);
        else yield return new WaitForSeconds(visibleSeconds);

        HidePopup();

        _running = false;
    }

    void ShowPopup()
    {
        if (popupRoot == null) return;

        popupRoot.SetActive(true);

        if (pauseGameWhileVisible)
        {
            _prevTimeScale = Time.timeScale;
            Time.timeScale = 0f;
        }

        if (disablePlayerControls)
        {
            if (_tpc) _tpc.enabled = false;
            if (_inputs) _inputs.enabled = false;
#if ENABLE_INPUT_SYSTEM
            if (_playerInput && _playerInput.actions != null)
            {
                var uiMap = _playerInput.actions.FindActionMap("UI", false);
                if (uiMap != null) _playerInput.SwitchCurrentActionMap("UI");
            }
#endif
        }

        // Show cursor for reading/clicking (even if no button)
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;

        if (canvasGroup != null && fadeSeconds > 0f)
            StartCoroutine(FadeCanvas(canvasGroup, 0f, 1f, fadeSeconds, pauseGameWhileVisible));
        else if (canvasGroup != null)
            canvasGroup.alpha = 1f;
    }

    void HidePopup()
    {
        if (canvasGroup != null && fadeSeconds > 0f)
            StartCoroutine(FadeOutThenDisable());
        else
            FinalizeHide();
    }

    System.Collections.IEnumerator FadeOutThenDisable()
    {
        yield return FadeCanvas(canvasGroup, 1f, 0f, fadeSeconds, pauseGameWhileVisible);
        FinalizeHide();
    }

    void FinalizeHide()
    {
        if (pauseGameWhileVisible)
            Time.timeScale = _prevTimeScale;

        if (disablePlayerControls)
        {
            if (_tpc) _tpc.enabled = true;
            if (_inputs) _inputs.enabled = true;
#if ENABLE_INPUT_SYSTEM
            if (_playerInput && _playerInput.actions != null)
            {
                var playerMap = _playerInput.actions.FindActionMap("Player", false);
                if (playerMap != null) _playerInput.SwitchCurrentActionMap("Player");
            }
#endif
        }

        if (relockCursorOnHide)
        {
            Cursor.visible = false;
            Cursor.lockState = CursorLockMode.Locked;
        }

        if (popupRoot != null) popupRoot.SetActive(false);
    }

    void CloseEarly()
    {
        if (!_running) return; // ignore if already finished
        StopAllCoroutines();   // stop any pending waits/fades
        HidePopup();
        _running = false;
    }

    static System.Collections.IEnumerator FadeCanvas(CanvasGroup cg, float from, float to, float seconds, bool useRealtime)
    {
        if (cg == null || seconds <= 0f) yield break;
        cg.alpha = from;
        float t = 0f;
        while (t < seconds)
        {
            t += useRealtime ? Time.unscaledDeltaTime : Time.deltaTime;
            cg.alpha = Mathf.Lerp(from, to, Mathf.Clamp01(t / seconds));
            yield return null;
        }
        cg.alpha = to;
    }
}
