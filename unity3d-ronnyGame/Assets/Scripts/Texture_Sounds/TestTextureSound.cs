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
        this.rectSounds[0] = new int[5] { 0, 1, 2, 3, 4 };
    }
    public void LoadSounds()
    {
        this.sounds = new AudioClip[5];
        this.sounds[0] = Resources.Load($"Sounds/Scenes/Ritter/Grass_1", typeof(AudioClip)) as AudioClip;
        this.sounds[1] = Resources.Load($"Sounds/Scenes/Ritter/Grass_2", typeof(AudioClip)) as AudioClip;
        this.sounds[2] = Resources.Load($"Sounds/Scenes/Ritter/Grass_3", typeof(AudioClip)) as AudioClip;
        this.sounds[3] = Resources.Load($"Sounds/Scenes/Ritter/Grass_4", typeof(AudioClip)) as AudioClip;
        this.sounds[4] = Resources.Load($"Sounds/Scenes/Ritter/Grass_5", typeof(AudioClip)) as AudioClip;
    }
}
