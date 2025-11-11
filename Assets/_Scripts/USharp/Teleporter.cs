
using UdonSharp;
using UnityEditor.EditorTools;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

public class Teleporter : UdonSharpBehaviour
{
    [SerializeField] private Transform destination;
    public override void Interact()
    {
        Networking.LocalPlayer.TeleportTo(destination.transform.position, Quaternion.identity);
    }
}
