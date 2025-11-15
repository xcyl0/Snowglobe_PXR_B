
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

public class EnterRoomTeleporter : UdonSharpBehaviour
{
    [SerializeField] private RoomManager roomManager;
    [SerializeField] private Transform target;
    [SerializeField] private float delay;
    public bool inworld = false;
    //[SerializeField] private Transform destination;
    [SerializeField] private AudioManager audioManager;
    override public void Interact()
    {
        Debug.Log("Interacted with a teleporter");
        roomManager.AssignAndTeleport();
        SendCustomEventDelayedSeconds("TeleportPlayer", delay);
        //var localplayer = Networking.LocalPlayer;
        //localplayer.TeleportTo(destination.position, destination.rotation);
        audioManager.SwitchToBedroom();
    }

    public void TeleportPlayer()
    {
        if (inworld)
            return;
        
        Networking.LocalPlayer.TeleportTo(target.transform.position, Quaternion.identity);
        audioManager.SwitchToForest();
    }
}
