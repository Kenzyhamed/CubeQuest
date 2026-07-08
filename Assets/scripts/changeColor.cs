using UnityEngine;

public class BlockColorChanger : MonoBehaviour
{
    [SerializeField] private Transform visual; // drag the Visual child in Inspector
    public GameManager gameManager;
    public SoundStateMachine stateMachine;
    private string Condition => gameManager.currentcondition;
    private Renderer rend;
    private MeshFilter meshFilter;

    public Mesh sphereMesh;
    public Mesh cylinderMesh;

    void Start()
    {
        rend = visual.GetComponent<Renderer>();
        meshFilter = visual.GetComponent<MeshFilter>();
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("RedCylinder"))
        {
            rend.material.color = Color.red;
            if (Condition == "A")
            {
                if (gameManager.IsCorrectColor("red")) stateMachine?.OnCorrectColorHit();
                else stateMachine?.OnWrongColorHit();
            }

        }
        else if (other.CompareTag("BlueCylinder"))
        {
            rend.material.color = Color.blue;
            if (Condition == "A")
            {
                if (gameManager.IsCorrectColor("blue")) stateMachine?.OnCorrectColorHit();
                else stateMachine?.OnWrongColorHit();
            }
        }
        else if (other.CompareTag("GreenCylinder"))
        {
            rend.material.color = Color.green;
            if (Condition == "A")
            {
                if (gameManager.IsCorrectColor("green")) stateMachine?.OnCorrectColorHit();
                else stateMachine?.OnWrongColorHit();
            }
        }
        else if (other.CompareTag("TurnSphere"))
        {
            meshFilter.mesh = sphereMesh;
            if (Condition == "A")
            {
                visual.localScale = new Vector3(1f, 1.3f, 1f);
                if (gameManager.IsCorrectShape("sphere")) stateMachine?.OnCorrectShapeHit();
                else stateMachine?.OnWrongShapeHit();
            }
        }
        else if (other.CompareTag("TurnCylinder"))
        {
            meshFilter.mesh = cylinderMesh;
            if (Condition == "A")
            {
                visual.localScale = new Vector3(1f, 1f, 1f);
                if (gameManager.IsCorrectShape("cylinder")) stateMachine?.OnCorrectShapeHit();
                else stateMachine?.OnWrongShapeHit();
            }
        }
    }
}