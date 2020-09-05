using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AnimateTextures : MonoBehaviour
{
    public Renderer renderer;
    public Texture[] textures;
    public int currentTexture;

    void Start()
    {
        StartCoroutine(AnimateTexture());
    }
    public IEnumerator AnimateTexture()
    {
        currentTexture++;
        currentTexture %= textures.Length;
        renderer.material.mainTexture = textures[currentTexture];
        yield return new WaitForSeconds(.25f);
        StartCoroutine(AnimateTexture());
    }
}
