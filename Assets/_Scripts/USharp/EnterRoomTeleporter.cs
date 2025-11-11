
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

public class EnterRoomTeleporter : UdonSharpBehaviour
{
    [SerializeField] private RoomManager roomManager;
    //[SerializeField] private Transform destination;

    override public void Interact()
    {
        Debug.Log("Interacted with a teleporter");
        roomManager.AssignAndTeleport();
        //var localplayer = Networking.LocalPlayer;
        //localplayer.TeleportTo(destination.position, destination.rotation);
    }
}
