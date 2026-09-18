using Unity.Netcode;
using UnityEngine;

// Place this on a NetworkObject that already lives in your scene
// (e.g. alongside RoleManager), so it auto-spawns for host and client.
public class SessionFlowManager : NetworkBehaviour
{
    [SerializeField] private GameObject enterButton; // the poke-interactable button GameObject
    [SerializeField] private IntroTimelineController tutorial;

    private readonly NetworkVariable<int> _connectedCount = new(
        0,
        NetworkVariableReadPermission.Everyone,
        NetworkVariableWritePermission.Server);

    public override void OnNetworkSpawn()
    {
        if (enterButton != null)
            enterButton.SetActive(false);

        if (IsServer)
        {
            _connectedCount.Value = NetworkManager.Singleton.ConnectedClientsList.Count;
            NetworkManager.Singleton.OnClientConnectedCallback += HandleClientCountChanged;
            NetworkManager.Singleton.OnClientDisconnectCallback += HandleClientCountChanged;
        }

        _connectedCount.OnValueChanged += (_, current) =>
        {
            // Only the host should ever see this button
            if (IsHost && enterButton != null)
                enterButton.SetActive(current >= 2);
        };
    }

    public override void OnNetworkDespawn()
    {
        if (IsServer && NetworkManager.Singleton != null)
        {
            NetworkManager.Singleton.OnClientConnectedCallback -= HandleClientCountChanged;
            NetworkManager.Singleton.OnClientDisconnectCallback -= HandleClientCountChanged;
        }
    }

    private void HandleClientCountChanged(ulong _) =>
        _connectedCount.Value = NetworkManager.Singleton.ConnectedClientsList.Count;

    // Wire this to the poke interactable's "When Select" UnityEvent in the Inspector
    public void OnEnterPressed()
    {
        if (!IsHost) return;
        enterButton.SetActive(false);
        BeginSessionRpc();
    }

    [Rpc(SendTo.Everyone)]
    private void BeginSessionRpc()
    {
        var localPlayer = NetworkManager.Singleton.LocalClient?.PlayerObject;
        if (localPlayer != null && localPlayer.TryGetComponent(out PlayerSpawnPositioner positioner))
        {
            positioner.MoveToStartSpot();
            tutorial.Play();
        }
    }
}