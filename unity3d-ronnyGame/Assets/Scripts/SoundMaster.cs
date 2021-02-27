using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SoundMaster : Kami
{
    public AudioSource source;
    public int currentSongMeasures = 0;
    public float masterVolume = 100;
    public float[] volumeTypes = { 100, 50, 50 }; //effectVolume[0], UIVolume[1] musicVolume[2]

    /*public new void Awake()
    {
        Configs.LoadSoundSettings(); need something like this in the future maybe?
        base.Awake();
    }*/
    public IEnumerator AlternateSongVariation(AudioClip newVariation)
    {
        if (newVariation.length == source.clip.length)
        {
            float measureDuration = source.clip.length / currentSongMeasures;
            int downBeat = Mathf.CeilToInt(source.time / measureDuration);
            yield return new WaitUntil(() => source.time >= downBeat * measureDuration);
            source.clip = newVariation;
            source.volume = (volumeTypes[2] / 100);
            source.Play();
        } else
        {
            Debug.LogError($"Incompatible Alternations: alternation is {newVariation.name} and current source is {source.clip.name}");
        }
        yield return true;
    }
    public bool ChangeSong(AudioClip song = null, string songName = "")
    {
        AudioClip audio = song ? song : FindSong(songName);
        source.clip = audio;
        source.volume = (volumeTypes[2] / 100);
        source.Play();
        return true;
    }
    public void ChangeMasterVolume(float volume)
    {
        masterVolume = volume;
    }
    public void ChangeVolume(float volume, int volumeType)
    {
        volumeTypes[volumeType] = volume;
    }
    public AudioClip FindSong(string songName)
    {
        AudioClip song = Resources.Load($"Sounds/Songs/{songName}") as AudioClip;
        if (song == null)
            Debug.LogError($"Song name '{songName}' could not be loaded check path Sounds/Songs/{songName}");
        return song;
    }
    public void InjectData(SoundMasterJson data)
    {
        this.masterVolume = data.masterVolume;
        data.volumeTypes.CopyTo(this.volumeTypes, 0);
    }
    public bool PlaySound(AudioClip sound, int volumeType)
    {
        float volume = (masterVolume / 100) * (volumeTypes[volumeType] / 100);
        if (sound == null)
        {
            Debug.LogError("sound source is bad");
            return false;
        }
        source.PlayOneShot(sound, volume);
        return true;
    }
}