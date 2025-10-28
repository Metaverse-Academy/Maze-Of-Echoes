// using UnityEngine;

// public class HintController_E : MonoBehaviour
// {
//     [Header("References")]
//     public GameObject hintPanel;         // واجهة الهنت اللي بتظهر
//     public Transform player;             // مجسم اللاعب (أو الكاميرا لو المنظور أول)
//     public float interactionDistance = 3f;  // المسافة المطلوبة للتفاعل
//     private bool isPanelActive = false;

//     void Update()
//     {
//         // زر التفاعل
//         if (Input.GetKeyDown(KeyCode.E))
//         {
//             if (!isPanelActive)
//             {
//                 TryShowHint();
//             }
//             else
//             {
//                 HideHint();
//             }
//         }
//     }

//     void TryShowHint()
//     {
//         // نبحث عن أقرب كائن عليه Tag = "Hint"
//         GameObject[] hints = GameObject.FindGameObjectsWithTag("Hint");
//         foreach (GameObject hint in hints)
//         {
//             float dist = Vector3.Distance(player.position, hint.transform.position);
//             if (dist <= interactionDistance)
//             {
//                 ShowHint();
//                 return; // أول هنت قريب يكفي
//             }
//         }

//         Debug.Log("لا يوجد Hint قريب!");
//     }

//     void ShowHint()
//     {
//         hintPanel.SetActive(true);
//         isPanelActive = true;

//         // قفل الماوس (اختياري)
//         Cursor.lockState = CursorLockMode.None;
//         Cursor.visible = true;

//         Debug.Log("تم فتح شاشة الهنت!");
//     }

//     void HideHint()
//     {
//         hintPanel.SetActive(false);
//         isPanelActive = false;

//         // رجع التحكم باللاعب
//         Cursor.lockState = CursorLockMode.Locked;
//         Cursor.visible = false;

//         Debug.Log("تم إغلاق شاشة الهنت!");
//     }
// }
//==========


using UnityEngine;
using UnityEngine.InputSystem; // نظام الإدخال الجديد

public class HintController_E : MonoBehaviour
{
    [Header("References")]
    public GameObject hintPanel;         
    public GameObject interactUI;        
    public Transform player;             
    public float interactionDistance = 3f;  

    private bool isPanelActive = false;
    private GameObject currentHint;      

    void Start()
    {
        if (interactUI != null)
            interactUI.SetActive(false);  // نخفي حرف E في البداية
    }

    void Update()
    {
        CheckForHintDistance();

        // زر E للتفاعل باستخدام Input System الجديد
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

    void CheckForHintDistance()
    {
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

        if (nearest != null && nearestDist <= interactionDistance)
        {
            currentHint = nearest;
            if (!isPanelActive && interactUI != null)
                interactUI.SetActive(true); // نظهر حرف E
        }
        else
        {
            currentHint = null;
            if (interactUI != null)
                interactUI.SetActive(false); // نخفي حرف E
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
