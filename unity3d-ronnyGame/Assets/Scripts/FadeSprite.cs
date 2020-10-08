using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FadeSprite : MonoBehaviour
{
    public float startingAlphaValue, endingAlphaValue, totalTime, timeElapsed;
    public SpriteRenderer spr;
    void Start()
    {
        spr = this.GetComponent<SpriteRenderer>();
        if (spr == null)
        {
            Debug.LogError("Need sprite component!");
        } else
        {
            spr.color = new Color(1, 1, 1, 0);
            StartCoroutine(FadeIn());
        }
    }
    public IEnumerator FadeIn()
    {
        yield return new WaitForSeconds(1);
        while (timeElapsed < totalTime)
        {
            timeElapsed += Time.deltaTime;
            float valueT = timeElapsed / totalTime;
            spr.color = new Color(spr.color.r, spr.color.g, spr.color.b, Mathf.Lerp(startingAlphaValue, endingAlphaValue, Mathf.Sqrt(valueT)));
            yield return new WaitForEndOfFrame();
        }
        yield return null;
    }
}
