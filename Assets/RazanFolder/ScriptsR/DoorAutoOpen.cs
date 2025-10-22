using UnityEngine;
using UnityEngine.SceneManagement;

public class DoorAutoOpen : MonoBehaviour
{
    public string nextSceneName = "NextScene"; 
    public float openAngle = 90f;              
    public float openSpeed = 2f;             
    public float delayBeforeSceneLoad = 5f;   
    private bool isOpening = false;
    private bool hasOpened = false;
    private Quaternion initialRotation;
    private Quaternion targetRotation;

    private void Start()
    {
        initialRotation = transform.rotation;
        targetRotation = Quaternion.Euler(transform.eulerAngles + new Vector3(0, openAngle, 0));
    }

    private void Update()
    {
        if (isOpening && !hasOpened)
        {
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * openSpeed);

            
            if (Quaternion.Angle(transform.rotation, targetRotation) < 1f)
            {
                hasOpened = true;
                Invoke(nameof(LoadNextScene), delayBeforeSceneLoad);
            }
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player") && !isOpening)
        {
            Debug.Log("Player touched the door! Rotating to open...");
            isOpening = true;
        }
    }

    private void LoadNextScene()
    {
        SceneManager.LoadScene(nextSceneName);
    }
}
