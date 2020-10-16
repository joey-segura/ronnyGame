using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FighterShadow : Fighter
{
    public SpriteRenderer spriteRenderer;
    public Animator animator;
    private Fighter parent;
    public bool dead = false, playing = false;

    public override void AddToHealth(int change, Fighter causer)
    {
        foreach (Func<int, Fighter, int> a in onHitEffects.Values)
        {
            change = a.Invoke(change, causer);
        }
        //Debug.Log($"{this.name} has {onHitEffects.Count} onhit functions");
        change = Mathf.FloorToInt(change / this.defenseMultiplier);
        Debug.Log($"{this.name}'s health just got changed by {change} by {causer.name}");
        if (this.gameObject.tag == "Player")
        {
            this.GetComponentInParent<BattleMaster>().expectedDamageTaken += change;
        }
        
        this.health += change;
        this.DeathCheck(causer.gameObject);
    }
    public override void DeathCheck(GameObject causer)
    {
        if (health <= 0 && !dead)
        {
            if (causer.gameObject.name.Contains("Joey") || causer.gameObject.name.Contains("Ritter")) // ugly but sensical solution
            {
                BattleMaster battleMasterScript = this.GetComponentInParent<BattleMaster>();
                battleMasterScript.expectedVirtueGain = Mathf.FloorToInt(Mathf.Abs(health));
                battleMasterScript.virtueExpectation.text = $"Expected Gain: {Mathf.FloorToInt(Mathf.Abs(health))}";
            }
            dead = true;
            parent.DeathTrigger(true);
            spriteRenderer.color += new Color(1, 0, 0, 0);
            Debug.LogWarning("Change redness to indicate death to skull sprite");
        }
    }
    public void InjectShadowData(Fighter source)
    {
        this.parent = source;
        battlePosition = this.transform.position;
        damage = source.damage;
        damageMultiplier = source.damageMultiplier;
        defenseMultiplier = source.defenseMultiplier;
        health = source.health;
        isStunned = source.isStunned;
        currentEffects = new Dictionary<int, Effect>(source.currentEffects);
        onHitEffects = new Dictionary<int, Func<int, Fighter, int>>(source.onHitEffects);
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
        playing = true;
        this.transform.position = battlePosition;
        CoroutineWithData move = new CoroutineWithData(this, MoveToBattleTarget(this.currentAction));
        while (!move.finished)
        {
            yield return new WaitForEndOfFrame();
        }
        if (this.currentAction.animation != null)
        {
            this.currentAction.animation.Play();
        }
        yield return new WaitForSeconds(this.currentAction.duration);
        CoroutineWithData moveBack = new CoroutineWithData(this, MoveToBattlePosition());
        while (!moveBack.finished)
        {
            yield return new WaitForEndOfFrame();
        }
        playing = false;
        yield return true;
    }
    public void ResetPosition()
    {
        this.transform.position = battlePosition;
    }
    public void SimulateAction()
    {
        if (this.currentAction != null && this.currentAction.targets != null)
        {
            for (int i = 0; i < this.currentAction.targets.Length; i++)
            {
                //if (this.currentAction.targets[i] != null)
                //{
                    for (int j = 0; j < this.currentAction.targets[i].transform.childCount; j++)
                    {
                        if (this.currentAction.targets[i].transform.GetChild(j).name.Contains("Shadow"))
                        {
                            this.currentAction.targets[i] = this.currentAction.targets[i].transform.GetChild(j).gameObject;
                        }
                    }
                //}
            }
            this.currentAction.ReevaluateActionValues(this);
            this.StartCoroutine(this.currentAction.Execute());
            //currentAction.Execute();
        }
    }
    public void OnMouseEnter()
    {
        //StartCoroutine(PlayAnimations());
    }
}
