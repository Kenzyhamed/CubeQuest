using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;

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

    // ── Condition flags (read from GameManager, kept in sync every frame) ──
    private bool _isA;
    private bool _isB1;
    private bool _isB2;

    // ── State Tracking (Condition A) ──────────────────────────────────────
    private float _timerA = 0f;
    private bool _wasGrabbedA = false;
    private bool _pulsing = false;
    private bool _jumping = false;

    // ── State Tracking (Condition B1 / B2 - arm guide) ────────────────────
    bool _wasColorCorrect = false;
    bool _wasShapeCorrect = false;
    private float _timerGuide = 0f;
    private bool _wasGrabbedGuide = false;
    private enum GuideStage { None, Cube, Color, Shape, Slot }
    private GuideStage _currentStage = GuideStage.None;
    private bool _isShowing = false;

    // ── Shared trial state ────────────────────────────────────────────────
    private bool _trialActive = false;
    private string _targetLetter = "";
    private SnapToPoint _currentTargetBlock;
    private int _currentLevelIndex = 0;

    void Awake()
    {
        Instance = this;
    }

    // ── Trial Lifecycle ───────────────────────────────────────────────────
    public void StartTrial(string targetLetter, int level)
    {
        ResetAllBlocks();
        _trialActive = true;
        _targetLetter = targetLetter;
        _currentLevelIndex = level;

        _isA = gameManager.isA.Value;
        _isB1 = gameManager.isB1.Value;
        _isB2 = gameManager.isB2.Value;

        ResetConditionAState();
        ResetGuideConditionState();

        StopAllCoroutines();
        if (armGuide != null) armGuide.Hide();

        if (stateMachine != null)
        {
            stateMachine.targetLetter = targetLetter;
        }

        StartConditionAAudioIfActive();

        if (level < 6)
        {
            SetDisksVisible(false);
            SetShapesVisible(false);
        }
        else if (level < 12)
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
        ResetConditionAState();
        ResetGuideConditionState();
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

        SyncConditionFlags();

        bool isGrabbed = IsBlockGrabbed(_currentTargetBlock);

        bool isOnSlot = _currentTargetBlock != null &&
                        Vector3.Distance(_currentTargetBlock.transform.position,
                        gameManager.slot.position) < 0.05f;

        LetterBox lb = _currentTargetBlock != null ? _currentTargetBlock.GetComponentInChildren<LetterBox>() : null;
        bool colorCorrect = lb != null && (gameManager.levelColors[gameManager.currentLevel.Value].ToLower() == "none" || lb.colorName.ToLower() == gameManager.levelColors[gameManager.currentLevel.Value].ToLower());
        string targetMesh = gameManager.levelMesh[gameManager.currentLevel.Value].ToLower();
        if (targetMesh == "cube") targetMesh = "none";
        bool shapeCorrect = lb != null && (targetMesh == "none" || lb.meshName.ToLower().Contains(targetMesh));

        // These two blocks are independent (not if/else), so if isA and
        // isB1/isB2 are both true they run together in the same frame.
        if (_isA)
        {
            RunConditionA(isGrabbed, lb);
        }

        if (_isB1 || _isB2)
        {
            RunGuideConditions(isGrabbed, isOnSlot, colorCorrect, shapeCorrect);
        }

        // Note: success/level-advance is triggered from SnapToPoint.RunCheck()
        // via gameManager.ReportCorrect(), not from here.
    }

    // ── Network-aware grabbed check ─────────────────────────────────────
    // Meta's Grabbable.GrabPoints only reflects LOCAL grab state (the machine
    // physically holding the controller). To know grab state correctly on
    // BOTH machines, fall back to checking NetworkObject ownership when this
    // machine isn't the one holding it.
    bool IsBlockGrabbed(SnapToPoint block)
    {
        if (block == null) return false;

        bool localGrab = block._grabbable != null &&
                        block._grabbable.GrabPoints != null &&
                        block._grabbable.GrabPoints.Count > 0;

        NetworkObject netObj = block.GetComponent<NetworkObject>();
        if (netObj != null && netObj.IsOwner)
        {
            return localGrab;
        }

        if (netObj != null)
        {
            return netObj.OwnerClientId != NetworkManager.ServerClientId;
        }

        return localGrab;
    }

    void SyncConditionFlags()
    {
        bool newIsA = gameManager.isA.Value;
        bool newIsB1 = gameManager.isB1.Value;
        bool newIsB2 = gameManager.isB2.Value;

        bool aChanged = newIsA != _isA;
        bool bChanged = (newIsB1 != _isB1) || (newIsB2 != _isB2);

        if (!aChanged && !bChanged) return;

        _isA = newIsA;
        _isB1 = newIsB1;
        _isB2 = newIsB2;

        if (aChanged)
        {
            ResetConditionAState();
            StartConditionAAudioIfActive();
        }
        if (bChanged) ResetGuideConditionState();
    }

    void StartConditionAAudioIfActive()
    {
        if (_isA && stateMachine != null)
            stateMachine.StartLevel(_currentLevelIndex);
    }

    void ResetConditionAState()
    {
        _pulsing = false;
        _jumping = false;
        _timerA = 0f;
        _wasGrabbedA = false;
        StopCoroutine("PulseLoop");
        StopCoroutine("JumpShakeLoop");

        if (colorslot != null) colorslot.SetActive(true);

        if (_currentTargetBlock != null)
        {
            _currentTargetBlock.isBeingControlled = false;
            Rigidbody rb = _currentTargetBlock.GetComponent<Rigidbody>();
            if (rb != null) rb.useGravity = true;
        }

        if (stateMachine != null) stateMachine.StopAudio();
    }

    void ResetGuideConditionState()
    {
        _timerGuide = 0f;
        _wasGrabbedGuide = false;
        _wasColorCorrect = false;
        _wasShapeCorrect = false;
        _isShowing = false;
        _currentStage = GuideStage.None;
        if (armGuide != null) armGuide.Hide();
    }

    void RunConditionA(bool isGrabbed, LetterBox lb)
    {
        if (isGrabbed != _wasGrabbedA)
        {
            _timerA = 0f;
            _wasGrabbedA = isGrabbed;

            if (isGrabbed)
            {
                _pulsing = false;
                _jumping = false;
                StopCoroutine("JumpShakeLoop");

                if (stateMachine != null)
                {
                    bool isCorrectCube = lb != null && lb.letter == _targetLetter;
                    if (isCorrectCube)
                    {
                        stateMachine.OnCorrectCubeGrabbed();
                        bool colorCorrect = lb != null && (gameManager.levelColors[gameManager.currentLevel.Value].ToLower() == "none" || lb.colorName.ToLower() == gameManager.levelColors[gameManager.currentLevel.Value].ToLower());
                        string targetMesh = gameManager.levelMesh[gameManager.currentLevel.Value].ToLower();
                        if (targetMesh == "cube") targetMesh = "none";
                        bool shapeCorrect = lb != null && (targetMesh == "none" || lb.meshName.ToLower().Contains(targetMesh));

                        if (colorCorrect && gameManager.currentLevel.Value >= 3) stateMachine.OnCorrectColorHit();
                        if (shapeCorrect && gameManager.currentLevel.Value >= 6) stateMachine.OnCorrectShapeHit();
                    }
                }
            }
            else
            {
                if (stateMachine != null)
                    stateMachine.OnCubeReleased();
            }
        }

        _timerA += Time.deltaTime;

        if (_timerA >= stateMachine.threshold)
        {
            _timerA = 0f;

            if (!isGrabbed && !_jumping && !_pulsing)
            {
                _pulsing = true;
                _jumping = true;
                StartCoroutine(PulseLoop());
                StartCoroutine(JumpShakeLoop());
            }
        }
    }

    void RunGuideConditions(bool isGrabbed, bool isOnSlot, bool colorCorrect, bool shapeCorrect)
    {
        if (isGrabbed != _wasGrabbedGuide)
        {
            _timerGuide = 0f;
            _wasGrabbedGuide = isGrabbed;
            _isShowing = false;
            _currentStage = GuideStage.None;
            if (armGuide != null) armGuide.Hide();
        }

        _timerGuide += Time.deltaTime;

        GuideStage desiredStage;

        if (!isGrabbed)
        {
            desiredStage = GuideStage.Cube;
        }
        else if (_currentTargetBlock == null)
        {
            desiredStage = GuideStage.None;
        }
        else if (!colorCorrect)
        {
            desiredStage = GuideStage.Color;
        }
        else if (!shapeCorrect)
        {
            desiredStage = GuideStage.Shape;
        }
        else if (!isOnSlot)
        {
            desiredStage = GuideStage.Slot;
        }
        else
        {
            desiredStage = GuideStage.None;
        }

        if (desiredStage != _currentStage)
        {
            _currentStage = desiredStage;
            _timerGuide = 0f;
            _isShowing = false;
            if (armGuide != null) armGuide.Hide();
        }

        if (!_isShowing && _timerGuide >= stateMachine.threshold && armGuide != null)
        {
            switch (_currentStage)
            {
                case GuideStage.Cube:
                    if (_currentTargetBlock != null)
                    {
                        armGuide.Show(_currentTargetBlock.transform.position, false, _isB1, _isB2);
                        _isShowing = true;
                    }
                    break;

                case GuideStage.Color:
                    {
                        Transform colorT = GetColorGuideTarget();
                        if (colorT != null)
                        {
                            armGuide.Show(colorT.position, true, _isB1, _isB2);
                            _isShowing = true;
                        }
                    }
                    break;

                case GuideStage.Shape:
                    {
                        Transform shapeT = GetShapeGuideTarget();
                        if (shapeT != null)
                        {
                            armGuide.Show(shapeT.position, true, _isB1, _isB2);
                            _isShowing = true;
                        }
                    }
                    break;

                case GuideStage.Slot:
                    armGuide.Show(gameManager.slot.position, false, _isB1, _isB2);
                    _isShowing = true;
                    break;

                case GuideStage.None:
                default:
                    break;
            }
        }

        _wasColorCorrect = colorCorrect;
        _wasShapeCorrect = shapeCorrect;
    }

    Transform GetColorGuideTarget()
    {
        switch (gameManager.levelColors[gameManager.currentLevel.Value].ToLower())
        {
            case "red": return redDisk;
            case "blue": return blueDisk;
            case "green": return greenDisk;
        }
        return null;
    }

    Transform GetShapeGuideTarget()
    {
        switch (gameManager.levelMesh[gameManager.currentLevel.Value].ToLower())
        {
            case "sphere": return sphereShape;
            case "cylinder": return cylinderShape;
        }
        return null;
    }

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

        // Only the machine that currently owns this block should drive its
        // position — otherwise NetworkTransform overrides this on the
        // non-owning machine.
        NetworkObject netObj = _currentTargetBlock.GetComponent<NetworkObject>();
        if (netObj != null && !netObj.IsOwner)
        {
            yield break;
        }

        float jumpHeight = 0.05f;
        float jumpSpeed = 2f;

        _currentTargetBlock.isBeingControlled = true;

        Rigidbody rb = _currentTargetBlock.GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.useGravity = false;
            rb.linearVelocity = Vector3.zero;
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
                rb.linearVelocity = Vector3.zero;
                rb.angularVelocity = Vector3.zero;
            }

            float t = Time.time * jumpSpeed;
            float yOffset = Mathf.Abs(Mathf.Sin(t)) * jumpHeight;
            _currentTargetBlock.transform.position = _currentTargetBlock.assignedSlot.position + Vector3.up * yOffset;

            yield return null;
        }

        if (rb != null) rb.useGravity = true;
        _currentTargetBlock.isBeingControlled = false;
        _currentTargetBlock.transform.position = _currentTargetBlock.assignedSlot.position;
    }
}