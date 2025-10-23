using UnityEngine;
using UnityEngine.SceneManagement;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

public class SimpleSceneButtons : MonoBehaviour
{
    public void RestartLevel()
    {
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;

        #if ENABLE_INPUT_SYSTEM
        var pi = FindObjectOfType<PlayerInput>();
        if (pi && pi.actions != null)
        {
            var player = pi.actions.FindActionMap("Player", false);
            if (player != null) pi.SwitchCurrentActionMap("Player");
        }
        #endif

        Scene current = SceneManager.GetActiveScene();
        SceneManager.LoadScene(current.name);
    }

    public void ReturnToMainMenu(string mainMenuScene)
    {
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;
        SceneManager.LoadScene(mainMenuScene);
    }

    public void QuitGame()
    {
        #if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
        #else
        Application.Quit();
        #endif
    }
}
