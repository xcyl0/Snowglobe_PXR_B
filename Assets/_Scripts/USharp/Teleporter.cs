
using UdonSharp;
using UnityEngine;
using UnityEngine.Events;
using VRC.SDKBase;
using VRC.Udon;
enum AudioSwitchType
{
    None,
    ToBedroom,
    ToForest
}

public class Teleporter : UdonSharpBehaviour
{
    [SerializeField] private Transform destination;
    [SerializeField] private AudioManager audioManager;

    [SerializeField] private AudioSwitchType optionalAudioSwitch;

    public override void Interact()
    {
        Networking.LocalPlayer.TeleportTo(destination.transform.position, Quaternion.identity);

        switch (optionalAudioSwitch)
        {
            case AudioSwitchType.None:
                break;
            case AudioSwitchType.ToBedroom:
                audioManager.SwitchToBedroom();
                break;
            case AudioSwitchType.ToForest:
                audioManager.SwitchToForest();
                break;
            default:
                break;


        }
    }
}
