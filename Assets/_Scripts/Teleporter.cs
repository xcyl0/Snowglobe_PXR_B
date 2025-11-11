
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

public class Teleporter : UdonSharpBehaviour
{

    [SerializeField] private Transform destination;

    void Start()
    {
        
    }

    override public void Interact()
    {
        Debug.Log("Interacted with a teleporter");

        var localplayer = Networking.LocalPlayer;
        localplayer.TeleportTo(destination.position, destination.rotation);
    }
}
