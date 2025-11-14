using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;

public class VanillaAudioManager : MonoBehaviour
{
    [SerializeField] private AudioMixer mixer;

    string onboard_volume_var = "ONBOARD_VOLUME";
    string bedroom_volume_var = "BEDROOM_VOLUME";
    string forest_volume_var = "FOREST_VOLUME";

    public void SwitchToBedroom()
    {
        SetMuted(onboard_volume_var, true);
        SetMuted(bedroom_volume_var, false);
    }

    public void SwitchToForest()
    {
        SetMuted(bedroom_volume_var, true);
        SetMuted(forest_volume_var, false);
    }

    // helpers
    public void SetMuted(string var_name, bool mute)
    {
        FadeMixer(var_name, mute ? -80f : 0f);
    }
    private IEnumerator FadeMixer(string var_name, float target)
    {
        mixer.GetFloat(var_name, out float current);

        float t = 0f;
        while (t < 1f)
        {
            t += Time.deltaTime * 4f;
            float v = Mathf.Lerp(current, target, t);
            mixer.SetFloat(var_name, v);
            yield return null;
        }
    }
}
