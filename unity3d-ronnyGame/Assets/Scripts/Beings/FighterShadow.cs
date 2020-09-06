using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FighterShadow : Fighter
{
    public SpriteRenderer spriteRenderer;
    public Animator animator;

    public override float AddToHealth(float change, Fighter causer)
    {
        foreach (Func<float, Fighter, float> a in onHitEffects.Values)
        {
            change = a.Invoke(change, causer);
        }
        //Debug.Log($"{this.name} has {onHitEffects.Count} onhit functions");
        change = change / this.defenseMultiplier;
        Debug.Log($"{this.name}'s health just got changed by {change} by {causer.name}");
        if (causer.gameObject.tag == "Party") // ugly but sensical solution, every fighther on health change should check if the causer was a party member (this accounts for all buff values etc)
        {
            BattleMaster battleMasterScript = this.GetComponentInParent<BattleMaster>();
            battleMasterScript.virtueExpectation.text = $"Expected Gain: {Mathf.RoundToInt(Mathf.Abs(change / 2))}";
        }
        if (this.gameObject.tag == "Player")
        {
            this.GetComponentInParent<BattleMaster>().expectedDamageTaken += change;
        }
        
        this.health += change;
        this.DeathCheck();
        return change;
    }
    public override void DeathCheck()
    {
        if (health < 0)
        {
            spriteRenderer.color += new Color(1, 0, 0, 0);
            Debug.LogWarning("Change redness to indicate death to skull sprite");
        }
    }
    public void InjectShadowData(Fighter source)
    {
        battlePosition = this.transform.position;
        damage = source.damage;
        damageMultiplier = source.damageMultiplier;
        defenseMultiplier = source.defenseMultiplier;
        health = source.health;
        isStunned = source.isStunned;
        currentEffects = source.currentEffects;
        onHitEffects = source.onHitEffects;
        this.spriteRenderer.sprite = source.GetComponent<SpriteRenderer>().sprite;
        this.spriteRenderer.color += new Color(0, 0, 0, -.5f);
        this.animator = source.GetComponent<Animator>();
        this.gameObject.tag = source.gameObject.tag;
        this.gameObject.name = $"{source.gameObject.name}(Shadow)";
        if (source.currentAction != null && source.currentAction.targets != null)
        {
            this.currentAction = source.currentAction.Clone();
            this.currentAction.originator = this.gameObject;
            this.currentAction.targets = (GameObject[]) source.currentAction.targets.Clone();
        }
        //this.animations = source.animations;
    }
    public IEnumerator PlayAnimations()
    {
        this.transform.position = battlePosition;
        StartCoroutine(BattleActionMove(this.currentAction));
        if (this.currentAction.animation != null)
        {
            this.currentAction.animation.Play();
        }
        yield return new WaitForSeconds(this.currentAction.duration);
        yield return true;
    }
    public void SimulateAction()
    {
        if (this.currentAction != null && this.currentAction.targets != null)
        {
            for (int i = 0; i < this.currentAction.targets.Length; i++)
            {
                for (int j = 0; j < this.currentAction.targets[i].transform.childCount; j++)
                {
                    if (this.currentAction.targets[i].transform.GetChild(j).name.Contains("Shadow"))
                    {
                        this.currentAction.targets[i] = this.currentAction.targets[i].transform.GetChild(j).gameObject;
                    }
                }
            }
            this.currentAction.ReEvaluateActionValues(this);
            StartCoroutine(this.currentAction.Execute());
            //currentAction.Execute();
        }
    }
    public void OnMouseEnter()
    {
        //StartCoroutine(PlayAnimations());
    }
}
