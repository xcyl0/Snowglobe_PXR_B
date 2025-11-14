
using UdonSharp;
using UnityEngine;
using VRC.SDK3.Video.Components.AVPro;
using VRC.SDKBase;
using VRC.Udon;

public class AdminVideoTrigger : UdonSharpBehaviour
{
    [SerializeField] private VRCAVProVideoPlayer player;

    private void Start()
    {
        Debug.Log("local player display name: "+ Networking.LocalPlayer.displayName);

        if (!Networking.IsInstanceOwner)
            gameObject.SetActive(false);
    }

    public override void Interact()
    {
        player.Play();
    }
}
