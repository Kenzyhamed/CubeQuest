using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class TrialManager : MonoBehaviour
{
    public static TrialManager Instance;

    // ── References ────────────────────────────────────────────────────────
    [Header("References")]
    public GameManager gameManager;
    public GameObject colorslot;
    public ArmGuideCubeToItsSlot armGuide;
    public SoundStateMachine stateMachine;

    // ── Arm Guide Targets ─────────────────────────────────────────────────
    [Header("Disks")]
    public Transform redDisk;
    public Transform blueDisk;
    public Transform greenDisk;

    [Header("Shapes")]
    public Transform sphereShape;
    public Transform cylinderShape;

    // ── Blocks ────────────────────────────────────────────────────────────
    [Header("Letter Blocks")]
    public SnapToPoint[] allBlocks;

    // ── State Tracking ────────────────────────────────────────────────────
    bool _wasColorCorrect = false;
    bool _wasShapeCorrect = false;
    private string _condition = "A";
    private float _timer = 0f;
    private bool _trialActive = false;
    private bool _wasGrabbed = false;
    private bool _pulsing = false;
    private bool _jumping = false;
    private string _targetLetter = "";
    private SnapToPoint _currentTargetBlock;

    void Awake()
    {
        Instance = this;
    }

    // ── Trial Lifecycle ───────────────────────────────────────────────────
    public void StartTrial(string targetLetter, string currentcondition, int level)
    {
        ResetAllBlocks();
        _trialActive = true;
        _targetLetter = targetLetter;
        _pulsing = false;
        _condition = currentcondition;
        _jumping = false;
        _timer = 0f;
        _wasGrabbed = false;
        _wasColorCorrect = false;
        _wasShapeCorrect = false;

        StopAllCoroutines();
        if (armGuide != null) armGuide.Hide();

        if (stateMachine != null)
        {
            stateMachine.targetLetter = targetLetter;
            if (_condition == "A")
                stateMachine.StartLevel(level);
            else
                stateMachine.PlayIntroOnly(level);
        }

        // show/hide disks and shapes based on level
        if (level < 3)
        {
            SetDisksVisible(false);
            SetShapesVisible(false);
        }
        else if (level < 6)
        {
            SetDisksVisible(true);
            SetShapesVisible(false);
        }
        else
        {
            SetDisksVisible(true);
            SetShapesVisible(true);
        }
        StartCoroutine(FindTargetBlock());
    }
        void SetDisksVisible(bool visible)
    {
        if (redDisk != null) redDisk.gameObject.SetActive(visible);
        if (blueDisk != null) blueDisk.gameObject.SetActive(visible);
        if (greenDisk != null) greenDisk.gameObject.SetActive(visible);
    }

    void SetShapesVisible(bool visible)
    {
        if (sphereShape != null) sphereShape.gameObject.SetActive(visible);
        if (cylinderShape != null) cylinderShape.gameObject.SetActive(visible);
    }
    void ResetAllBlocks()
    {
        foreach (SnapToPoint block in allBlocks)
        {
            LetterBox lb = block.GetComponentInChildren<LetterBox>();
            if (lb == null) continue;
            if (lb._mf != null) lb._mf.mesh = lb.OrigMesh;
            if (lb._mr != null) lb._mr.material.color = lb.OrigColor;
        }
    }

    IEnumerator FindTargetBlock()
    {
        yield return null;

        _currentTargetBlock = null;
        foreach (SnapToPoint block in allBlocks)
        {
            LetterBox lb = block.GetComponent<LetterBox>();
            if (lb != null && lb.letter == _targetLetter)
            {
                _currentTargetBlock = block;
                break;
            }
        }

        if (_currentTargetBlock == null)
            Debug.LogWarning($"Could not find block for letter: {_targetLetter}");
    }

    public void EndTrial(bool success)
    {
        _trialActive = false;
        _pulsing = false;
        _jumping = false;
        _timer = 0f;
        _wasGrabbed = false;
        StopAllCoroutines();
        if (armGuide != null) armGuide.Hide();

        if (_currentTargetBlock != null)
        {
            _currentTargetBlock.isBeingControlled = false;
            _currentTargetBlock.transform.position = _currentTargetBlock.assignedSlot.position;
        }
    }

    // ── Update ────────────────────────────────────────────────────────────
    void Update()
    {
        if (!_trialActive) return;

        bool isGrabbed = _currentTargetBlock != null &&
                        _currentTargetBlock._grabbable != null &&
                        _currentTargetBlock._grabbable.GrabPoints != null &&
                        _currentTargetBlock._grabbable.GrabPoints.Count > 0;

        bool isOnSlot = _currentTargetBlock != null &&
                        Vector3.Distance(_currentTargetBlock.transform.position,
                        gameManager.slot.position) < 0.05f;

        LetterBox lb = _currentTargetBlock != null ? _currentTargetBlock.GetComponentInChildren<LetterBox>() : null;
        bool colorCorrect = lb != null && (gameManager.levelColors[gameManager.currentLevel].ToLower() == "none" || lb.colorName == gameManager.levelColors[gameManager.currentLevel].ToLower());
        bool shapeCorrect = lb != null && (gameManager.levelMesh[gameManager.currentLevel].ToLower() == "none" || lb.meshName.Contains(gameManager.levelMesh[gameManager.currentLevel].ToLower()));

        if (_condition == "A")
        {
            // ── Grab state change ─────────────────────────────────────────
            if (isGrabbed != _wasGrabbed)
            {
                _timer = 0f;
                _wasGrabbed = isGrabbed;

                if (isGrabbed)
                {
                    _pulsing = false;
                    _jumping = false;
                    StopCoroutine("JumpShakeLoop");

                    _wasColorCorrect = false;
                    _wasShapeCorrect = false;

                    if (stateMachine != null)
                    {
                        bool isCorrectCube = lb != null && lb.letter == _targetLetter;
                        if (isCorrectCube)
                        {
                            stateMachine.OnCorrectCubeGrabbed();
                            if (colorCorrect && gameManager.currentLevel >= 3) stateMachine.OnCorrectColorHit();
                            if (shapeCorrect && gameManager.currentLevel >= 6) stateMachine.OnCorrectShapeHit();
                        }

                    }
                }
                else
                {
                    if (stateMachine != null)
                        stateMachine.OnCubeReleased();
                }
            }


            // ── Timer ─────────────────────────────────────────────────────
            _timer += Time.deltaTime;

            if (_timer >= stateMachine.threshold)
            {
                _timer = 0f;

                if (!isGrabbed && !_jumping && !_pulsing)
                {
                    _pulsing = true;
                    _jumping = true;
                    StartCoroutine(PulseLoop());
                    StartCoroutine(JumpShakeLoop());
                }
            }
        }
        else if (_condition == "B1" || _condition == "B2")
        {
            // ── Grab state change ─────────────────────────────────────────
            if (isGrabbed != _wasGrabbed)
            {
                _timer = 0f;
                _wasGrabbed = isGrabbed;
                if (armGuide != null) armGuide.Hide();
            }

            // ── Color / Shape change while grabbed ────────────────────────
            if (isGrabbed && (colorCorrect != _wasColorCorrect || shapeCorrect != _wasShapeCorrect))
            {
                _wasColorCorrect = colorCorrect;
                _wasShapeCorrect = shapeCorrect;
                _timer = 0f;
                if (armGuide != null) armGuide.Hide();
            }

            // ── Timer ─────────────────────────────────────────────────────
            _timer += Time.deltaTime;

            if (_timer >= stateMachine.threshold)
            {
                _timer = 0f;

                if (!isGrabbed)
                {
                    if (armGuide != null && _currentTargetBlock != null)
                        armGuide.Show(_currentTargetBlock.transform.position, false);
                }
                else if (!isOnSlot)
                {
                    if (_currentTargetBlock != null)
                    {
                        if (!colorCorrect)
                        {
                            Transform guideTarget = GetColorGuideTarget();
                            if (armGuide != null && guideTarget != null)
                                armGuide.Show(guideTarget.position, true);
                        }
                        else if (!shapeCorrect)
                        {
                            Transform guideTarget = GetShapeGuideTarget();
                            if (armGuide != null && guideTarget != null)
                                armGuide.Show(guideTarget.position, true);
                        }
                        else
                        {
                            if (armGuide != null)
                                armGuide.Show(gameManager.slot.position, false);
                        }
                    }
                }
            }
        }
    }

    // ── Arm Guide Helpers ─────────────────────────────────────────────────
    Transform GetColorGuideTarget()
    {
        switch (gameManager.levelColors[gameManager.currentLevel].ToLower())
        {
            case "red":   return redDisk;
            case "blue":  return blueDisk;
            case "green": return greenDisk;
        }
        return null;
    }

    Transform GetShapeGuideTarget()
    {
        switch (gameManager.levelMesh[gameManager.currentLevel].ToLower())
        {
            case "sphere":   return sphereShape;
            case "cylinder": return cylinderShape;
        }
        return null;
    }

    // ── Condition A Visual Feedback ───────────────────────────────────────
    IEnumerator PulseLoop()
    {
        while (_pulsing)
        {
            if (colorslot == null) yield break;
            colorslot.SetActive(false);
            yield return new WaitForSeconds(0.2f);
            colorslot.SetActive(true);
            yield return new WaitForSeconds(0.2f);
        }
    }

    IEnumerator JumpShakeLoop()
    {
        if (_currentTargetBlock == null) yield break;

        float jumpHeight = 0.05f;
        float jumpSpeed  = 2f;

        _currentTargetBlock.isBeingControlled = true;

        Rigidbody rb = _currentTargetBlock.GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.useGravity      = false;
            rb.linearVelocity  = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
        }

        while (_jumping)
        {
            bool isGrabbed = _currentTargetBlock._grabbable != null &&
                            _currentTargetBlock._grabbable.GrabPoints != null &&
                            _currentTargetBlock._grabbable.GrabPoints.Count > 0;

            if (isGrabbed)
            {
                _currentTargetBlock.isBeingControlled = false;
                if (rb != null) rb.useGravity = true;
                yield break;
            }

            if (rb != null)
            {
                rb.linearVelocity  = Vector3.zero;
                rb.angularVelocity = Vector3.zero;
            }

            float t       = Time.time * jumpSpeed;
            float yOffset = Mathf.Abs(Mathf.Sin(t)) * jumpHeight;
            _currentTargetBlock.transform.position = _currentTargetBlock.assignedSlot.position + Vector3.up * yOffset;

            yield return null;
        }

        if (rb != null) rb.useGravity = true;
        _currentTargetBlock.isBeingControlled = false;
        _currentTargetBlock.transform.position = _currentTargetBlock.assignedSlot.position;
    }
}