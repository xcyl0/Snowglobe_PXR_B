using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

public class AudioManager : UdonSharpBehaviour
{
    [Header("Assign these in the inspector")]
    public AudioSource onboardSource;
    public AudioSource bedroomSource;
    public AudioSource forestSource;

    private void Start()
    {
        SwitchToOnboard();
    }

    public void SwitchToOnboard()
    {
        onboardSource.gameObject.SetActive(true);
        bedroomSource.gameObject.SetActive(false);
        forestSource.gameObject.SetActive(false);
    }

    public void SwitchToBedroom()
    {
        onboardSource.gameObject.SetActive(false);
        bedroomSource.gameObject.SetActive(true);
        forestSource.gameObject.SetActive(false);
    }

    public void SwitchToForest()
    {
        onboardSource.gameObject.SetActive(false);
        bedroomSource.gameObject.SetActive(false);
        forestSource.gameObject.SetActive(true);
    }
}
