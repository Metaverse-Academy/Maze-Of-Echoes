using UnityEngine;

public class HealthDamage : MonoBehaviour
{
    public int maxHealth = 100;
    public int currentHealth;
    public GameObject gameOverPanel;        // حط الـ Panel هنا من الـ Inspector
    public AudioSource deathMusic;          // موسيقى الموت
    public GameOverUIController gameOver;
    void Start()
    {
        currentHealth = maxHealth;
        
        //if (gameOverPanel != null)
           // gameOverPanel.SetActive(false);
    }
    
    public void TakeDamage(int damage)
    {
        currentHealth -= damage;
        
        if (currentHealth <= 0)
        {
            Die();
        }
    }
    
    void Die()
    {
        //if (gameOver) gameOver.ShowLose();
        //if (deathMusic) deathMusic.Play();
        if (RedFlash.Instance) RedFlash.Instance.FlashDeath();
        if (gameOverPanel) gameOverPanel.SetActive(true);

    Cursor.visible = true;
    Cursor.lockState = CursorLockMode.None;

    if (deathMusic)
    {
        deathMusic.ignoreListenerPause = true;
        deathMusic.Play();
    }
        //Time.timeScale = 0f;                // يوقف اللعبة
        
        //if (gameOverPanel != null)
            //gameOverPanel.SetActive(true);
        
        // تشغيل موسيقى الموت
        //if (deathMusic != null)
        //{
            //deathMusic.ignoreListenerPause = true;  // عشان يشتغل حتى لو اللعبة واقفة
            //deathMusic.Play();
        //}
    }
    
    // public void RestartGame()
    // {
    //     Time.timeScale = 1f;
    //     SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    // }
}