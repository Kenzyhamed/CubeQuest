using UnityEngine;

public class ArmGuideCubeToItsSlot : MonoBehaviour
{
    [Header("What moves")]
    public Transform armRoot;
    public Transform fingerTip;
    public Animator handAnimator;

    bool _showing = false;
    string _condition = "A";
    Vector3 _targetPosition;

    void Start() => gameObject.SetActive(false);

    public void SetCondition(string condition)
    {
        _condition = condition;
    }

    public void Show(Vector3 targetPosition)
    {
        if (_condition == "A") return;

        _targetPosition = targetPosition;

        if (!_showing)
        {
            Vector3 desired = new Vector3(_targetPosition.x, _targetPosition.y - 0.15f, _targetPosition.z - 0.2f);
            Vector3 delta = desired - fingerTip.position;
            armRoot.position = armRoot.position + delta;
        }

        gameObject.SetActive(true);
        if (handAnimator != null)
            handAnimator.SetBool("isPointing", true);
        _showing = true;
    }
    public void Hide()
    {
        _showing = false;
        if (handAnimator != null)
            handAnimator.SetBool("isPointing", false);
        gameObject.SetActive(false);
    }
}