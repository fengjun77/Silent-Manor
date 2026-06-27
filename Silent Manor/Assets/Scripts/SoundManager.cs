using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SoundManager : Singleton<SoundManager>
{
    private AudioSource randomPitchAudioSource;
    private AudioSource audioSource;
    public Slider sfxSlider;

    [SerializeField] private SoundEffectGroup[] soundEffectGroups;
    private Dictionary<string, List<AudioClip>> soundDictionary;

    protected override void Awake()
    {
        base.Awake();

        AudioSource[] audioSources = GetComponents<AudioSource>();
        audioSource = audioSources[0];
        randomPitchAudioSource = audioSources[1];

        Init();
    }

    void Start()
    {
        sfxSlider.onValueChanged.AddListener(value => OnValueChanged());
    }

    private void Init()
    {
        soundDictionary = new Dictionary<string, List<AudioClip>>();
        foreach(var soundEffectGroup in soundEffectGroups)
        {
            soundDictionary[soundEffectGroup.name] = soundEffectGroup.audioClips;
        }
    }

    private AudioClip GetRandomClip(string name)
    {
        if(soundDictionary.ContainsKey(name))
        {
            List<AudioClip> audioClips = soundDictionary[name];

            if(audioClips.Count > 0)
            {
                return audioClips[Random.Range(0,audioClips.Count)];
            }
        }

        return null;
    }

    public void Play(string soundName, bool randomPitch)
    {
        AudioClip audioClip = GetRandomClip(soundName);
        if(audioClip != null)
        {
            if(randomPitch)
            {
                randomPitchAudioSource.pitch =Random.Range(.8f, 1.5f);
                randomPitchAudioSource.PlayOneShot(audioClip);
            }
            else
                audioSource.PlayOneShot(audioClip);
        }
    }

    public void SetVolume(float volume)
    {
        randomPitchAudioSource.volume = volume;
        audioSource.volume = volume;
    }

    public void OnValueChanged()
    {
        SetVolume(sfxSlider.value);
    }

    // 导出当前音量给存档调用
    public float GetCurrentSfxVolume()
    {
        return audioSource.volume;
    }

    // 读档时恢复音量
    public void LoadSfxVolume(float volume)
    {
        SetVolume(volume);
        // 如果有滑动条同步更新UI
        if (sfxSlider != null)
        {
            sfxSlider.value = volume;
        }
    }
}

[System.Serializable]
public struct SoundEffectGroup
{
    public string name;
    public List<AudioClip> audioClips;
}


