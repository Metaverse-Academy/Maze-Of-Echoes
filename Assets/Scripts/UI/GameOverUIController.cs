using UnityEngine;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif
using UnityEngine.AI;

public class GameOverUIController : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] private GameObject loseUI; // your lose Image/Panel (starts disabled)

    [Header("Disable on Lose")]
    [Tooltip("Player movement/look scripts to disable (e.g., ThirdPersonController, StarterAssetsInputs).")]
    [SerializeField] private MonoBehaviour[] playerComponentsToDisable;

    [Tooltip("Monster AI scripts to disable (e.g., your AI controller scripts).")]
    [SerializeField] private MonoBehaviour[] monsterComponentsToDisable;

    [Tooltip("Optional: NavMeshAgents to stop instantly.")]
    [SerializeField] private NavMeshAgent[] agentsToStop;

    [Tooltip("Optional: Animators to pause (set speed=0).")]
    [SerializeField] private Animator[] animatorsToPause;

    [Header("Input (New Input System)")]
    [SerializeField] private bool switchToUIActionMap = true;
    [SerializeField] private string uiMapName = "UI";
    [SerializeField] private string playerMapName = "Player";

    private bool shown;

    public void ShowLose()
    {
        if (shown) return;
        shown = true;

        // 1) Show UI
        if (loseUI) loseUI.SetActive(true);

        // 2) Stop gameplay (without pausing Time)
        foreach (var c in playerComponentsToDisable) if (c) c.enabled = false;
        foreach (var c in monsterComponentsToDisable) if (c) c.enabled = false;

        if (agentsToStop != null)
        {
            foreach (var a in agentsToStop)
            {
                if (!a) continue;
                a.isStopped = true;
                a.velocity = Vector3.zero;
            }
        }

        if (animatorsToPause != null)
        {
            foreach (var an in animatorsToPause)
                if (an) an.speed = 0f;
        }

        // 3) Enable mouse / UI input
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;

        #if ENABLE_INPUT_SYSTEM
        if (switchToUIActionMap)
        {
            var pi = FindObjectOfType<PlayerInput>();
            if (pi && pi.actions != null)
            {
                var ui = pi.actions.FindActionMap(uiMapName, false);
                if (ui != null) pi.SwitchCurrentActionMap(uiMapName);
            }
        }
        #endif
    }

    // Optional: call this before restarting/going to menu if you want to restore state in the same scene.
    public void RestoreForGameplay()
    {
        foreach (var an in animatorsToPause) if (an) an.speed = 1f;
        foreach (var a in agentsToStop) if (a) a.isStopped = false;
        foreach (var c in playerComponentsToDisable) if (c) c.enabled = true;
        foreach (var c in monsterComponentsToDisable) if (c) c.enabled = true;

        #if ENABLE_INPUT_SYSTEM
        var pi = FindObjectOfType<PlayerInput>();
        if (pi && pi.actions != null)
        {
            var player = pi.actions.FindActionMap(playerMapName, false);
            if (player != null) pi.SwitchCurrentActionMap(playerMapName);
        }
        #endif

        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;
        if (loseUI) loseUI.SetActive(false);
        shown = false;
    }
}
