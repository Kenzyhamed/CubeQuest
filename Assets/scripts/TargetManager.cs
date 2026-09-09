using Unity.Netcode;
using UnityEngine;

public class TargetManager : MonoBehaviour
{
    public static TargetManager Instance;
    [Header("References")]
    public GameManager gameManager;
    [Header("Sounds")]
    private readonly string[] levelColors = {"none", "none", "none", "none", "none", "none","blue", "blue", "green", "green", "green", "blue", "blue",  "red", "blue", "blue", "red", "green", "red", "green" };
    private readonly string[] levelMesh = {"none", "none", "none", "none", "none", "none","cube", "cube", "cube","cube", "cube", "cube", "sphere", "cylinder", "cylinder", "sphere", "cylinder", "cylinder", "sphere", "sphere"};
    private readonly string[] levelLetters = {"M", "E", "P", "K", "N", "L", "F", "R", "P", "M", "E", "K", "M", "F", "P", "L", "F", "R", "P", "N"};

    void Start()
    {
        SetTargets();
    }


    public void SetTargets()
    {
        gameManager.SetTargets(levelLetters, levelColors, levelMesh);
    }
}