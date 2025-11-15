
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

public class AutoTep_Player : UdonSharpBehaviour
{
    [SerializeField] private Transform target;
    [SerializeField] private float delay;
    public override void OnPlayerTriggerEnter(VRCPlayerApi player)
    {
        
        SendCustomEventDelayedSeconds("TeleportPlayer", delay);
        gameObject.GetComponent<Collider>().enabled = false;
    }

    public void TeleportPlayer()
    {
        Networking.LocalPlayer.TeleportTo(target.transform.position, Quaternion.identity);
    }
}
