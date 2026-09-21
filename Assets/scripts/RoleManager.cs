using Unity.Netcode;
using UnityEngine;

public class RoleManager : NetworkBehaviour
{
    [SerializeField] private GameObject adminDashboard;

    public override void OnNetworkSpawn()
    {
        if (adminDashboard != null)
            adminDashboard.SetActive(IsHost);
    }
}