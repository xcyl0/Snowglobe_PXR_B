using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

public class RoomManager : UdonSharpBehaviour
{
    [SerializeField] private Transform[] roomSpawns;
    [SerializeField] private Transform lobbySpawn;

    [UdonSynced]
    private int occupancy = 0;

    public override void Interact()
    {
        AssignAndTeleport();
    }

    public void AssignAndTeleport()
    {
        if (!Networking.IsOwner(gameObject))
        {
            Networking.SetOwner(Networking.LocalPlayer, gameObject);
        }
        int myIndex = occupancy;
        occupancy++;
        if (occupancy >= roomSpawns.Length)
            occupancy = 0;

        RequestSerialization();


        Transform target = roomSpawns[myIndex];


        Networking.LocalPlayer.TeleportTo(target.position, target.rotation);
    }
}