using UnityEngine;
public class UIAnimatorUnscaled : MonoBehaviour
{
    void OnEnable()
    {
        foreach (var a in GetComponentsInChildren<Animator>(true))
            a.updateMode = AnimatorUpdateMode.UnscaledTime;
    }
}
