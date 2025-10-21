using UnityEngine;

namespace StarterAssets
{
    [RequireComponent(typeof(CharacterController))]
    public class SprintBreathing : MonoBehaviour
    {
        [Header("Audio")]
        [Tooltip("Looping breathing clip. Set Loop=ON in the AudioSource.")]
        [SerializeField] private AudioSource breathingSource;
        [Range(0f, 1f)] public float maxVolume = 0.65f;
        public float fadeInSeconds = 0.25f;
        public float fadeOutSeconds = 0.25f;

        [Header("When to count as RUNNING")]
        [Tooltip("Minimum horizontal speed to consider the player moving.")]
        public float moveSpeedThreshold = 0.3f;
        [Tooltip("Optional reference to ThirdPersonController to check Grounded (recommended).")]
        public ThirdPersonController thirdPerson; // optional

        private CharacterController _cc;
        private StarterAssetsInputs _inputs;

        private bool _isRunningNow;
        private float _volVel; // for SmoothDamp

        void Awake()
        {
            _cc = GetComponent<CharacterController>();
            _inputs = GetComponent<StarterAssetsInputs>();
            if (thirdPerson == null) thirdPerson = GetComponent<ThirdPersonController>();

            if (breathingSource != null)
            {
                breathingSource.playOnAwake = false;
                breathingSource.loop = true;
                breathingSource.volume = 0f;
            }
        }

        void Update()
        {
            if (breathingSource == null || _inputs == null || _cc == null) return;

            // Horizontal speed (ignore vertical so jumping doesn't trigger it)
            Vector3 v = _cc.velocity; v.y = 0f;
            float horizSpeed = v.magnitude;

            bool groundedOk = thirdPerson == null ? true : thirdPerson.Grounded;
            bool shouldRun = _inputs.sprint && groundedOk && (horizSpeed > moveSpeedThreshold);

            // Edge: start/stop the looping source
            if (shouldRun && !_isRunningNow)
            {
                if (!breathingSource.isPlaying) breathingSource.Play();
            }
            else if (!shouldRun && _isRunningNow)
            {
                // keep playing while we fade out; we'll stop when silent
            }

            _isRunningNow = shouldRun;

            // Smooth volume
            float targetVol = shouldRun ? maxVolume : 0f;
            float smoothTime = shouldRun ? Mathf.Max(0.01f, fadeInSeconds) : Mathf.Max(0.01f, fadeOutSeconds);
            breathingSource.volume = Mathf.SmoothDamp(breathingSource.volume, targetVol, ref _volVel, smoothTime, Mathf.Infinity, Time.unscaledDeltaTime);

            // Stop the source once fully faded out
            if (!shouldRun && breathingSource.isPlaying && breathingSource.volume < 0.01f)
                breathingSource.Stop();
        }
    }
}
