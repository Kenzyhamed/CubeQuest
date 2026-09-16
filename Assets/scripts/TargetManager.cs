using Meta.XR.MultiplayerBlocks.Shared;
using Unity.Netcode;
using UnityEngine;

public class TargetManager : NetworkBehaviour
{
    public static TargetManager Instance;

    [Header("References")]
    public GameManager gameManager;
    public CustomMatchmaking customMatchmaking;

    [Header("Testing - Networking")]
    [Tooltip("Leave EMPTY to host a new room (first person). " +
             "Paste the host's room token here to join as the client (second person).")]
    public string roomTokenToJoin;

    [Header("Sounds")]
    private readonly string[] levelColors = {"none", "none", "none", "none", "none", "none","blue", "blue", "green", "green", "green", "blue", "blue",  "red", "blue", "blue", "red", "green", "red", "green" };
    private readonly string[] levelMesh = {"none", "none", "none", "none", "none", "none","cube", "cube", "cube","cube", "cube", "cube", "sphere", "cylinder", "cylinder", "sphere", "cylinder", "cylinder", "sphere", "sphere"};
    private readonly string[] levelLetters = {"M", "E", "P", "K", "N", "L", "F", "R", "P", "M", "E", "K", "M", "F", "P", "L", "F", "R", "P", "N"};

    private void Awake()
    {
        Instance = this;
    }

    async void Start()
    {
        if (string.IsNullOrEmpty(roomTokenToJoin))
        {
            var result = await customMatchmaking.CreateRoom();
            if (result.IsSuccess)
            {
                Debug.Log($"[TargetManager] Hosting room. Give this token to your client: {result.RoomToken}");
            }
            else
            {
                Debug.LogError($"[TargetManager] Failed to create room: {result.ErrorMessage}");
            }
        }
        else
        {
            var result = await customMatchmaking.JoinRoom(roomTokenToJoin, null);
            if (!result.IsSuccess)
            {
                Debug.LogError($"[TargetManager] Failed to join room: {result.ErrorMessage}");
            }
        }
    }

    public override void OnNetworkSpawn()
    {
        if (IsServer)
        {
            NetworkManager.Singleton.OnClientConnectedCallback += OnClientConnected;
            CheckIfBothPlayersConnected();
        }
    }

    public override void OnNetworkDespawn()
    {
        if (NetworkManager.Singleton != null)
        {
            NetworkManager.Singleton.OnClientConnectedCallback -= OnClientConnected;
        }
    }

    private void OnClientConnected(ulong clientId)
    {
        CheckIfBothPlayersConnected();
    }

    private void CheckIfBothPlayersConnected()
    {
        if (!IsServer)
        {
            return;
        }

        if (NetworkManager.Singleton.ConnectedClientsIds.Count >= 2)
        {
            NetworkManager.Singleton.OnClientConnectedCallback -= OnClientConnected;
            SetTargetsRpc();
        }
    }

    [Rpc(SendTo.ClientsAndHost)]
    private void SetTargetsRpc()
    {
        SetTargets();
    }

    public void SetTargets()
    {
        gameManager.SetTargets(levelLetters, levelColors, levelMesh);
    }
}