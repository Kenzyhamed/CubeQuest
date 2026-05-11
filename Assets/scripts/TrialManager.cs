using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class TrialManager : MonoBehaviour
{
    public static TrialManager Instance;

    [Header("References")]
    public GameManager gameManager;
    public GameObject colorslot;
    public ArmGuideCubeToItsSlot armGuide;

    [Header("Letter Blocks")]
    public SnapToPoint[] allBlocks;

    [Header("Breakdown Threshold")]
    public float hesitationThreshold = 3f;

    private Dictionary<string, AudioClip> _findSounds = new Dictionary<string, AudioClip>();
    private AudioClip _placeInSlotSound;
    private string _condition = "A";

    private float _timer = 0f;

    private bool _trialActive = false;
    private bool _wasGrabbed = false;
    private bool _pulsing = false;
    private bool _jumping = false;
    private string _targetLetter = "";
    private SnapToPoint _currentTargetBlock;

    void Awake() => Instance = this;

    public void SetSounds(string condition, AudioClip placeSlot, AudioClip E, AudioClip F, AudioClip L, AudioClip K, AudioClip M, AudioClip N, AudioClip R, AudioClip P)
    {
        _placeInSlotSound = placeSlot;
        _condition = condition;
        hesitationThreshold = condition == "B2" ? 5f : 3f;

        if (armGuide != null) armGuide.SetCondition(condition);

        _findSounds["E"] = E;
        _findSounds["F"] = F;
        _findSounds["L"] = L;
        _findSounds["K"] = K;
        _findSounds["M"] = M;
        _findSounds["N"] = N;
        _findSounds["R"] = R;
        _findSounds["P"] = P;
    }

    AudioClip GetFindSoundForLetter(string letter)
    {
        if (_findSounds.ContainsKey(letter))
            return _findSounds[letter];
        Debug.LogWarning($"No find sound for letter: {letter}");
        return null;
    }

    public void StartTrial(string targetLetter)
    {
        _trialActive = true;
        _targetLetter = targetLetter;
        _pulsing = false;
        _jumping = false;
        _timer = 0f;
        _wasGrabbed = false;
  
        StopAllCoroutines();
        if (colorslot != null) colorslot.SetActive(true);
        if (armGuide != null) armGuide.Hide();

        StartCoroutine(FindTargetBlock());
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
                Debug.Log($"Found target block: {block.gameObject.name}");
                break;
            }
        }

        if (_currentTargetBlock == null)
            Debug.LogWarning($"Could not find block for letter: {_targetLetter}");
    }

    public void EndTrial(bool success)
    {
        Debug.Log($"EndTrial called, success: {success}");

        _trialActive = false;
        _pulsing = false;
        _jumping = false;
        _timer = 0f;
        _wasGrabbed = false;
        StopAllCoroutines();
        if (colorslot != null) colorslot.SetActive(true);
        if (armGuide != null) armGuide.Hide();

        if (_currentTargetBlock != null)
        {
            _currentTargetBlock.isBeingControlled = false;
            _currentTargetBlock.transform.position = _currentTargetBlock.assignedSlot.position;
        }
    }
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

        if (isGrabbed != _wasGrabbed)
        {
            _timer = 0f;
            _wasGrabbed = isGrabbed;
            if (armGuide != null) armGuide.Hide();
            if (isGrabbed)
            {
                _pulsing = false;
                _jumping = false;
                StopCoroutine("PulseLoop");
                StopCoroutine("JumpShakeLoop");
            }
        }

        _timer += Time.deltaTime;

        if (_timer >= hesitationThreshold)
        {
            _timer = 0f;
            if (!isGrabbed)
            {
                if (_condition == "A")
                {
                    // Condition A — find sound + jump + pulse
                    AudioClip findClip = GetFindSoundForLetter(_targetLetter);
                    if (gameManager.audioSource && findClip != null)
                        gameManager.audioSource.PlayOneShot(findClip);

                    if (!_jumping && !_pulsing)
                    {
                        _pulsing = true;
                        _jumping = true;
                        StartCoroutine(PulseLoop());
                        StartCoroutine(JumpShakeLoop());
                    }
                }
                else
                {
                    // B1/B2 — point at cube
                    if (armGuide != null && _currentTargetBlock != null)
                        armGuide.Show(_currentTargetBlock.transform.position);
                }
            }
            else if (!isOnSlot)
            {
                if (_condition == "A")
                {
                    // Condition A — play place in slot sound
                    if (gameManager.audioSource && _placeInSlotSound != null)
                        gameManager.audioSource.PlayOneShot(_placeInSlotSound);
                }
                else
                {
                    // B1/B2 — point at slot
                    if (armGuide != null)
                        armGuide.Show(gameManager.slot.position);
                }
            }
        }
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

public enum BreakdownType { Hesitation, WrongPickup, MissSlot, Drop }