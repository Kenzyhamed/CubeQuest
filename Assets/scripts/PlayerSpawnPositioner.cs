using Unity.Netcode;
using UnityEngine;

public class PlayerSpawnPositioner : NetworkBehaviour
{
    [SerializeField] private Transform waitingSpot; // drag scene anchor, or find by tag below
    [SerializeField] private Transform startSpot;

    public override void OnNetworkSpawn()
    {
        if (!IsOwner) return; // only move your own rig, never someone else's

        if (waitingSpot == null)
            waitingSpot = GameObject.FindWithTag("WaitingSpot")?.transform;

        if (waitingSpot != null)
            transform.SetPositionAndRotation(waitingSpot.position, waitingSpot.rotation);
    }

    public void MoveToStartSpot()
    {
        if (!IsOwner) return;

        if (startSpot == null)
            startSpot = GameObject.FindWithTag("StartSpot")?.transform;

        if (startSpot != null)
            transform.SetPositionAndRotation(startSpot.position, startSpot.rotation);
    }
}