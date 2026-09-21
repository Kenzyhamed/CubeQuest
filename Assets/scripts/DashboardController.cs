using UnityEngine;
using TMPro;
using Unity.Netcode;

public class AdminDashboardController : NetworkBehaviour
{
    [Header("References")]
    [Tooltip("Assign your scene's GameManager here.")]
    [SerializeField] private GameManager gameManager;
    [SerializeField] private TutorialModeController tutorialMode;
    [SerializeField] private FirstTutorial tut1;
    [SerializeField] private SecondTutorial tut2;
    [SerializeField] private ThirdTutorial tut3;


    public void ToggleA()
    {
        if (!ValidateGameManager()) return;
        if (IsServer)
        {
            gameManager.isA.Value = !gameManager.isA.Value;
        }

    }
    public void StartT1()
    {
        tutorialMode.BeginTutorial();
        tut1.Play();
    }
    public void StartT2()
    {
        tutorialMode.BeginTutorial();
        tut2.Play();
    }
    public void StartT3()
    {
        tutorialMode.BeginTutorial();
        tut2.Play();
    }

    public void ToggleB1()
    {
        if (!ValidateGameManager()) return;
        if (IsServer)
        {
            gameManager.isB1.Value = !gameManager.isB1.Value;
        }
    }

    public void ToggleB2()
    {
        if (!ValidateGameManager()) return;
        if (IsServer)
        {
            gameManager.isB2.Value = !gameManager.isB2.Value;
        }
    }

    public void ChangeLevel(GameObject poked)
    {
        if (!ValidateGameManager()) return;

        if (poked == null)
        {
            Debug.LogWarning("AdminDashboardController: ChangeLevel called with a null GameObject.");
            return;
        }
        if (!TryGetNumberFromChildText(poked, out int levelNumber))
        {
            Debug.LogWarning($"AdminDashboardController: could not read a number from '{poked.name}' or its children.");
            return;
        }

        gameManager.GoToLevel(levelNumber - 1); // GoToLevel already writes currentLevel.Value internally
    }

    private bool ValidateGameManager()
    {
        if (gameManager != null) return true;
        Debug.LogWarning("AdminDashboardController: GameManager reference not set.");
        return false;
    }

    private bool TryGetNumberFromChildText(GameObject obj, out int number)
    {
        number = -1;
        var tmp = obj.GetComponent<TMP_Text>();
        if (tmp != null && int.TryParse(tmp.text.Trim(), out number))
            return true;
        return false;
    }
}