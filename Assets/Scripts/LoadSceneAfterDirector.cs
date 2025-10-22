using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.SceneManagement;

public class LoadSceneAfterDirector : MonoBehaviour
{
    [SerializeField] private PlayableDirector director;
    [Tooltip("Exact scene name (must be in Build Settings).")]
    [SerializeField] private string sceneToLoad = "Level1";
    [Tooltip("Optional delay after the cutscene ends.")]
    [SerializeField] private float delay = 0.5f;

    void Reset()
    {
        director = GetComponent<PlayableDirector>();
    }

    void OnEnable()
    {
        if (director != null) director.stopped += OnCutsceneFinished;
    }

    void OnDisable()
    {
        if (director != null) director.stopped -= OnCutsceneFinished;
    }

    private void OnCutsceneFinished(PlayableDirector d)
    {
        if (!gameObject.activeInHierarchy) return;
        StartCoroutine(LoadAfterDelay());
    }

    System.Collections.IEnumerator LoadAfterDelay()
    {
        if (delay > 0f) yield return new WaitForSeconds(delay);
        SceneManager.LoadScene(sceneToLoad, LoadSceneMode.Single);
    }
}
