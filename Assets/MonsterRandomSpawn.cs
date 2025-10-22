// ...existing code...
using System.Collections;
using Unity.Cinemachine;
using UnityEngine;

public class MonsterRandomSpawn : MonoBehaviour
{
    [Header("Spawn Settings")]
    public float spawnRangeX = 20f;          // مدى الظهور على محور X
    public float spawnRangeZ = 20f;          // مدى الظهور على محور Z
    public Vector3 centerPoint;              // نقطة المركز للظهور حولها
    public float minDisappearTime = 2f;      // أقل وقت اختفاء
    public float maxDisappearTime = 5f;      // أكثر وقت اختفاء
    public float runDuration = 3f;           // مدة الجري

    [Header("Chase Settings")]
    public float chaseRange = 4f;           // مدى المطاردة
    public float chaseSpeed = 7f;            // سرعة المطاردة
    public float normalSpeed = 5f;           // السرعة العادية

    [Header("References")]
    public Transform player;                 // مرجع اللاعب
    [SerializeField] private Renderer monsterRenderer;

    // ...added fields...
    [Header("Appear In Front of Camera")]
    public Transform playerCamera;           // camera transform to use (if null will use Camera.main)
    public float appearDistance = 3f;        // distance in front of camera where monster will appear
    public float appearHeightOffset = 0f;    // vertical offset relative to player's y
    // ...existing code...
    private Animator animator;
    private bool isChasing = false;
    private Coroutine spawnRoutine;

    void Start()
    {
        animator = GetComponent<Animator>();

        // إذا لم يتم تعيين اللاعب، ابحث عنه
        if (player == null)
        {
            GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
            if (playerObj != null)
            {
                player = playerObj.transform;
            }
        }

        // set camera reference if not assigned
        if (playerCamera == null && Camera.main != null)
        {
            playerCamera = Camera.main.transform;
        }

        // إذا لم يتم تعيين نقطة المركز، استخدم موقع الوحش الحالي
        if (centerPoint == Vector3.zero)
        {
            centerPoint = transform.position;
        }

        // بدء روتين الظهور العشوائي
        spawnRoutine = StartCoroutine(RandomSpawnRoutine());
    }

    private void OnFlashLightClick(bool isOn)
    {
        // يمكن استخدام هذا الحدث لإيقاف الوحش مؤقتًا إذا كانت المصباح قيد التشغيل
        if (isOn)
        {
            // إيقاف المطاردة إذا كانت المصباح قيد التشغيل
           
                StartChasing();
            }
        }
    

    void Update()
    {
        if (player == null) return;

        // حساب المسافة بين الوحش واللاعب
        float distanceToPlayer = Vector3.Distance(transform.position, player.position);

        // التحقق من دخول اللاعب في مدى المطاردة
        if (distanceToPlayer <= chaseRange && monsterRenderer.enabled && !isChasing)
        {
            StartChasing();
        }
        else if (distanceToPlayer > chaseRange && isChasing)
        {
            StopChasing();
        }

        // تنفيذ المطاردة
        if (isChasing)
        {
            ChasePlayer();
        }


    }

    IEnumerator RandomSpawnRoutine()
    {
        while (true)
        {
            // اختفاء
            monsterRenderer.enabled = false;
            animator.SetBool("isRunning", false);

            // وقت اختفاء عشوائي
            float disappearTime = Random.Range(minDisappearTime, maxDisappearTime);
            yield return new WaitForSeconds(disappearTime);

            // الانتقال لموقع عشوائي
            TeleportToRandomPosition();

            // ظهور
            monsterRenderer.enabled = true;
            animator.SetBool("isRunning", true);

            // الحركة للأمام
            float timer = 0;
            while (timer < runDuration && !isChasing)
            {
                transform.position += transform.forward * normalSpeed * Time.deltaTime;
                timer += Time.deltaTime;
                yield return null;
            }

            // إذا بدأت المطاردة، توقف عن الروتين حتى تنتهي المطاردة
            while (isChasing)
            {
                yield return null;
            }

            // وقوف بعد الجري
            animator.SetBool("isRunning", false);
        }
    }

    void TeleportToRandomPosition()
    {
        // توليد موقع عشوائي حول نقطة المركز
        float randomX = Random.Range(-spawnRangeX, spawnRangeX);
        float randomZ = Random.Range(-spawnRangeZ, spawnRangeZ);

        Vector3 randomPosition = centerPoint + new Vector3(randomX, 0, randomZ);
        transform.position = randomPosition;

        // دوران عشوائي
        transform.rotation = Quaternion.Euler(0, Random.Range(0, 360), 0);

        Debug.Log("Monster spawned at: " + randomPosition);
    }

    void StartChasing()
    {
        isChasing = true;
        animator.SetBool("isRunning", true);

        // If we have a player camera, teleport the monster to a point in front of that camera
        if (playerCamera != null)
        {
            Vector3 appearPos = playerCamera.position + playerCamera.forward * appearDistance;
            // Align vertically with the player's y plus optional offset so the monster is not at camera height
            appearPos.y = player.position.y + appearHeightOffset;

            transform.position = appearPos;

            // rotate to face the player
            Vector3 lookDir = (player.position - transform.position);
            lookDir.y = 0;
            if (lookDir != Vector3.zero)
                transform.rotation = Quaternion.LookRotation(lookDir.normalized);
        }

        Debug.Log("Monster started chasing!");
    }

    void StopChasing()
    {
        isChasing = false;
        animator.SetBool("isRunning", false);
        Debug.Log("Monster stopped chasing!");
    }

    void ChasePlayer()
    {
        float distanceToPlayer = Vector3.Distance(transform.position, player.position);
        // التحرك باتجاه اللاعب
        Vector3 direction = (player.position - transform.position).normalized;
        transform.position += direction * chaseSpeed * Time.deltaTime;

        // تدوير الوحش باتجاه اللاعب
        if (direction != Vector3.zero)
        {
            Quaternion lookRotation = Quaternion.LookRotation(new Vector3(direction.x, 0, direction.z));
            transform.rotation = Quaternion.Slerp(transform.rotation, lookRotation, Time.deltaTime * 10f);
        }

        if (distanceToPlayer <= 2f)
        {
            // Reached the player — stop chasing (you can replace this with attack logic)
            StopChasing();
        }
    }

    // رسم منطقة الظهور العشوائي في المحرر
    void OnDrawGizmosSelected()
    {
        // مدى المطاردة (أحمر)
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, chaseRange);

        // منطقة الظهور العشوائي (أخضر)
        Gizmos.color = Color.green;
        Vector3 center = centerPoint == Vector3.zero ? transform.position : centerPoint;
        Gizmos.DrawWireCube(center, new Vector3(spawnRangeX * 2, 1, spawnRangeZ * 2));
    }
}
// ...existing code...