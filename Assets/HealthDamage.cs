using UnityEngine;

public class HealthDamage : MonoBehaviour
{
    public int maxHealth = 100;
    public int currentHealth;
    public GameObject gameOverPanel;        // حط الـ Panel هنا من الـ Inspector
    public AudioSource deathMusic;          // موسيقى الموت
    
    void Start()
    {
        currentHealth = maxHealth;
        
        if (gameOverPanel != null)
            gameOverPanel.SetActive(false);
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
        Time.timeScale = 0f;                // يوقف اللعبة
        
        if (gameOverPanel != null)
            gameOverPanel.SetActive(true);
        
        // تشغيل موسيقى الموت
        if (deathMusic != null)
        {
            deathMusic.ignoreListenerPause = true;  // عشان يشتغل حتى لو اللعبة واقفة
            deathMusic.Play();
        }
    }
    
    // public void RestartGame()
    // {
    //     Time.timeScale = 1f;
    //     SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    // }
}