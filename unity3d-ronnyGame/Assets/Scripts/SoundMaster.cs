using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SoundMaster : Kami
{
    public AudioSource source;
    public int currentSongMeasures = 0;
    public float masterVolume = 100;
    public float[] volumeTypes = { 50, 50, 50 }; //effectVolume[0], UIVolume[1] musicVolume[2]

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
    public bool ChangeSong(int Measures = 4, AudioClip song = null, string songName = "")
    {
        AudioClip audio = null;
        if (Measures < 1)
        {
            Debug.LogError($"Less than 1 beats per measure assigned to song {song} or songName {songName}, can't alternate between variations if any");
            return false;
        }
        if (!song)
        {
            if (songName == string.Empty)
            {
                Debug.LogError("song parameter null and songName empty");
            }
            audio = FindSong(songName);
            if (!audio)
            {
                Debug.LogError($"Song name {songName} in resources not found");
                return false;
            }
        } else
        {
            audio = song;
        }
        currentSongMeasures = Measures;
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