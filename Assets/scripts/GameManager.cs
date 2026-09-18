using UnityEngine;
using TMPro;
using System.Collections;
using Unity.Netcode;

public class GameManager : NetworkBehaviour
{
    public TrialManager trialmanager;
    public Transform slot;
    public TextMeshProUGUI targetLetterText;
    public TextMeshProUGUI resultText;
    public GameObject targetShape;

    public Mesh cylinderMesh;
    public Mesh sphereMesh;
    public Mesh cubeMesh;
    public Mesh noMesh;
    public string currentcondition;
    public string[] levelLetters;
    public string[] levelColors;
    public string[] levelMesh;

    public NetworkVariable<int> currentLevel = new NetworkVariable<int>(
        0,
        NetworkVariableReadPermission.Everyone,
        NetworkVariableWritePermission.Server
    );

    [Header("Conditions (set by AdminDashboardController)")]
    public NetworkVariable<bool> isA = new NetworkVariable<bool>(
        false, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);
    public NetworkVariable<bool> isB1 = new NetworkVariable<bool>(
        false, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);
    public NetworkVariable<bool> isB2 = new NetworkVariable<bool>(
        false, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);

    public override void OnNetworkSpawn()
    {
        currentLevel.OnValueChanged += HandleLevelChanged;
        isA.OnValueChanged += (_, _) => RefreshCondition();
        isB1.OnValueChanged += (_, _) => RefreshCondition();
        isB2.OnValueChanged += (_, _) => RefreshCondition();
    }

    public override void OnNetworkDespawn()
    {
        currentLevel.OnValueChanged -= HandleLevelChanged;
    }

    public void SetTargets(string[] levelLettersCon, string[] levelColorsCon, string[] levelMeshCon)
    {

        levelLetters = levelLettersCon;
        levelColors = levelColorsCon;
        levelMesh = levelMeshCon;
   
    }

    /// <summary>Called by AdminDashboardController when a level trigger is poked.</summary>
    public void GoToLevel(int levelIndex)
    {
        if (!IsServer) return; // dashboard only ever runs on the host/server

        if (levelLetters == null || levelIndex < 0 || levelIndex >= levelLetters.Length)
        {
            Debug.LogWarning($"GameManager: level index {levelIndex} is out of range.");
            return;
        }

        currentLevel.Value = levelIndex;
    }

    void RefreshCondition()
    {
        var active = new System.Collections.Generic.List<string>();
        if (isA.Value) active.Add("A");
        if (isB1.Value) active.Add("B1");
        if (isB2.Value) active.Add("B2");
        currentcondition = string.Join("+", active);
    }

    // Fires on EVERY machine (host and client) once the new level value has synced.
    void HandleLevelChanged(int oldLevel, int newLevel)
    {
        StartCoroutine(LoadLevelRoutine(newLevel));
    }

    IEnumerator LoadLevelRoutine(int levelIndex)
    {
        if (levelLetters == null || levelIndex >= levelLetters.Length)
            yield break;

        yield return new WaitForSeconds(2f);

        if (trialmanager != null && trialmanager.stateMachine != null)
        {
            trialmanager.stateMachine.targetLetter = levelLetters[levelIndex];
            trialmanager.stateMachine.PlayIntroOnly(levelIndex);
        }

        // Runs locally on both machines — TrialManager itself stays a plain
        // MonoBehaviour and doesn't care whether this machine is host or client.
        trialmanager.StartTrial(levelLetters[levelIndex], levelIndex);

        if (targetLetterText != null) targetLetterText.text = levelLetters[levelIndex];

        if (levelMesh != null && levelIndex < levelMesh.Length)
            UpdateMesh(levelMesh[levelIndex]);

        if (levelColors != null && levelIndex < levelColors.Length)
            UpdateColor(levelColors[levelIndex]);

        Debug.Log($"Loaded level {levelIndex + 1}, letter: {levelLetters[levelIndex]}, mesh: {levelMesh?[levelIndex]}, color: {levelColors?[levelIndex]}");
    }

    void UpdateMesh(string meshName)
    {
        MeshFilter meshFilter = targetShape.GetComponent<MeshFilter>();
        if (meshFilter == null) return;

        switch (meshName.ToLower())
        {
            case "sphere":
                meshFilter.mesh = sphereMesh;
                targetShape.transform.localScale = new Vector3(0.3f, 0.3f, 0.3f);
                break;
            case "cube":
                meshFilter.mesh = cubeMesh;
                targetShape.transform.localScale = new Vector3(0.25f, 0.21f, 0.2f);
                break;
            case "cylinder":
                meshFilter.mesh = cylinderMesh;
                targetShape.transform.localScale = new Vector3(0.35f, 0.25f, 0.25f);
                break;
            case "none":
                meshFilter.mesh = noMesh;
                break;
            default:
                Debug.LogWarning($"Unknown mesh '{meshName}', no change applied.");
                break;
        }
    }

    void UpdateColor(string colorName)
    {
        if (colorName.ToLower() == "same") return;

        MeshRenderer meshRenderer = targetShape.GetComponent<MeshRenderer>();
        if (meshRenderer == null) return;

        switch (colorName.ToLower())
        {
            case "red":
                meshRenderer.material.color = Color.red;
                break;
            case "green":
                meshRenderer.material.color = Color.green;
                break;
            case "blue":
                meshRenderer.material.color = Color.blue;
                break;
            case "none":
                meshRenderer.material.color = Color.clear;
                break;
            default:
                Debug.LogWarning($"Unknown color '{colorName}', no change applied.");
                break;
        }
    }

    public bool IsCorrectLetter(string letter, string meshName, string colorName, out bool colorMatch, out bool meshMatch, out bool letterMatch)
    {
        letterMatch = letter == targetLetterText.text.Trim();
        meshMatch = levelMesh[currentLevel.Value].ToLower() == "none" || meshName.Contains(levelMesh[currentLevel.Value].ToLower());
        colorMatch = levelColors[currentLevel.Value].ToLower() == "none" || colorName == levelColors[currentLevel.Value].ToLower();

        return letterMatch && colorMatch && meshMatch;
    }

    public bool IsCorrectColor(string colorName)
    {
        return levelColors[currentLevel.Value].ToLower() == "none" || colorName == levelColors[currentLevel.Value].ToLower();
    }

    public bool IsCorrectShape(string meshName)
    {
        return levelMesh[currentLevel.Value].ToLower() == "none" || meshName.Contains(levelMesh[currentLevel.Value].ToLower());
    }

    // ── Called from TrialManager/SnapToPoint when a block is placed correctly. ──
    // Works no matter which machine (host or client) is the one actually playing,
    // since either could be holding the cube thanks to Meta's networked grabbable
    // ownership transfer.
    public void ReportCorrect()
    {
        if (IsServer)
        {
            OnCorrect(); // this machine already IS the server — no RPC needed
        }
        else
        {
            ReportCorrectRpc(); // client asks the server to advance the level
        }
    }

    [Rpc(SendTo.Server)]
    private void ReportCorrectRpc()
    {
        OnCorrect();
    }

    private void OnCorrect()
    {
        // Runs only on the server, either directly (host played) or via RPC (client played).
        StartCoroutine(NextLevelAfterDelay());
    }

    IEnumerator NextLevelAfterDelay()
    {
        yield return new WaitForSeconds(4f);
        currentLevel.Value = currentLevel.Value + 1; // triggers HandleLevelChanged on both machines
    }
}