using System.Collections;
using UnityEngine;

public class MonsterMovement : MonoBehaviour
{
    private Animator animator;
    public float moveSpeed = 5f;
    public float disappearTime = 2f; // وقت الاختفاء بالثواني
    public float runDuration = 3f; // مدة الجري
    public float waitBeforeRepeat = 5f; // وقت الانتظار قبل التكرار
    
    [SerializeField] private Renderer monsterRenderer;
    
    void Start()
    {
        animator = GetComponent<Animator>();
       
        
        // يبدأ تلقائياً
        StartCoroutine(DisappearAndRun());
    }
    
    IEnumerator DisappearAndRun()
    {
        while(true) // يتكرر للأبد
        {
            // اختفاء فجأة
            monsterRenderer.enabled = false;
            animator.SetBool("isRunning", false);
            
            // انتظار
            yield return new WaitForSeconds(disappearTime);
            
            // ظهور وجري على طول
            monsterRenderer.enabled = true;
            animator.SetBool("isRunning", true);
            
            // الحركة للأمام
            float timer = 0;
            while(timer < runDuration)
            {
                transform.position += transform.forward * moveSpeed * Time.deltaTime;
                timer += Time.deltaTime;
                yield return null;
            }
            
            // وقوف بعد الجري
            animator.SetBool("isRunning", false);
            
            // انتظار قبل التكرار مرة ثانية
            yield return new WaitForSeconds(waitBeforeRepeat);
        }
    }
}