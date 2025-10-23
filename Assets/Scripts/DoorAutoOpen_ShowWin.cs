using UnityEngine;

public class DoorAutoOpen_ShowWin : MonoBehaviour
{
    [Header("Door Open")]
    public float openAngle = 90f;           // how much to rotate on Y
    public float openSpeed = 2f;            // slerp speed

    [Header("Win UI")]
    [Tooltip("Drag your Win panel/image here (should be disabled by default).")]
    public GameObject winPanel;
    [Tooltip("Delay after door fully opens before showing the panel.")]
    public float delayBeforeShow = 1.0f;

    private bool isOpening = false;
    private bool hasOpened = false;
    private Quaternion initialRotation;
    private Quaternion targetRotation;

    private void Start()
    {
        initialRotation = transform.rotation;
        targetRotation = Quaternion.Euler(transform.eulerAngles + new Vector3(0, openAngle, 0));

        // optional safety
        if (winPanel != null) winPanel.SetActive(false);
    }

    private void Update()
    {
        if (isOpening && !hasOpened)
        {
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * openSpeed);

            // When close enough to target, consider it opened
            if (Quaternion.Angle(transform.rotation, targetRotation) < 1f)
            {
                hasOpened = true;
                Invoke(nameof(ShowWinPanel), delayBeforeShow);
            }
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player") && !isOpening)
        {
            isOpening = true;
            Debug.Log("Player touched the door! Rotating to open...");
        }
    }

    private void ShowWinPanel()
    {
        if (winPanel == null)
        {
            Debug.LogWarning("Win panel is not assigned on DoorAutoOpen_ShowWin.");
            return;
        }

        // Show the panel
        winPanel.SetActive(true);

        // Make sure the cursor is usable for buttons
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;

        // If you use a script like UIFocusOnEnable on the panel,
        // it will handle disabling player/AI and switching to the UI action map.
    }
}
