
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

public class RoomManager : UdonSharpBehaviour
{
    [SerializeField] private Transform[] roomSpawns;
    [SerializeField] private Transform lobbySpawn;

    [UdonSynced] private int[] roomOccupants;

    // locals for the user
    private int localRoomAssignmentIndex = -1;
    private VRCPlayerApi localPlayer;

    void Start()
    {
        localPlayer = Networking.LocalPlayer;

        if (!Networking.IsOwner(gameObject))
            return;

        if (roomSpawns == null) roomSpawns = new Transform[0];

        if (roomOccupants == null || roomOccupants.Length != roomSpawns.Length)
        {
            roomOccupants = new int[roomSpawns.Length];
            for (int i = 0; i < roomSpawns.Length; i++)
            {
                roomOccupants[i] = -1;
                RequestSerialization();
            }


        }
    }

    public override void Interact() => AssignAndTeleport();

    public void AssignAndTeleport()
    {
        if (localPlayer == null) localPlayer = Networking.LocalPlayer; ;
        if (localPlayer == null) return;

        // take ownership, claim a free room, serialize
        Networking.SetOwner(localPlayer, gameObject);
        if (roomOccupants == null || roomOccupants.Length != roomSpawns.Length)
        {
            roomOccupants = new int[roomSpawns.Length];
            for (int i = 0; i < roomOccupants.Length; i++) roomOccupants[i] = -1;
        }

        // find first free room
        int pId = localPlayer.playerId;
        int freeIdx = -1;

        for (int i = 0; i <= roomOccupants.Length; i++)
        {
            if (roomOccupants[i] == -1)
            {
                freeIdx = i;
                break;
            }
        }

        // no rooms available
        if (freeIdx == -1)
        {
            if (lobbySpawn != null) localPlayer.TeleportTo(lobbySpawn.position, Quaternion.identity);
            return;
        }

        roomOccupants[freeIdx] = pId;
        localRoomAssignmentIndex = freeIdx;
        RequestSerialization();

        // teleport
        if (roomSpawns[freeIdx] != null) localPlayer.TeleportTo(roomSpawns[freeIdx].position, Quaternion.identity);
        else if (lobbySpawn != null) localPlayer.TeleportTo(lobbySpawn.position, Quaternion.identity);
    }

    public override void OnPlayerLeft(VRCPlayerApi player)
    {
        if (!Networking.IsOwner(gameObject) || player == null) return;

        int leftId = player.playerId;
        if (roomOccupants == null) return;

        bool changed = false;
        for (int i = 0; i < roomOccupants.Length; i++)
        {
            if (roomOccupants[i] == leftId)
            {
                roomOccupants[i] = -1;
                changed = true;
            }
        }
        if (changed) RequestSerialization();
    }

    public override void OnOwnershipTransferred(VRCPlayerApi newOwner)
    {
        if (Networking.IsOwner(gameObject))
        {
            if (roomOccupants == null || roomOccupants.Length != roomSpawns.Length)
            {
                roomOccupants = new int[roomSpawns.Length];
                for (int i = 0; i < roomOccupants.Length; i++) roomOccupants[i] = -1;
                RequestSerialization();
            }
        }
    }

    public override void OnDeserialization()
    {
        // Re-detect our assigned slot after sync (useful on late join)
        if (localPlayer == null) localPlayer = Networking.LocalPlayer;
        if (localPlayer == null || roomOccupants == null) return;

        localRoomAssignmentIndex = -1;
        int pId = localPlayer.playerId;
        for (int i = 0; i < roomOccupants.Length; i++)
        {
            if (roomOccupants[i] == pId)
            {
                localRoomAssignmentIndex = i;
                break;
            }
        }
    }

    // Optional public method for a Return pad to call
    public void ReleaseLocalRoomAndTeleport(Transform destination)
    {
        if (localPlayer == null) localPlayer = Networking.LocalPlayer;
        if (localPlayer == null) return;

        // Free our slot (owner needed)
        Networking.SetOwner(localPlayer, gameObject);
        if (roomOccupants != null && localRoomAssignmentIndex >= 0 && localRoomAssignmentIndex < roomOccupants.Length)
        {
            if (roomOccupants[localRoomAssignmentIndex] == localPlayer.playerId)
            {
                roomOccupants[localRoomAssignmentIndex] = -1;
                RequestSerialization();
            }
        }
        localRoomAssignmentIndex = -1;

        localPlayer.TeleportTo(destination.position, Quaternion.identity);
    }

}
