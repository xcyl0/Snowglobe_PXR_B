
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

public class RoomManager : UdonSharpBehaviour
{
    [SerializeField] private Transform[] roomSpawns;
    [SerializeField] private Transform lobbySpawn;

    [UdonSynced] private int occupancy;

    public override void Interact() => AssignAndTeleport();

    public void AssignAndTeleport()
    {
        occupancy++;
        if (occupancy >= roomSpawns.Length)
            occupancy = 0;

        Networking.LocalPlayer.TeleportTo(roomSpawns[occupancy].position, Quaternion.identity);
    }
}
