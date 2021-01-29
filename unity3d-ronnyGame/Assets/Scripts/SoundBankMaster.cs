using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class TextureSound
{
    public string textureName;
    public AudioClip[] sounds;
    public int textureWidth, textureHeight;
    public Rect[] rects;
    public int[][] rectSounds;
    public void AddToSoundBankList(string textureName)
    {
        if (!SoundBankMaster.SoundTextures.ContainsKey(textureName))
        {
            SoundBankMaster.SoundTextures.Add(textureName, this);
        }
    }
    public AudioClip GetSoundAtTexCoord(Vector2 pixelCoord)
    {
        for (int i = 0; i < this.rects.Length; i++)
        {
            Vector2 actual = new Vector2(pixelCoord.x * this.textureWidth, pixelCoord.y * this.textureHeight);
            if (this.rects[i].Contains(actual))
            {
                int soundIndex = Random.Range(0, this.rectSounds[i].Length);
                //Debug.Log(sounds[this.rectSounds[i][soundIndex]]);
                return sounds[this.rectSounds[i][soundIndex]];
            }
        }
        Debug.LogError($"No Sound Found for {this.textureName}");
        return null;
    }
}

public class SoundBankMaster : Kami
{
    public static Dictionary<string, TextureSound> SoundTextures;


    public void LoadSounds()
    {
        SoundTextures = new Dictionary<string, TextureSound>();
        LoadSoundTextures();
    }

    public AudioClip GetSoundOfTexture(string name, Vector2 texCoord)
    {
        foreach (string textName in SoundTextures.Keys)
        {
            if (textName == name)
            {
                TextureSound textureSound;
                SoundTextures.TryGetValue(name, out textureSound);
                return textureSound.GetSoundAtTexCoord(texCoord);
            }
        }
        return null;
    }

    private void LoadSoundTextures()
    {
        switch (sceneMaster.currentSceneName)
        {
            case "Ritter":
                new TestTextureSound();
                break;
            case "Joey":
                break;
            default:
                break;
        }
    }
}
