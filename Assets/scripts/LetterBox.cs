using UnityEngine;
using TMPro;

public class LetterBox : MonoBehaviour
{
    [HideInInspector] public string letter;
    public TextMeshProUGUI label;

    public void Start()
    {
        if (label != null)
        {
            letter = label.text;
        }

        Debug.Log("Letter is: " + letter);
    }
}