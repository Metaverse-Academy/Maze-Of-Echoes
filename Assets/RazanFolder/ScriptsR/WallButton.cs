using UnityEngine;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif
using System.Collections;

[RequireComponent(typeof(Collider))]
public class WallButton : MonoBehaviour
{
    [Header("Player detection")]
    [SerializeField] private string playerTag = "Player";
    [SerializeField] private GameObject promptUI;          // "E"
    [SerializeField] private float interactDistance = 0f;  // 0 = disable extra distance check

    [Header("Button press (simple anim)")]
    [SerializeField] private Transform pressablePart;      // red cap
    [SerializeField] private float pressDepth = 0.04f;
    [SerializeField] private float pressSpeed = 10f;       // higher = snappier
    [SerializeField] private AudioSource pressSfx;

    [Header("Wall movement")]
    [SerializeField] private Transform targetWall;         // assign in Inspector!
    [SerializeField] private Vector3 wallMoveOffset = new Vector3(2f, 0f, 0f);
    [SerializeField] private float wallMoveTime = 1.0f;
    [SerializeField] private AnimationCurve wallCurve = null;  // leave null = smoothstep
    [SerializeField] private bool useUnscaledTime = true;

    [Header("Movement Audio (simple)")]
    [Tooltip("Assign an AudioSource with your looping motor/slide clip. It will Play() at move start and Stop() at move end.")]
    [SerializeField] private AudioSource moveAudio;        // must have a clip assigned in Inspector

    [Header("One-shot / cooldown")]
    [SerializeField] private bool oneShot = true;
    [SerializeField] private float cooldown = 1f;

    bool playerIn, busy, used;
    Vector3 capStartLocalPos;
    Vector3 wallStartPos;
    Rigidbody wallRb;

    void Reset()
    {
        var c = GetComponent<Collider>();
        c.isTrigger = true;
    }

    void Awake()
    {
        if (promptUI) promptUI.SetActive(false);
        if (pressablePart) capStartLocalPos = pressablePart.localPosition;

        if (targetWall)
        {
            wallStartPos = targetWall.position;
            wallRb = targetWall.GetComponent<Rigidbody>();
        }

        if (wallCurve == null)
            wallCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);

        // Prepare movement audio (keep it dead simple)
        if (moveAudio)
        {
            moveAudio.playOnAwake = false;
            moveAudio.loop = true;           // ensure looping
            moveAudio.dopplerLevel = 0f;     // optional: avoid weird pitch changes while moving
            // IMPORTANT: assign a loop clip to moveAudio.clip in the Inspector
        }
    }

    void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag(playerTag)) return;
        playerIn = true;
        if (promptUI && !used) promptUI.SetActive(true);
    }

    void OnTriggerExit(Collider other)
    {
        if (!other.CompareTag(playerTag)) return;
        playerIn = false;
        if (promptUI) promptUI.SetActive(false);
    }

    void Update()
    {
        if (!playerIn || busy || (oneShot && used)) return;

        bool pressed =
#if ENABLE_INPUT_SYSTEM
            (Keyboard.current != null && Keyboard.current.eKey.wasPressedThisFrame)
#else
            Input.GetKeyDown(KeyCode.E)
#endif
            ;

        if (!pressed) return;

        if (interactDistance > 0f)
        {
            var p = GameObject.FindGameObjectWithTag(playerTag);
            if (p && Vector3.Distance(p.transform.position, transform.position) > interactDistance)
                return;
        }

        if (!targetWall)
        {
            Debug.LogWarning("[WallButton] targetWall is NOT assigned.");
            return;
        }

        StartCoroutine(PressAndMove());
    }

    IEnumerator PressAndMove()
    {
        busy = true; used = true;
        if (promptUI) promptUI.SetActive(false);
        if (pressSfx) pressSfx.Play();

        // Press cap
        if (pressablePart)
        {
            yield return MoveCap(capStartLocalPos,
                                 capStartLocalPos - pressablePart.forward * pressDepth,
                                 1f / Mathf.Max(0.01f, pressSpeed));
            yield return Wait(0.05f);
            yield return MoveCap(pressablePart.localPosition, capStartLocalPos, 1f / Mathf.Max(0.01f, pressSpeed));
        }

        // Ensure RB won’t fight us
        if (wallRb && !wallRb.isKinematic)
            wallRb.isKinematic = true;

        // --- AUDIO: just start the loop now ---
        if (moveAudio && moveAudio.clip)
            moveAudio.Play();

        // Move wall
        Vector3 a = wallStartPos;
        Vector3 b = wallStartPos + wallMoveOffset;
        float t = 0f;
        float dur = Mathf.Max(0.0001f, wallMoveTime);
        while (t < dur)
        {
            t += useUnscaledTime ? Time.unscaledDeltaTime : Time.deltaTime;
            float u = wallCurve.Evaluate(Mathf.Clamp01(t / dur));
            Vector3 pos = Vector3.LerpUnclamped(a, b, u);

            if (wallRb) wallRb.MovePosition(pos);
            else targetWall.position = pos;

            yield return null;
        }
        if (wallRb) wallRb.MovePosition(b); else targetWall.position = b;

        // --- AUDIO: stop exactly when movement ends ---
        if (moveAudio)
            moveAudio.Stop();

        if (!oneShot)
        {
            yield return Wait(cooldown);
            used = false;
            if (playerIn && promptUI) promptUI.SetActive(true);
        }

        busy = false;
    }

    IEnumerator MoveCap(Vector3 from, Vector3 to, float duration)
    {
        float t = 0f;
        duration = Mathf.Max(0.0001f, duration);
        while (t < duration)
        {
            t += useUnscaledTime ? Time.unscaledDeltaTime : Time.deltaTime;
            float u = Mathf.Clamp01(t / duration);
            pressablePart.localPosition = Vector3.Lerp(from, to, u);
            yield return null;
        }
        pressablePart.localPosition = to;
    }

    IEnumerator Wait(float seconds)
    {
        float t = 0f;
        while (t < seconds)
        {
            t += useUnscaledTime ? Time.unscaledDeltaTime : Time.deltaTime;
            yield return null;
        }
    }
}
