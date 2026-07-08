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
    public int currentLevel => gamemanager.currentLevel;

    [Header("Shared Sounds")]
    public AudioClip findLetter;
    public AudioClip FindM;
    public AudioClip FindN;
    public AudioClip FindL;
    public AudioClip FindK;
    public AudioClip FindF;
    public AudioClip FindE;
    public AudioClip FindR;
    public AudioClip FindP;
    public AudioClip reachForLetter;
    public AudioClip ReachM;
    public AudioClip ReachN;
    public AudioClip ReachL;
    public AudioClip ReachK;
    public AudioClip ReachF;
    public AudioClip ReachE;
    public AudioClip ReachR;
    public AudioClip ReachP;
    public AudioClip placeOnSlot;
    public AudioClip wellDone;
    public AudioClip tryAgain;

    [Header("1-3 Sounds")]
    public AudioClip introL;

    [Header("4-6 Sounds")]
    public AudioClip introC;
    public AudioClip hitTargetRed;
    public AudioClip hitTargetBlue;
    public AudioClip hitTargetGreen;
    public AudioClip wrongColor;

    [Header("7-10 Sounds")]
    public AudioClip introS;
    public AudioClip hitTargetSphere;
    public AudioClip hitTargetCyl;
    public AudioClip wrongShape;

    [HideInInspector] public string targetLetter;
    [HideInInspector] public string targetColor;
    [HideInInspector] public string targetShape;

    public float threshold = 6f;

    State _currentState;
    Coroutine _activeCoroutine;

    // ── Letter-specific clips ─────────────────────────────────────────────
    AudioClip GetFindClip()
    {
        switch (targetLetter.ToUpper())
        {
            case "M": return FindM != null ? FindM : findLetter;
            case "N": return FindN != null ? FindN : findLetter;
            case "L": return FindL != null ? FindL : findLetter;
            case "K": return FindK != null ? FindK : findLetter;
            case "F": return FindF != null ? FindF : findLetter;
            case "E": return FindE != null ? FindE : findLetter;
            case "R": return FindR != null ? FindR : findLetter;
            case "P": return FindP != null ? FindP : findLetter;
            default:  return findLetter;
        }
    }

    AudioClip GetReachClip()
    {
        switch (targetLetter.ToUpper())
        {
            case "M": return ReachM != null ? ReachM : reachForLetter;
            case "N": return ReachN != null ? ReachN : reachForLetter;
            case "L": return ReachL != null ? ReachL : reachForLetter;
            case "K": return ReachK != null ? ReachK : reachForLetter;
            case "F": return ReachF != null ? ReachF : reachForLetter;
            case "E": return ReachE != null ? ReachE : reachForLetter;
            case "R": return ReachR != null ? ReachR : reachForLetter;
            case "P": return ReachP != null ? ReachP : reachForLetter;
            default:  return reachForLetter;
        }
    }

    AudioClip GetColorClip()
    {
        switch (gamemanager.levelColors[currentLevel].ToLower())
        {
            case "red":   return hitTargetRed;
            case "blue":  return hitTargetBlue;
            case "green": return hitTargetGreen;
            default:      return null;
        }
    }

    AudioClip GetShapeClip()
    {
        switch (gamemanager.levelMesh[currentLevel].ToLower())
        {
            case "sphere":   return hitTargetSphere;
            case "cylinder": return hitTargetCyl;
            default:         return null;
        }
    }

    float WaitTime(AudioClip clip)
    {
        return Mathf.Max(0, threshold - (clip != null ? clip.length : 0));
    }

    // ── StartLevel ────────────────────────────────────────────────────────
    public void StartLevel(int levelIndex)
    {
        SetState(State.Idle);

        if (levelIndex < 3)
            _activeCoroutine = StartCoroutine(IntroThenFind(introL));
        else if (levelIndex < 6)
            _activeCoroutine = StartCoroutine(IntroThenFind(introC));
        else
            _activeCoroutine = StartCoroutine(IntroThenFind(introS));
    }

    // ── State setter ──────────────────────────────────────────────────────
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

    // ── Play and wait ─────────────────────────────────────────────────────
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

    // ── Intro → FindCycle ─────────────────────────────────────────────────
    IEnumerator IntroThenFind(AudioClip intro)
    {
        yield return StartCoroutine(PlayAndWait(intro));
        SetState(State.FindCycle);
        _activeCoroutine = StartCoroutine(FindCycleLoop());
    }

    // ── Find cycle ────────────────────────────────────────────────────────
    IEnumerator FindCycleLoop()
    {
        int step = 0;
        while (_currentState == State.FindCycle)
        {
            AudioClip clip = (step % 4 < 2) ? GetFindClip() : GetReachClip();
            yield return new WaitForSeconds(WaitTime(clip));
            if (_currentState != State.FindCycle) yield break;
            yield return StartCoroutine(PlayAndWait(clip));
            step++;
        }
    }

    // ── Color cycle ───────────────────────────────────────────────────────
    IEnumerator ColorCycleLoop()
    {
        while (_currentState == State.ColorCycle)
        {
            AudioClip clip = GetColorClip();
            yield return new WaitForSeconds(WaitTime(clip));
            if (_currentState != State.ColorCycle) yield break;
            yield return StartCoroutine(PlayAndWait(clip));
        }
    }

    // ── Shape cycle ───────────────────────────────────────────────────────
    IEnumerator ShapeCycleLoop()
    {
        while (_currentState == State.ShapeCycle)
        {
            AudioClip clip = GetShapeClip();
            yield return new WaitForSeconds(WaitTime(clip));
            if (_currentState != State.ShapeCycle) yield break;
            yield return StartCoroutine(PlayAndWait(clip));
        }
    }

    // ── Place cycle ───────────────────────────────────────────────────────
    IEnumerator PlaceCycleLoop()
    {
        while (_currentState == State.PlaceCycle)
        {
            yield return new WaitForSeconds(WaitTime(placeOnSlot));
            if (_currentState != State.PlaceCycle) yield break;
            yield return StartCoroutine(PlayAndWait(placeOnSlot));
        }
    }

    // ── Grabbed routing ───────────────────────────────────────────────────
    public void OnCorrectCubeGrabbed()
    {
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

    // ── Wrong cube ────────────────────────────────────────────────────────

    // ── Color hit ─────────────────────────────────────────────────────────
    public void OnCorrectColorHit()
    {
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
        SetState(State.ColorCycle);
        _activeCoroutine = StartCoroutine(WrongColorThenCycle());
    }

    IEnumerator WrongColorThenCycle()
    {
        yield return StartCoroutine(PlayAndWait(wrongColor));
        if (_currentState != State.ColorCycle) yield break;
        _activeCoroutine = StartCoroutine(ColorCycleLoop());
    }

    // ── Shape hit ─────────────────────────────────────────────────────────
    public void OnCorrectShapeHit()
    {
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
        SetState(State.ShapeCycle);
        _activeCoroutine = StartCoroutine(WrongShapeThenCycle());
    }

    IEnumerator WrongShapeThenCycle()
    {
        yield return StartCoroutine(PlayAndWait(wrongShape));
        if (_currentState != State.ShapeCycle) yield break;
        _activeCoroutine = StartCoroutine(ShapeCycleLoop());
    }

    // ── Placed ────────────────────────────────────────────────────────────
    public void OnCorrectPlaced()
    {
        SetState(State.Done);
        _activeCoroutine = StartCoroutine(PlayAndWait(wellDone));
    }

    public void OnWrongPlaced()
    {
        SetState(State.FindCycle);
        _activeCoroutine = StartCoroutine(WrongPlacedThenFind());
    }

    IEnumerator WrongPlacedThenFind()
    {
        yield return StartCoroutine(PlayAndWait(tryAgain));
        if (_currentState != State.FindCycle) yield break;
        yield return StartCoroutine(PlayAndWait(GetFindClip()));
        _activeCoroutine = StartCoroutine(FindCycleLoop());
    }

    // ── Released ──────────────────────────────────────────────────────────
    public void OnCubeReleased()
    {
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