using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class RedFlash : MonoBehaviour
{
    [Header("Refs")]
    [SerializeField] private Image flashImage;   // assign your full-screen red Image

    [Header("Timing")]
    [Tooltip("How fast it pops in (seconds).")]
    [SerializeField] private float fadeIn = 0.06f;
    [Tooltip("How fast it fades out (seconds).")]
    [SerializeField] private float fadeOut = 0.35f;

    [Header("Intensity")]
    [Range(0f, 1f)] public float deathAlpha = 0.65f;   // strong flash on death
    [Range(0f, 1f)] public float hitAlpha = 0.25f;     // lighter flash on damage

    [Header("Behavior")]
    [Tooltip("Use unscaled time so it works even if Time.timeScale = 0.")]
    public bool useUnscaledTime = true;

    static RedFlash _instance;
    public static RedFlash Instance => _instance;

    void Awake()
    {
        _instance = this;
        if (!flashImage) flashImage = GetComponent<Image>();
        SetAlpha(0f);
    }

    public void FlashDeath() => StartCoroutine(FlashRoutine(deathAlpha));
    public void FlashHit()   => StartCoroutine(FlashRoutine(hitAlpha));

    IEnumerator FlashRoutine(float targetAlpha)
    {
        if (!flashImage) yield break;

        // quick pop-in
        float t = 0f;
        float start = 0f;
        while (t < fadeIn)
        {
            t += useUnscaledTime ? Time.unscaledDeltaTime : Time.deltaTime;
            SetAlpha(Mathf.Lerp(start, targetAlpha, t / Mathf.Max(0.0001f, fadeIn)));
            yield return null;
        }
        SetAlpha(targetAlpha);

        // fade out
        t = 0f;
        while (t < fadeOut)
        {
            t += useUnscaledTime ? Time.unscaledDeltaTime : Time.deltaTime;
            SetAlpha(Mathf.Lerp(targetAlpha, 0f, t / Mathf.Max(0.0001f, fadeOut)));
            yield return null;
        }
        SetAlpha(0f);
    }

    void SetAlpha(float a)
    {
        if (!flashImage) return;
        var c = flashImage.color;
        c.a = a;
        flashImage.color = c;
    }
}
