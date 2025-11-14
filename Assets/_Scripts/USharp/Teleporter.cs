
using UdonSharp;
using UnityEngine;
using UnityEngine.Events;
using VRC.SDKBase;
using VRC.Udon;

public class Teleporter : UdonSharpBehaviour
{
    [SerializeField] private Transform destination;
    [SerializeField] private UnityEvent optional_event;

    public override void Interact()
    {
        Networking.LocalPlayer.TeleportTo(destination.transform.position, Quaternion.identity);
        if (optional_event != null)
            optional_event.Invoke();
    }
}
