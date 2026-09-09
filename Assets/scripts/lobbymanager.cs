using UnityEngine;

using UnityEngine.UI;
using TMPro;
using Unity.Netcode;
using Meta.XR.MultiplayerBlocks.Shared;

public class LobbyManager : MonoBehaviour
{
    [SerializeField] private CustomMatchmaking matchmaking;

    [Header("UI")]
    [SerializeField] private GameObject waitingPanel;
    [SerializeField] private TMP_Text statusText;
    [SerializeField] private Button startGameButton; // host-only, shown after client joins

    private const string LobbyName = "CubeQuest";

    void Start()
    {
        startGameButton.gameObject.SetActive(false);
        startGameButton.onClick.AddListener(OnStartGamePressed);

        AutoConnect();
    }

    async void AutoConnect()
    {
        statusText.text = "Looking for existing game...";

        var joinResult = await matchmaking.JoinOpenRoom(LobbyName);

        if (joinResult.IsSuccess)
        {
            // someone else already hosting — I'm the client
            statusText.text = "Joined game. Waiting for host to start...";
        }
        else
        {
            // no room found — I become host
            statusText.text = "No game found. Hosting...";

            var options = new CustomMatchmaking.RoomCreationOptions
            {
                IsPrivate = false, // must be false/open for JoinOpenRoom to find it
                MaxPlayersPerRoom = 2,
                LobbyName = LobbyName
            };

            var hostResult = await matchmaking.CreateRoom(options);

            if (hostResult.IsSuccess)
            {
                statusText.text = "Hosting. Waiting for a player...";
                NetworkManager.Singleton.OnClientConnectedCallback += OnClientJoined;
            }
            else
            {
                statusText.text = $"Failed: {hostResult.ErrorMessage}";
            }
        }
    }

    void OnClientJoined(ulong clientId)
    {
        if (!NetworkManager.Singleton.IsServer) return; // only the host reacts
        statusText.text = "Player connected!";
        startGameButton.gameObject.SetActive(true);
    }

    void OnStartGamePressed()
    {
        if (!NetworkManager.Singleton.IsServer) return;
        NetworkManager.Singleton.SceneManager.LoadScene("Intro", UnityEngine.SceneManagement.LoadSceneMode.Single);
    }
}