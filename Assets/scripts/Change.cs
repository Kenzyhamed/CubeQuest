using UnityEngine;

public class ShapeEffector : MonoBehaviour
{
    public enum EffectType { Red, Blue, Green, TurnSphere, TurnCylinder }

    [SerializeField] private EffectType effectType; // set per-disk in the Inspector
    public GameManager gameManager;
    public SoundStateMachine stateMachine;
    private string Condition => gameManager.currentcondition;

    public Mesh sphereMesh;
    public Mesh cylinderMesh;

    void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Cube")) return;
        if (!IsTargetMatch()) return; // non-matching disks do nothing at all

        var visual = other.transform.Find("Visual");
        if (visual == null) return;

        var rend = visual.GetComponent<Renderer>();
        var meshFilter = visual.GetComponent<MeshFilter>();

        switch (effectType)
        {
            case EffectType.Red:
                rend.material.color = Color.red;
                if (Condition == "A") stateMachine?.OnCorrectColorHit();
                break;

            case EffectType.Blue:
                rend.material.color = Color.blue;
                if (Condition == "A") stateMachine?.OnCorrectColorHit();
                break;

            case EffectType.Green:
                rend.material.color = Color.green;
                if (Condition == "A") stateMachine?.OnCorrectColorHit();
                break;

            case EffectType.TurnSphere:
                meshFilter.mesh = sphereMesh;
                visual.localScale = new Vector3(1f, 1.3f, 1f);
                if (Condition == "A") stateMachine?.OnCorrectShapeHit();
                break;

            case EffectType.TurnCylinder:
                meshFilter.mesh = cylinderMesh;
                visual.localScale = new Vector3(1f, 1f, 1f);
                if (Condition == "A") stateMachine?.OnCorrectShapeHit();
                break;
        }
    }

    private bool IsTargetMatch()
    {
        return effectType switch
        {
            EffectType.Red => gameManager.IsCorrectColor("red"),
            EffectType.Blue => gameManager.IsCorrectColor("blue"),
            EffectType.Green => gameManager.IsCorrectColor("green"),
            EffectType.TurnSphere => gameManager.IsCorrectShape("sphere"),
            EffectType.TurnCylinder => gameManager.IsCorrectShape("cylinder"),
            _ => false
        };
    }
}