using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TestTextureSound : TextureSound
{
    public TestTextureSound()
    {
        this.textureName = "Test";
        this.textureWidth = 128;
        this.textureHeight = 128;
        LoadRects();
        LoadRectSoundLinks();
        LoadSounds();
        //load sounds
        this.AddToSoundBankList(this.textureName);
    }
    public void LoadRects()
    {
        this.rects = new Rect[1];
        this.rects[0].xMin = 0;
        this.rects[0].xMax = 64;
        this.rects[0].yMin = 64;
        this.rects[0].yMax = 128;
    }
    public void LoadRectSoundLinks()
    {
        this.rectSounds = new int[1][];
        this.rectSounds[0] = new int[3] { 0, 1, 2 };
    }
    public void LoadSounds()
    {
        this.sounds = new AudioClip[3];
        this.sounds[0] = Resources.Load($"Sounds/Scenes/Ritter/Test1", typeof(AudioClip)) as AudioClip;
        this.sounds[1] = Resources.Load($"Sounds/Scenes/Ritter/Test2", typeof(AudioClip)) as AudioClip;
        this.sounds[2] = Resources.Load($"Sounds/Scenes/Ritter/Test3", typeof(AudioClip)) as AudioClip;
    }
}
