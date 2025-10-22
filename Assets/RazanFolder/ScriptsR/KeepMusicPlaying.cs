using UnityEngine;

public class KeepMusicPlaying : MonoBehaviour
{
    private void Awake()
    {
       
        DontDestroyOnLoad(gameObject);

        
        if (FindObjectsOfType<KeepMusicPlaying>().Length > 1)
        {
            Destroy(gameObject);
        }
    }
}
