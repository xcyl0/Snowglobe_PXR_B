
using UdonSharp;
using UnityEngine;
using UnityEngine.Events;
using VRC.SDKBase;
using VRC.Udon;

public class Teleporter : UdonSharpBehaviour
{
    [SerializeField] private Transform destination;
    [SerializeField] private AudioManager audioManager;
    [SerializeField] private EnterRoomTeleporter tp;


    public override void Interact()
    {
        Networking.LocalPlayer.TeleportTo(destination.transform.position, Quaternion.identity);
        tp.inworld = true;


        audioManager.SwitchToForest();
    }
}
