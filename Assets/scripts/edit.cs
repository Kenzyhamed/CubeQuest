using UnityEngine;

public class ScaleWatcher : MonoBehaviour
{
    private Vector3 lastScale;

    void Update()
    {
        if (transform.localScale != lastScale)
        {
            Debug.Log("Scale changed to: " + transform.localScale);
            Debug.Log(new System.Diagnostics.StackTrace().ToString());
            lastScale = transform.localScale;
        }
    }
}