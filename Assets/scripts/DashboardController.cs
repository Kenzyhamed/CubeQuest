using UnityEngine;
using TMPro;
using Unity.Netcode;

public class AdminDashboardController : NetworkBehaviour
{
    [Header("References")]
    [Tooltip("Assign your scene's GameManager here.")]
    [SerializeField] private GameManager gameManager;

    public void ToggleA()
    {
        if (!ValidateGameManager()) return;
        gameManager.isA.Value = !gameManager.isA.Value;
    }

    public void ToggleB1()
    {
        if (!ValidateGameManager()) return;
        gameManager.isB1.Value = !gameManager.isB1.Value;
    }

    public void ToggleB2()
    {
        if (!ValidateGameManager()) return;
        gameManager.isB2.Value = !gameManager.isB2.Value;
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