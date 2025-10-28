using UnityEngine;

public class SpatialAudioSetup : MonoBehaviour
{
    private AudioSource audioSource;

    void Start()
    {
        audioSource = GetComponent<AudioSource>();
        
        // اجعله 3D spatial audio
        audioSource.spatialBlend = 1f; // 0 = 2D, 1 = 3D
        
        // نوع التلاشي
        audioSource.rolloffMode = AudioRolloffMode.Logarithmic;
        
        // المسافات
        audioSource.minDistance = 3f;
        audioSource.maxDistance = 100f;
    }
}