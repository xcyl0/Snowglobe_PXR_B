
using UdonSharp;
using Unity.Burst.Intrinsics;
using UnityEngine;
using UnityEngine.Apple;
using VRC.SDKBase;
using VRC.Udon;
using VRC.Udon.Common.Interfaces;

public class AdminPlayerTP : UdonSharpBehaviour
{
    [SerializeField] Transform destination;

    void Start()
    {
        if (!Networking.LocalPlayer.displayName.Equals("010n"))
            gameObject.SetActive(false);
    }


    public override void Interact()
    {

        VRCPlayerApi[] players = new VRCPlayerApi[80];
        VRCPlayerApi.GetPlayers(players);

        for (int i = 0; i < players.Length; i++)
        {
            VRCPlayerApi p = players[i];
            if (p == null) { continue; }
            if (p.isInstanceOwner) { continue; }

            SendCustomNetworkEvent(NetworkEventTarget.Others, nameof(RequestTeleport));
        }

    }

    public void RequestTeleport()
    {
        VRCPlayerApi local = Networking.LocalPlayer;
        if (local == null)
            return;

        local.TeleportTo(destination.position, destination.rotation);
    }


}
