using UnityEngine;
using UnityEngine.Audio;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance;

    public AudioMixer audioMixer; // Unity Audio Mixer kullan
    private void Awake()
    {
        if (Instance == null) Instance = this;
    }

    public void SetMasterVolume(float volume)
    {
        audioMixer.SetFloat("MasterVolume", Mathf.Log10(volume) * 20);
        PlayerPrefs.SetFloat("MasterVolume", volume);
    }

    public void LoadVolume()
    {
        float volume = PlayerPrefs.GetFloat("MasterVolume", 0.75f);
        SetMasterVolume(volume);
    }
    public void OnMasterSliderChanged(float value)
    {
        AudioManager.Instance.SetMasterVolume(value);
    }

}
