
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

public class LeaveRoomTeleporter : UdonSharpBehaviour
{
    [SerializeField] private RoomManager roomManager;
    [SerializeField] private Transform teleportDestination;
    public override void Interact()
    {
        //roomManager.ReleaseLocalRoomAndTeleport(teleportDestination);
    }
}
