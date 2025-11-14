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

    [Header("Fade settings")]
    public float fadeDuration = 0.25f;  // seconds

    // fade state
    private bool _isFading;
    private float _fadeT;
    private AudioSource _fadeOut;
    private AudioSource _fadeIn;
    private float _fadeOutStartVol;
    private float _fadeInStartVol;

    private void Start()
    {
        // Optional: ensure only one is active at start
        if (onboardSource != null) onboardSource.volume = 1f;
        if (bedroomSource != null) bedroomSource.volume = 0f;
        if (forestSource != null) forestSource.volume = 0f;

        if (onboardSource != null && !onboardSource.isPlaying) onboardSource.Play();
        if (bedroomSource != null && !bedroomSource.isPlaying) bedroomSource.Play();
        if (forestSource != null && !forestSource.isPlaying) forestSource.Play();
    }

    private void Update()
    {
        if (!_isFading) return;
        if (fadeDuration <= 0f)
        {
            _fadeT = 1f;
        }
        else
        {
            _fadeT += Time.deltaTime / fadeDuration;
        }

        float t = Mathf.Clamp01(_fadeT);

        if (_fadeOut != null)
            _fadeOut.volume = Mathf.Lerp(_fadeOutStartVol, 0f, t);
        if (_fadeIn != null)
            _fadeIn.volume = Mathf.Lerp(_fadeInStartVol, 1f, t);

        if (t >= 1f)
        {
            _isFading = false;
        }
    }

    private void StartCrossFade(AudioSource from, AudioSource to)
    {
        if (from == to || to == null) return;

        if (from != null && !from.isPlaying) from.Play();
        if (to != null && !to.isPlaying) to.Play();

        _fadeOut = from;
        _fadeIn = to;

        _fadeOutStartVol = from != null ? from.volume : 0f;
        _fadeInStartVol = to != null ? to.volume : 0f;

        _fadeT = 0f;
        _isFading = true;
    }

    // ========== Public API ==========

    public void SwitchToOnboard()
    {
        StartCrossFade(bedroomSource, onboardSource);
        StartCrossFade(forestSource, onboardSource);
    }

    public void SwitchToBedroom()
    {
        StartCrossFade(onboardSource, bedroomSource);
        StartCrossFade(forestSource, bedroomSource);
    }

    public void SwitchToForest()
    {
        StartCrossFade(onboardSource, forestSource);
        StartCrossFade(bedroomSource, forestSource);
    }
}
