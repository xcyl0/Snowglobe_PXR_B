
using UdonSharp;
using UnityEngine;
using VRC.SDK3.Components;
using VRC.SDK3.Video.Components.AVPro;
using VRC.SDKBase;
using VRC.Udon;

public class AdminVideoTrigger : UdonSharpBehaviour
{
    [SerializeField] private VRCAVProVideoPlayer player;
    [SerializeField] private VRCUrl url;
    private void Start()
    {
        Debug.Log("local player display name: " + Networking.LocalPlayer.displayName);

        if (!Networking.IsInstanceOwner)
            gameObject.SetActive(false);
    }

    public override void Interact()
    {
        player.PlayURL(url);
    }
}
