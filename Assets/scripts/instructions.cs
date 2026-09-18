using UnityEngine;
using System.Collections;

public class IntroTimelineController : MonoBehaviour
{
    [Header("Audio")]
    public AudioSource audioSource;
    public AudioClip introVoiceover;
    public AudioClip findMSound;

    [Header("Shelves")]
    public GameObject[] shelves;

    [Header("Letter Cubes (for condition A bounce)")]
    public GameObject[] letterCubes;

    [Header("Disks")]
    public GameObject[] disks;

    [Header("Shapes")]
    public GameObject[] shapes;

    [Header("Target Display")]
    public GameObject targetLetterDisplay;
    public GameObject targetShapeObject;
    public Mesh cubeMesh;
    public Mesh cylinderMesh;

    [Header("Slot")]
    public GameObject yellowSlot;

    [Header("M Cube")]
    public GameObject mCube;
    public float moveDuration = 1.5f;

    [Header("Demo Targets (for hand pointing / m cube movement)")]
    public Transform blueDiskTransform;
    public Transform cylinderShapeTransform;

    [Header("Guide Hand (single hand - animator bools driven directly)")]
    public Animator handAnimator;

    [Header("Blink Settings")]
    public float blinkInterval = 0.2f;
    public float slowBlinkInterval = 0.4f;

    public TargetManager targetmanager;

    Coroutine _shelfBlink, _cubeBounce, _diskBlink, _shapeBlink, _slotBlink, _letterBlink, _targetBlink, _mBounce;
    Vector3 _mCubeStartPos;


    public void Play()
    {
        if (audioSource != null && introVoiceover != null)
            audioSource.PlayOneShot(introVoiceover);

        if (mCube != null)
            _mCubeStartPos = mCube.transform.position;

        StartCoroutine(RunTimeline());
    }

    IEnumerator RunTimeline()
    {
        // t=6 - shelves flash slowly
        yield return new WaitForSeconds(6f);
        _shelfBlink = StartCoroutine(BlinkLoop(shelves, slowBlinkInterval));

        // t=10 - shelves stop, cubes jump up and down (condition A)
        yield return new WaitForSeconds(4f);
        StopBlink(_shelfBlink, shelves);
        _cubeBounce = StartCoroutine(BounceAllLoop(letterCubes, 4f));

        // t=14 - cubes stop, disks flash slowly
        yield return new WaitForSeconds(4f);
        if (_cubeBounce != null) StopCoroutine(_cubeBounce);
        ResetPositions(letterCubes);
        _diskBlink = StartCoroutine(BlinkLoop(disks, slowBlinkInterval));

        // t=18 - disks stop, shapes flash slowly
        yield return new WaitForSeconds(4f);
        StopBlink(_diskBlink, disks);
        _shapeBlink = StartCoroutine(BlinkLoop(shapes, slowBlinkInterval));

        // t=23 - shapes stop, yellow slot flashes slowly
        yield return new WaitForSeconds(5f);
        StopBlink(_shapeBlink, shapes);
        _slotBlink = StartCoroutine(BlinkLoop(new[] { yellowSlot }, slowBlinkInterval));

        // t=30 - slot stops, target letter flashes
        yield return new WaitForSeconds(7f);
        StopBlink(_slotBlink, new[] { yellowSlot });
        _letterBlink = StartCoroutine(BlinkLoop(new[] { targetLetterDisplay }, slowBlinkInterval));

        // t=34 - letter stops, target mesh becomes cube + blue, flashes slowly
        yield return new WaitForSeconds(4f);
        StopBlink(_letterBlink, new[] { targetLetterDisplay });
        SetTargetMesh(cubeMesh);
        SetTargetColor(Color.blue);
        _targetBlink = StartCoroutine(BlinkLoop(new[] { targetShapeObject }, slowBlinkInterval));

        // t=44 - target mesh changes to cylinder
        yield return new WaitForSeconds(10f);
        StopBlink(_targetBlink, new[] { targetShapeObject });
        SetTargetMesh(cylinderMesh);

        // t=53 - m cube jumps up and down
        yield return new WaitForSeconds(9f);
        _mBounce = StartCoroutine(BounceLoop(mCube, 3f));

        // t=65 (1:05) - m cube moves to blue disk and hits it
        yield return new WaitForSeconds(12f);
        yield return StartCoroutine(MoveCube(mCube, blueDiskTransform.position, moveDuration));

        // t=75 (1:15) - m cube moves to cylinder
        yield return new WaitForSeconds(10f - moveDuration);
        yield return StartCoroutine(MoveCube(mCube, cylinderShapeTransform.position, moveDuration));

        // t=89 (1:29) - m cube moves to slot
        yield return new WaitForSeconds(14f - moveDuration);
        yield return StartCoroutine(MoveCube(mCube, yellowSlot.transform.position, moveDuration));

        // t=94 (1:34) - m cube returns to the yellow slot, color/shape reset,
        // bounces again, FindM sound plays
        yield return new WaitForSeconds(5f - moveDuration);
        yield return StartCoroutine(MoveCube(mCube, yellowSlot.transform.position, moveDuration));
        SetTargetColor(Color.white);
        SetTargetMesh(cubeMesh);
        _mBounce = StartCoroutine(BounceLoop(mCube, 3f));
        if (audioSource != null && findMSound != null)
            audioSource.PlayOneShot(findMSound);

        // t=103 (1:43) - pointing hand: m cube -> disk -> cylinder -> slot, 1.5s each
        yield return new WaitForSeconds(9f);
        yield return StartCoroutine(HandDemo(isB2: false));

        // t=114 (1:54) - same sequence, b2 hand style
        yield return new WaitForSeconds(5f);
        yield return StartCoroutine(HandDemo(isB2: true));

        // timeline complete
        targetmanager.SetTargets();
    }

    // ── Blink helpers ────────────────────────────────────────────────────
    IEnumerator BlinkLoop(GameObject[] objects, float interval)
    {
        while (true)
        {
            SetActiveAll(objects, false);
            yield return new WaitForSeconds(interval);
            SetActiveAll(objects, true);
            yield return new WaitForSeconds(interval);
        }
    }

    void StopBlink(Coroutine blink, GameObject[] objects)
    {
        if (blink != null) StopCoroutine(blink);
        SetActiveAll(objects, true);
    }

    void SetActiveAll(GameObject[] objects, bool active)
    {
        if (objects == null) return;
        foreach (GameObject go in objects)
        {
            if (go != null) go.SetActive(active);
        }
    }

    // ── Bounce helpers ──────────────────────────────────────────────────
    IEnumerator BounceLoop(GameObject cube, float duration)
    {
        if (cube == null) yield break;

        Vector3 basePos = cube.transform.position;
        float jumpHeight = 0.05f;
        float jumpSpeed = 2f;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            float yOffset = Mathf.Abs(Mathf.Sin(Time.time * jumpSpeed)) * jumpHeight;
            cube.transform.position = basePos + Vector3.up * yOffset;
            elapsed += Time.deltaTime;
            yield return null;
        }

        cube.transform.position = basePos;
    }

    IEnumerator BounceAllLoop(GameObject[] cubes, float duration)
    {
        if (cubes == null || cubes.Length == 0) yield break;

        Vector3[] basePositions = new Vector3[cubes.Length];
        for (int i = 0; i < cubes.Length; i++)
            if (cubes[i] != null) basePositions[i] = cubes[i].transform.position;

        float jumpHeight = 0.05f;
        float jumpSpeed = 2f;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            float yOffset = Mathf.Abs(Mathf.Sin(Time.time * jumpSpeed)) * jumpHeight;
            for (int i = 0; i < cubes.Length; i++)
                if (cubes[i] != null) cubes[i].transform.position = basePositions[i] + Vector3.up * yOffset;
            elapsed += Time.deltaTime;
            yield return null;
        }

        ResetPositionsTo(cubes, basePositions);
    }

    void ResetPositions(GameObject[] cubes)
    {
        // no stored base positions available here; left as a hook if cubes
        // need to be snapped back after StopCoroutine cuts BounceAllLoop early
    }

    void ResetPositionsTo(GameObject[] cubes, Vector3[] basePositions)
    {
        for (int i = 0; i < cubes.Length; i++)
            if (cubes[i] != null) cubes[i].transform.position = basePositions[i];
    }

    // ── Movement helper ─────────────────────────────────────────────────
    IEnumerator MoveCube(GameObject cube, Vector3 targetPosition, float duration)
    {
        if (cube == null) yield break;

        Vector3 start = cube.transform.position;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            cube.transform.position = Vector3.Lerp(start, targetPosition, elapsed / duration);
            elapsed += Time.deltaTime;
            yield return null;
        }

        cube.transform.position = targetPosition;
    }

    // ── Target shape helpers ────────────────────────────────────────────
    void SetTargetColor(Color color)
    {
        if (targetShapeObject == null) return;
        MeshRenderer mr = targetShapeObject.GetComponent<MeshRenderer>();
        if (mr != null) mr.material.color = color;
    }

    void SetTargetMesh(Mesh mesh)
    {
        if (targetShapeObject == null || mesh == null) return;
        MeshFilter mf = targetShapeObject.GetComponent<MeshFilter>();
        if (mf != null) mf.mesh = mesh;
    }

    // ── Hand demo ────────────────────────────────────────────────────────
    IEnumerator HandDemo(bool isB2)
    {
        const float showDuration = 1.5f;

        ShowHand(mCube.transform.position,
            isPointing: !isB2, isGrab: isB2, isKeepHold: false, isRelease: false);
        yield return new WaitForSeconds(showDuration);
        HideHand();

        ShowHand(blueDiskTransform.position,
            isPointing: !isB2, isGrab: false, isKeepHold: isB2, isRelease: false);
        yield return new WaitForSeconds(showDuration);
        HideHand();

        ShowHand(cylinderShapeTransform.position,
            isPointing: !isB2, isGrab: false, isKeepHold: isB2, isRelease: false);
        yield return new WaitForSeconds(showDuration);
        HideHand();

        ShowHand(yellowSlot.transform.position,
            isPointing: !isB2, isGrab: false, isKeepHold: false, isRelease: isB2);
        yield return new WaitForSeconds(showDuration);
        HideHand();
    }

    void ShowHand(Vector3 targetPosition, bool isPointing, bool isGrab, bool isKeepHold, bool isRelease)
    {
        SetHandVisible(true);

        if (handAnimator != null)
        {
            Vector3 desired = new Vector3(targetPosition.x, targetPosition.y - 0.1f, targetPosition.z - 0.15f);
            handAnimator.transform.position = desired;

            handAnimator.SetBool("isPointing", isPointing);
            handAnimator.SetBool("isGrab", isGrab);
            handAnimator.SetBool("isRelease", isRelease);
            handAnimator.SetBool("isKeepHold", isKeepHold);
        }
    }

    void HideHand()
    {
        if (handAnimator != null)
        {
            handAnimator.SetBool("isPointing", false);
            handAnimator.SetBool("isGrab", false);
            handAnimator.SetBool("isRelease", false);
            handAnimator.SetBool("isKeepHold", false);
        }

        SetHandVisible(false);
    }

    void SetHandVisible(bool visible)
    {
        if (handAnimator == null) return;
        handAnimator.gameObject.SetActive(visible);
    }
}