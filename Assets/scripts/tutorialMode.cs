using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using Oculus.Interaction;
public class TutorialModeController : NetworkBehaviour
{
    [SerializeField] private GameManager gameManager;

    [Tooltip("All cube GameObjects that should be non-grabbable during tutorial.")]
    [SerializeField] public List<GameObject> allCubes;

    [Tooltip("Other systems to pause during tutorial (TrialManager, SoundStateMachine, etc.)")]
    [SerializeField] private List<MonoBehaviour> mechanismsToDisable;

    private NetworkVariable<bool> _tutorialActive = new(
        false, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);

    public bool IsTutorialActive => _tutorialActive.Value;

    // Call this server-side when the tutorial should begin (e.g. from OnEnterPressed/BeginSessionRpc)
    public void BeginTutorial()
    {
        if (!IsServer) return;
        _tutorialActive.Value = true;
        gameManager.isA.Value = false;
        gameManager.isB1.Value = false;
        gameManager.isB2.Value = false;
        SetCubesGrabbableRpc(false);
    }

    // Call this once the tutorial sequence is actually finished
    public void EndTutorial()
    {
        if (!IsServer) return;

        _tutorialActive.Value = false;
        SetCubesGrabbableRpc(true);

    }

    [Rpc(SendTo.Everyone)]
    private void SetCubesGrabbableRpc(bool grabbable)
    {
        foreach (var cube in allCubes)
        {
            if (cube == null) continue;
            var grab = cube.GetComponent<Grabbable>(); // swap for your actual grab component
            if (grab != null) grab.enabled = grabbable;
        }
    }

    [Rpc(SendTo.Everyone)]
    private void SetMechanismsEnabledRpc(bool enabledState)
    {
        foreach (var mechanism in mechanismsToDisable)
        {
            if (mechanism != null) mechanism.enabled = enabledState;
        }
    }
}