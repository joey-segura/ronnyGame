﻿using UnityEngine;

[ExecuteInEditMode]
public class SpriteOutline : MonoBehaviour
{
    //public Color color = Color.white;
    public Color color = new Color(0, 0, 0, .5f);

    private SpriteRenderer spriteRenderer;

    private float timeElapsed = 0, timeLimit = .75f;
    private bool changeOutline, blink = false;

    void OnEnable()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        UpdateOutline(changeOutline = true);
    }
    void OnDisable()
    {
        UpdateOutline(changeOutline = false);
    }
    void Update()
    {
        if (blink)
        {
            timeElapsed += Time.deltaTime;
            if (timeElapsed > timeLimit)
            {
                UpdateOutline(changeOutline = !changeOutline);
                timeElapsed = 0;
            }
        }
    }
    void UpdateOutline(bool outline)
    {
        MaterialPropertyBlock mpb = new MaterialPropertyBlock();
        spriteRenderer.GetPropertyBlock(mpb);
        mpb.SetFloat("_Outline", outline ? 1f : 0);
        mpb.SetColor("_OutlineColor", color);
        spriteRenderer.SetPropertyBlock(mpb);
    }
}