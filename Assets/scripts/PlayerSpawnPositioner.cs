using Unity.Netcode;
using UnityEngine;

public class PlayerSpawnPositioner : NetworkBehaviour
{
    [SerializeField] private Transform startSpotClient;
    [SerializeField] private Transform startSpotHost;


    public void MoveToStartSpot()
    {

        if (startSpotClient != null && startSpotHost != null)
            if (IsServer)
            {
                transform.SetPositionAndRotation(startSpotHost.position, startSpotHost.rotation);       
            }
            else
            {
                transform.SetPositionAndRotation(startSpotClient.position, startSpotClient.rotation);
            }

    }
}