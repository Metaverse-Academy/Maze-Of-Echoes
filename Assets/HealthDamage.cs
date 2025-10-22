using UnityEngine;

public class HealthDamage : MonoBehaviour
{



  public int maxHealth = 100;
    public int currentHealth;
    public GameObject gameOverPanel; // حط الـ Panel هنا من الـ Inspector

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
        Time.timeScale = 0f; // يوقف اللعبة
        if (gameOverPanel != null)
            gameOverPanel.SetActive(true);
    }

    // public void RestartGame()
    // {
    //     Time.timeScale = 1f;
    //     SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    // }


}
