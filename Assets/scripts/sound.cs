using UnityEngine;
using System.Collections;

public class SoundStateMachine : MonoBehaviour
{
    public enum State
    {
        Idle,
        FindCycle,
        Grabbed,
        ColorCycle,
        ShapeCycle,
        PlaceCycle,
        Done
    }

    [Header("References")]
    public AudioSource audioSource;
    public GameManager gamemanager;
    public int currentLevel => gamemanager.currentLevel.Value;

    [Header("Find/Reach the cube - general (not letter-specific, mixed into every pool)")]
    public AudioClip[] findGeneral;

    [Header("Find/Reach the cube - one pool per letter (mix of find + reach lines)")]
    public AudioClip[] LetterM;
    public AudioClip[] LetterN;
    public AudioClip[] LetterL;
    public AudioClip[] LetterK;
    public AudioClip[] LetterF;
    public AudioClip[] LetterE;
    public AudioClip[] LetterR;
    public AudioClip[] LetterP;

    [Header("Place on slot")]
    public AudioClip[] placeOnSlot;

    [Header("Success")]
    public AudioClip[] correct;

    [Header("1-3 Sounds")]
    public AudioClip introL;

    [Header("4-6 Sounds")]
    public AudioClip introC;

    [Header("Color hits")]
    public AudioClip[] hitRed;
    public AudioClip[] hitBlue;
    public AudioClip[] hitGreen;

    [Header("7-10 Sounds")]
    public AudioClip introS;

    [Header("Shape hits")]
    public AudioClip[] hitSphere;
    public AudioClip[] hitCylinder;

    [Header("Wrong feedback")]
    public AudioClip[] wrongColor;
    public AudioClip[] wrongLetter;
    public AudioClip[] wrongShape;

    [HideInInspector] public string targetLetter;
    [HideInInspector] public string targetColor;
    [HideInInspector] public string targetShape;

    public float threshold = 6f;

    State _currentState;
    Coroutine _activeCoroutine;

    bool ConditionAActive => gamemanager != null && gamemanager.isA.Value;

    AudioClip PickRandom(AudioClip[] clips)
    {
        if (clips == null || clips.Length == 0) return null;
        return clips[Random.Range(0, clips.Length)];
    }

    AudioClip PickCombined(AudioClip[] specific, AudioClip[] general)
    {
        int specificLen = specific?.Length ?? 0;
        int generalLen = general?.Length ?? 0;
        int total = specificLen + generalLen;
        if (total == 0) return null;

        int index = Random.Range(0, total);
        return index < specificLen ? specific[index] : general[index - specificLen];
    }

    AudioClip GetFindClip()
    {
        switch (targetLetter.ToUpper())
        {
            case "M": return PickCombined(LetterM, findGeneral);
            case "N": return PickCombined(LetterN, findGeneral);
            case "L": return PickCombined(LetterL, findGeneral);
            case "K": return PickCombined(LetterK, findGeneral);
            case "F": return PickCombined(LetterF, findGeneral);
            case "E": return PickCombined(LetterE, findGeneral);
            case "R": return PickCombined(LetterR, findGeneral);
            case "P": return PickCombined(LetterP, findGeneral);
            default:  return PickRandom(findGeneral);
        }
    }

    AudioClip GetColorClip()
    {
        switch (gamemanager.levelColors[currentLevel].ToLower())
        {
            case "red":   return PickRandom(hitRed);
            case "blue":  return PickRandom(hitBlue);
            case "green": return PickRandom(hitGreen);
            default:      return null;
        }
    }

    AudioClip GetShapeClip()
    {
        switch (gamemanager.levelMesh[currentLevel].ToLower())
        {
            case "sphere":   return PickRandom(hitSphere);
            case "cylinder": return PickRandom(hitCylinder);
            default:         return null;
        }
    }

    float WaitTime(AudioClip clip)
    {
        return Mathf.Max(0, threshold - (clip != null ? clip.length : 0));
    }

    public void StartLevel(int levelIndex)
    {
        if (!ConditionAActive) return;

        SetState(State.Idle);

        if (levelIndex < 3)
            _activeCoroutine = StartCoroutine(IntroThenFind(introL));
        else if (levelIndex < 6)
            _activeCoroutine = StartCoroutine(IntroThenFind(introC));
        else
            _activeCoroutine = StartCoroutine(IntroThenFind(introS));
    }

    void SetState(State newState)
    {
        _currentState = newState;
        if (_activeCoroutine != null)
        {
            StopCoroutine(_activeCoroutine);
            _activeCoroutine = null;
        }
        if (audioSource != null)
            audioSource.Stop();
    }

    IEnumerator PlayAndWait(AudioClip clip)
    {
        if (audioSource == null || clip == null) yield break;
        audioSource.PlayOneShot(clip);
        yield return new WaitForSeconds(clip.length);
    }

    public void PlayIntroOnly(int levelIndex)
    {
        SetState(State.Idle);
        AudioClip intro = levelIndex < 3 ? introL : levelIndex < 6 ? introC : introS;
        _activeCoroutine = StartCoroutine(PlayAndWait(intro));
    }

    public void StopAudio()
    {
        SetState(State.Idle);
    }

    IEnumerator IntroThenFind(AudioClip intro)
    {
        yield return StartCoroutine(PlayAndWait(intro));
        SetState(State.FindCycle);
        _activeCoroutine = StartCoroutine(FindCycleLoop());
    }

    IEnumerator FindCycleLoop()
    {
        while (_currentState == State.FindCycle)
        {
            AudioClip clip = GetFindClip();
            yield return StartCoroutine(PlayAndWait(clip));
            if (_currentState != State.FindCycle) yield break;
            yield return new WaitForSeconds(WaitTime(clip));
        }
    }

    IEnumerator ColorCycleLoop()
    {
        while (_currentState == State.ColorCycle)
        {
            AudioClip clip = GetColorClip();
            yield return StartCoroutine(PlayAndWait(clip));
            if (_currentState != State.ColorCycle) yield break;
            yield return new WaitForSeconds(WaitTime(clip));
        }
    }

    IEnumerator ShapeCycleLoop()
    {
        while (_currentState == State.ShapeCycle)
        {
            AudioClip clip = GetShapeClip();
            yield return StartCoroutine(PlayAndWait(clip));
            if (_currentState != State.ShapeCycle) yield break;
            yield return new WaitForSeconds(WaitTime(clip));
        }
    }

    IEnumerator PlaceCycleLoop()
    {
        while (_currentState == State.PlaceCycle)
        {
            AudioClip clip = PickRandom(placeOnSlot);
            yield return StartCoroutine(PlayAndWait(clip));
            if (_currentState != State.PlaceCycle) yield break;
            yield return new WaitForSeconds(WaitTime(clip));
        }
    }

    public void OnCorrectCubeGrabbed()
    {
        if (!ConditionAActive) return;
        if (_currentState == State.Done) return;
        SetState(State.Grabbed);

        if (currentLevel < 3)
            _activeCoroutine = StartCoroutine(GrabbedToPlace());
        else
            _activeCoroutine = StartCoroutine(GrabbedToColor());
    }

    IEnumerator GrabbedToPlace()
    {
        yield return new WaitForSeconds(threshold);
        if (_currentState != State.Grabbed) yield break;
        SetState(State.PlaceCycle);
        _activeCoroutine = StartCoroutine(PlaceCycleLoop());
    }

    IEnumerator GrabbedToColor()
    {
        yield return new WaitForSeconds(threshold);
        if (_currentState != State.Grabbed) yield break;
        SetState(State.ColorCycle);
        _activeCoroutine = StartCoroutine(ColorCycleLoop());
    }

    public void OnCorrectColorHit()
    {
        if (!ConditionAActive) return;
        if (currentLevel < 6)
        {
            SetState(State.PlaceCycle);
            _activeCoroutine = StartCoroutine(ColorCorrectToPlace());
        }
        else
        {
            SetState(State.ShapeCycle);
            _activeCoroutine = StartCoroutine(ColorCorrectToShape());
        }
    }

    IEnumerator ColorCorrectToPlace()
    {
        yield return new WaitForSeconds(threshold);
        if (_currentState != State.PlaceCycle) yield break;
        _activeCoroutine = StartCoroutine(PlaceCycleLoop());
    }

    IEnumerator ColorCorrectToShape()
    {
        yield return new WaitForSeconds(threshold);
        if (_currentState != State.ShapeCycle) yield break;
        _activeCoroutine = StartCoroutine(ShapeCycleLoop());
    }

    public void OnWrongColorHit()
    {
        if (!ConditionAActive) return;
        SetState(State.ColorCycle);
        _activeCoroutine = StartCoroutine(WrongColorThenCycle());
    }

    IEnumerator WrongColorThenCycle()
    {
        yield return StartCoroutine(PlayAndWait(PickRandom(wrongColor)));
        if (_currentState != State.ColorCycle) yield break;
        _activeCoroutine = StartCoroutine(ColorCycleLoop());
    }

    public void OnCorrectShapeHit()
    {
        if (!ConditionAActive) return;
        SetState(State.PlaceCycle);
        _activeCoroutine = StartCoroutine(ShapeCorrectToPlace());
    }

    IEnumerator ShapeCorrectToPlace()
    {
        yield return new WaitForSeconds(threshold);
        if (_currentState != State.PlaceCycle) yield break;
        _activeCoroutine = StartCoroutine(PlaceCycleLoop());
    }

    public void OnWrongShapeHit()
    {
        if (!ConditionAActive) return;
        SetState(State.ShapeCycle);
        _activeCoroutine = StartCoroutine(WrongShapeThenCycle());
    }

    IEnumerator WrongShapeThenCycle()
    {
        yield return StartCoroutine(PlayAndWait(PickRandom(wrongShape)));
        if (_currentState != State.ShapeCycle) yield break;
        _activeCoroutine = StartCoroutine(ShapeCycleLoop());
    }

    public void OnCorrectPlaced()
    {
        if (!ConditionAActive) return;
        SetState(State.Done);
        _activeCoroutine = StartCoroutine(PlayAndWait(PickRandom(correct)));
    }

    public void OnWrongPlaced()
    {
        if (!ConditionAActive) return;
        SetState(State.FindCycle);
        _activeCoroutine = StartCoroutine(WrongPlacedThenFind());
    }

    IEnumerator WrongPlacedThenFind()
    {
        yield return StartCoroutine(PlayAndWait(PickRandom(wrongLetter)));
        if (_currentState != State.FindCycle) yield break;
        yield return StartCoroutine(PlayAndWait(GetFindClip()));
        _activeCoroutine = StartCoroutine(FindCycleLoop());
    }

    public void OnCubeReleased()
    {
        if (!ConditionAActive) return;
        if (_currentState == State.Grabbed    ||
            _currentState == State.PlaceCycle ||
            _currentState == State.ColorCycle ||
            _currentState == State.ShapeCycle)
        {
            SetState(State.FindCycle);
            _activeCoroutine = StartCoroutine(FindCycleLoop());
        }
    }
}