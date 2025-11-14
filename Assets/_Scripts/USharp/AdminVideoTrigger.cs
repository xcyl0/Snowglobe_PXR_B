
using UdonSharp;
using UnityEngine;
using VRC.SDK3.Components;
using VRC.SDK3.Video.Components.AVPro;
using VRC.SDKBase;
using VRC.Udon;
using VRC.Udon.Common.Interfaces;

public class AdminVideoTrigger : UdonSharpBehaviour
{
    [SerializeField] private VRCAVProVideoPlayer player;
    [SerializeField] private VRCUrl url;
    private void Start()
    {
        Debug.Log("local player display name: " + Networking.LocalPlayer.displayName);

        if (!Networking.IsInstanceOwner)
        {
            gameObject.GetComponent<MeshRenderer>().enabled = false;
            gameObject.GetComponent<Collider>().enabled = false;
        }
            
    }

    public override void Interact()
    {
        if (Networking.IsInstanceOwner)
        SendCustomNetworkEvent(NetworkEventTarget.All, nameof(PlayVideo));
    }

    public void PlayVideo()
    {
        player.PlayURL(url);
    }
}
