using UnityEngine;
using System.Collections;

public class RegionCue : MonoBehaviour
{
    public GameObject slotHighlight;
    public float pulseDuration = 0.5f;

    public void PulseSlot()
    {
        Debug.Log("PulseSlot called");
        if (slotHighlight != null)
            StartCoroutine(Pulse(slotHighlight));
        else
            Debug.LogWarning("slotHighlight is null!");
    }

    IEnumerator Pulse(GameObject target)
    {
        target.SetActive(false);
        yield return new WaitForSeconds(0.1f);
        target.SetActive(true);
        yield return new WaitForSeconds(pulseDuration);
        target.SetActive(false);
        yield return new WaitForSeconds(0.1f);
        target.SetActive(true);

        
    }
}