using UnityEngine;

public class ConditionManager : MonoBehaviour
{
    public static ConditionManager Instance;

    [Header("References")]
    public GameManager gameManager;
    public TrialManager trialManager;

    [Header("Sounds")]
    public AudioClip Intro;
    public AudioClip correctSound;
    public AudioClip wrongSound;
    public AudioClip FindM;    
    public AudioClip FindN;   
    public AudioClip FindL;   
    public AudioClip FindK;   
    public AudioClip FindF;   
    public AudioClip FindE;   
    public AudioClip FindR;  
    public AudioClip FindP;
    public AudioClip PlaceinSlot;  
    private string[] levelLettersA = { "M", "E", "P", "K", "N", "L", "F", "R", "P", "M" };
     private string[] levelLettersB1 = { "E", "K", "M", "F", "P", "L", "F", "R", "P", "N" };
      private string[] levelLettersB2 = { "K", "L", "N", "F", "N", "L", "R", "E", "P", "M" };

    void Awake()
    {
        Instance = this;
    }
    public void setA()
    {
        Debug.Log("POKED A");
        SetCondition("A");

    }
     public void setB1()
    {
        SetCondition("B1");
    }
    public void setB2()
    {
        SetCondition("B2");
    }
    void Start()
    {
        SetCondition("A");
    }

    public void SetCondition(string condition)
    {
        string[] levelLetters={};
        if(condition == "A")
        {
            levelLetters = levelLettersA;
        }
        else if (condition == "B1")
        {
            levelLetters= levelLettersB1;
        }
        else
        {
            levelLetters= levelLettersB2;
        }
        gameManager.SetSounds(Intro, correctSound, wrongSound);
        gameManager.SetCondition(condition, levelLetters);
        trialManager.SetSounds(condition,PlaceinSlot, FindE, FindF, FindL, FindK, FindM, FindN, FindR, FindP);

    }


}