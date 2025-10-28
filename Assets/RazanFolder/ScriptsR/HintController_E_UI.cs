using UnityEngine;
using UnityEngine.InputSystem; // نستخدم النظام الجديد

public class HintController_E_UI : MonoBehaviour
{
    [Header("References")]
    public GameObject hintPanel;         // واجهة الهنت (الـ Panel الكبير)
    public GameObject interactUI;        // النص أو الأيقونة اللي فيها حرف E
    public Transform player;             // اللاعب أو الكاميرا
    public float interactionDistance = 3f;

    private GameObject currentHint;      // الهنت اللي قريب منه اللاعب
    private bool isPanelActive = false;

    void Start()
    {
        // نخفي واجهة الـ "اضغط E" في البداية
        if (interactUI != null)
            interactUI.SetActive(false);
    }

    void Update()
    {
        CheckForHint();

        // زر E للتفاعل
        if (Keyboard.current.eKey.wasPressedThisFrame)
        {
            if (isPanelActive)
            {
                HideHint();
            }
            else if (currentHint != null)
            {
                ShowHint();
            }
        }
    }

    void CheckForHint()
    {
        // نبحث عن أقرب Hint
        GameObject[] hints = GameObject.FindGameObjectsWithTag("Hint");
        GameObject nearest = null;
        float nearestDist = Mathf.Infinity;

        foreach (GameObject hint in hints)
        {
            float dist = Vector3.Distance(player.position, hint.transform.position);
            if (dist < nearestDist)
            {
                nearest = hint;
                nearestDist = dist;
            }
        }

        // لو فيه هنت قريب بما فيه الكفاية
        if (nearest != null && nearestDist <= interactionDistance)
        {
            currentHint = nearest;
            if (!isPanelActive && interactUI != null)
                interactUI.SetActive(true);
        }
        else
        {
            currentHint = null;
            if (interactUI != null)
                interactUI.SetActive(false);
        }
    }

    void ShowHint()
    {
        hintPanel.SetActive(true);
        isPanelActive = true;
        interactUI.SetActive(false);

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        Debug.Log("تم فتح شاشة الهنت!");
    }

    void HideHint()
    {
        hintPanel.SetActive(false);
        isPanelActive = false;
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        Debug.Log("تم إغلاق شاشة الهنت!");
    }
}
