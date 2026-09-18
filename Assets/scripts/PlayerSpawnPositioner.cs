using Unity.Netcode;
using UnityEngine;

public class PlayerSpawnPositioner : MonoBehaviour
{
    [SerializeField] private Transform startSpot;



    public void MoveToStartSpot()
    {

        if (startSpot == null)
            startSpot = GameObject.FindWithTag("StartSpot")?.transform;

        if (startSpot != null)
            transform.SetPositionAndRotation(startSpot.position, startSpot.rotation);
    }
}