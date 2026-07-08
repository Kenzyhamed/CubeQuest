using Unity.Netcode;
using UnityEngine;

public class ConditionManager : NetworkBehaviour
{
    public static ConditionManager Instance;

    [Header("References")]
    public GameManager gameManager;


    [Header("Sounds")]

    private string[] levelColorA = { "none", "none", "none", "red", "blue", "blue", "red", "green", "red", "green" };
    private string[] levelColorB1 = { "none", "none", "none", "green", "blue", "green", "green", "green", "red", "blue" };
    private string[] levelColorB2 = { "none", "none", "none", "blue", "blue", "green", "green", "green", "blue", "blue" };
    private string[] levelmeshA = { "none", "none", "none", "cube", "cube", "cube", "sphere", "cylinder", "cylinder", "sphere" };
    private string[] levelmeshB1 = { "none", "none", "none", "cube", "cube", "cube", "cylinder", "cylinder", "sphere", "sphere" };
    private string[] levelmeshB2 = { "none", "none", "none", "cube", "cube", "cube", "cylinder", "sphere", "sphere", "cylinder" };
    private string[] levelLettersA = { "M", "E", "P", "K", "N", "L", "F", "R", "P", "M" };
    private string[] levelLettersB1 = { "E", "K", "M", "F", "P", "L", "F", "R", "P", "N" };
    private string[] levelLettersB2 = { "K", "L", "N", "F", "N", "L", "R", "E", "P", "M" };

    // Syncs condition across both headsets

    void Start()
    {
        SetCondition("A");
    }

    public void setA()
    {
        if (!IsHost) return; // guest cant trigger this
        //currentCondition.Value = 0;
    }

    public void setB1()
    {
        if (!IsHost) return;
       // currentCondition.Value = 1;
    }

    public void setB2()
    {
        if (!IsHost) return;
        //currentCondition.Value = 2;
    }

    public void SetCondition(string condition)
    {
        string[] levelLetters = {};
        string[] levelColors = {};
        string[] levelMesh = {};
        if (condition == "A"){
            levelLetters = levelLettersA;
            levelMesh = levelmeshA;
            levelColors = levelColorA;}
        else if (condition == "B1"){
            levelLetters = levelLettersB1;
            levelMesh = levelmeshB1;
            levelColors = levelColorB1;}
        else{
            levelLetters = levelLettersB2;
            levelMesh = levelmeshB2;
            levelColors = levelColorB2;}

        gameManager.SetCondition(condition, levelLetters, levelColors, levelMesh);
    }
}