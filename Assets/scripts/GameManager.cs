using UnityEngine;
using TMPro;
using System.Collections;

public class GameManager : MonoBehaviour
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
    public int currentLevel = 0;

    public void SetCondition(string condition, string[] levelLettersCon, string[] levelColorsCon, string[] levelMeshCon)
    {
        currentLevel = 0;
        levelLetters = levelLettersCon;
        levelColors = levelColorsCon;
        levelMesh = levelMeshCon;
        currentcondition=condition;
        LoadLevel(currentLevel);
    }

    void LoadLevel(int levelIndex)
    {
        StartCoroutine(LoadLevelRoutine(levelIndex));
    }

    IEnumerator LoadLevelRoutine(int levelIndex)
    {
        if (levelLetters == null || levelIndex >= levelLetters.Length)
            yield break;

        yield return new WaitForSeconds(2f);

        trialmanager.StartTrial(levelLetters[levelIndex], currentcondition, levelIndex);

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
                targetShape.transform.localScale = new Vector3(0.2f, 0.2f, 0.2f);
                break;
            case "cube":
                meshFilter.mesh = cubeMesh;
                targetShape.transform.localScale = new Vector3(0.15f, 0.11f, 0.1f);
                break;
            case "cylinder":
                meshFilter.mesh = cylinderMesh;
                targetShape.transform.localScale = new Vector3(0.25f, 0.15f, 0.15f);
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
        meshMatch  = levelMesh[currentLevel].ToLower() == "none" || meshName.Contains(levelMesh[currentLevel].ToLower());
        colorMatch = levelColors[currentLevel].ToLower() == "none" || colorName == levelColors[currentLevel].ToLower();

        Debug.Log($"Meshname: {meshName} vs {levelMesh[currentLevel]}");
        Debug.Log($"Color: {colorName} vs {levelColors[currentLevel].ToLower()}");

        return letterMatch && colorMatch && meshMatch;
    }

    public bool IsCorrectColor(string colorName)
    {
        return levelColors[currentLevel].ToLower() == "none" || colorName == levelColors[currentLevel].ToLower();
    }

    public bool IsCorrectShape(string meshName)
    {
        return levelMesh[currentLevel].ToLower() == "none" || meshName.Contains(levelMesh[currentLevel].ToLower());
    }
    public void OnCorrect()
    {
        StartCoroutine(NextLevelAfterDelay());
    }

    IEnumerator NextLevelAfterDelay()
    {
        yield return new WaitForSeconds(2f);
        currentLevel++;
        LoadLevel(currentLevel);
    }
}