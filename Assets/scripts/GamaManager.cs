using UnityEngine;
using TMPro;
using System.Collections;

public class GameManager : MonoBehaviour
{
    public TrialManager trialmanager;
    public Transform slot;
    public TextMeshProUGUI targetLetterText;
    public TextMeshProUGUI resultText;

    public AudioSource audioSource;
    private AudioClip Intro;
    private AudioClip correctSound;
    private AudioClip wrongSound;



    private string[] levelLetters;
    private int currentLevel = 0;

    public void SetCondition(string condition, string[] levelLettersA)
    {
        currentLevel = 0;
        levelLetters = levelLettersA;

        LoadLevel(currentLevel);
    }

    public void SetSounds(AudioClip Introsound, AudioClip correct, AudioClip wrong)
    {
        correctSound = correct;
        wrongSound = wrong;
        Intro = Introsound;
    }

    void LoadLevel(int levelIndex)
    {
        TrialManager.Instance?.StartTrial(levelLetters[levelIndex]);
        if (levelLetters == null || levelIndex >= levelLetters.Length)
        {
            if (resultText != null) resultText.text = "All Done!";
            return;
        }
        if (targetLetterText != null) targetLetterText.text = levelLetters[levelIndex];
        if (resultText != null) resultText.text = "Grab the letter that is displayed below";

        if (audioSource && Intro != null) audioSource.PlayOneShot(Intro);
        Debug.Log($"Loaded level {levelIndex + 1}, correct letter: {levelLetters[levelIndex]}");
    }

    public bool IsCorrectLetter(string letter)
    {
        return letter == targetLetterText.text.Trim();
    }

    public void OnCorrect()
    {
        if (resultText != null) resultText.text = "Well Done!";
        if (audioSource && correctSound) audioSource.PlayOneShot(correctSound);
        StartCoroutine(NextLevelAfterDelay());
    }

    public void OnWrong()
    {
        if (resultText != null) resultText.text = "Try Again";
        if (audioSource && wrongSound) audioSource.PlayOneShot(wrongSound);
    }

    public void PlayPrompt(string message)
    {
        Debug.Log($"Audio Prompt: {message}");
    }

    IEnumerator NextLevelAfterDelay()
    {
        yield return new WaitForSeconds(2f);
        currentLevel++;
        LoadLevel(currentLevel);
    }
}