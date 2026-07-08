using UnityEngine;
using Oculus.Interaction;

public class SnapToPoint : MonoBehaviour
{
    public Transform[] snapPoints;
    public float snapDistance = 0.5f;

    [Header("Return to slot settings")]
    public Transform assignedSlot;

    [Header("References")]
    public GameManager gameManager;
    public SoundStateMachine stateMachine;
    private string condition;

    public Grabbable _grabbable;
    public bool isBeingControlled = false;

    bool _snappedToSlot = false;
    bool _checkPending = false;
    bool _wasGrabbed = false;
    Quaternion _originalRotation;

    void Start()
    {
        _grabbable = GetComponentInParent<Grabbable>()
                  ?? GetComponentInChildren<Grabbable>()
                  ?? GetComponent<Grabbable>();

        _originalRotation = transform.rotation;
    }

    void Update()
    {
        condition=gameManager.currentcondition;
        if (isBeingControlled) return;

        bool isGrabbed = _grabbable != null &&
                        _grabbable.GrabPoints != null &&
                        _grabbable.GrabPoints.Count > 0;

        if (isGrabbed && !_wasGrabbed)
            _wasGrabbed = true;

        if (!isGrabbed) _wasGrabbed = false;

        if (isGrabbed)
        {
            _snappedToSlot = false;
            _checkPending = false;
            CancelInvoke(nameof(RunCheck));
            return;
        }

        bool isNearSnap = false;
        if (snapPoints != null)
        {
            foreach (Transform point in snapPoints)
            {
                if (point == null) continue;
                if (Vector3.Distance(transform.position, point.position) <= snapDistance)
                {
                    isNearSnap = true;
                    break;
                }
            }
        }

        if (!isNearSnap && assignedSlot != null)
        {
            transform.position = assignedSlot.position;
            transform.rotation = _originalRotation;
            Rigidbody rb = GetComponent<Rigidbody>();
            if (rb != null)
            {
                rb.linearVelocity = Vector3.zero;
                rb.angularVelocity = Vector3.zero;
            }
            return;
        }

        if (snapPoints == null || snapPoints.Length == 0) return;

        Transform closestSnapPoint = null;
        float closestDistance = Mathf.Infinity;

        foreach (Transform point in snapPoints)
        {
            if (point == null) continue;
            float distance = Vector3.Distance(transform.position, point.position);
            if (distance < closestDistance)
            {
                closestDistance = distance;
                closestSnapPoint = point;
            }
        }

        if (closestSnapPoint != null && closestDistance < snapDistance)
        {
            transform.position = closestSnapPoint.position;
            transform.rotation = _originalRotation;

            if (!_snappedToSlot && !_checkPending && gameManager != null &&
                closestSnapPoint == gameManager.slot)
            {
                _snappedToSlot = true;
                _checkPending = true;
                Invoke(nameof(RunCheck), 1f);
            }
        }
    }

    void RunCheck()
    {
        _checkPending = false;
        LetterBox lb = GetComponentInChildren<LetterBox>();
        if (lb == null) return;

        bool correct = gameManager.IsCorrectLetter(lb.letter, lb.meshName, lb.colorName, out bool cMatch, out bool mMatch, out bool lMatch);

        if (correct)
        {
            TrialManager.Instance?.EndTrial(success: true);

            //if (condition == "A")
            //{
            if (stateMachine != null) stateMachine.OnCorrectPlaced();
            //}
            SendHome();
            gameManager.OnCorrect();
            _snappedToSlot = false;
        }
        else
        {
            if (condition == "A" && stateMachine != null)
            {
                if (!lMatch)
                    stateMachine.OnWrongPlaced();
                else if (!cMatch)
                    stateMachine.OnWrongColorHit();
                else if (!mMatch)
                    stateMachine.OnWrongShapeHit();
            }
            SendHome();
            _snappedToSlot = false;
        }
    }

    public void SendHome()
    {
        _snappedToSlot = false;
        _checkPending = false;
        CancelInvoke(nameof(RunCheck));
        if (assignedSlot != null)
        {
            transform.position = assignedSlot.position;
            transform.rotation = _originalRotation;
        }
    }
}