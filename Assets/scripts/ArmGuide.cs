using UnityEngine;

public class ArmGuideCubeToItsSlot : MonoBehaviour
{
    [Header("What moves")]
    public Transform armRoot;
    public Transform fingerTip;
    public Animator handAnimator;
    public Transform slot;

    bool _showing = false;
    Vector3 _targetPosition;

    void Start() => SetVisibility(false);

    public void Show(Vector3 targetPosition, bool isAtColorOrShape, bool isB1, bool isB2)
    {

        _targetPosition = targetPosition;

        if (!_showing)
        {
            Vector3 desired = new Vector3(_targetPosition.x, _targetPosition.y - 0.1f, _targetPosition.z - 0.15f);
            transform.position = desired;
        }

        if (isB1)
        {
            if (handAnimator != null)
                handAnimator.SetBool("isPointing", true);
        }

        if (isB2)
        {
            bool useRelease = Vector3.Distance(_targetPosition, slot.position) < 0.05f;

            if (handAnimator != null)
            {
                handAnimator.SetBool("isGrab",     !useRelease && !isAtColorOrShape);
                handAnimator.SetBool("isRelease",   useRelease && !isAtColorOrShape);
                handAnimator.SetBool("isKeepHold", !useRelease && isAtColorOrShape);
                handAnimator.SetBool("isPointing", false);
            }
        }

        _showing = true;
        SetVisibility(true);
    }

    public void Hide()
    {
        _showing = false;
        if (handAnimator != null)
        {
            handAnimator.SetBool("isPointing", false);
            handAnimator.SetBool("isGrab",     false);
            handAnimator.SetBool("isRelease",  false);
            handAnimator.SetBool("isKeepHold", false);
        }
        SetVisibility(false);
    }

    void SetVisibility(bool visible)
    {
        foreach (Renderer r in GetComponentsInChildren<Renderer>())
            r.enabled = visible;
    }
}