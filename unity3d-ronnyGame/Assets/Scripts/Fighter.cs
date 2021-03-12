using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

[System.Serializable]

public class Fighter : Being
{
    public float health, damageMultiplier = 1;
    public int damage, defense = 0;
    public bool isStunned = false, isPoisoned = false, playingWalkingSound = false;
    protected bool isBattle = false;
    public const string SYMBOLS = "αβγδεζηθικλμνξοπρστυφχψω!@#$%^.,&*ABCDEFGHIJKLMNOPQRSTUVWXYZ";

    private Shader defaultShader;

    public string[] party;
    public FighterAction currentAction = null;
    public Dictionary<int, Effect> currentEffects = new Dictionary<int, Effect>();
    //private List<Effect> currentEffects = new List<Effect>();

    public Vector3 battlePosition;
    public Dictionary<int, Func<int, Fighter, int>> onHitEffects = new Dictionary<int, Func<int, Fighter, int>>();
    public Dictionary<int, Func<int, Fighter, int>> onAttackEffects = new Dictionary<int, Func<int, Fighter, int>>();

    protected AudioClip[] onHitSounds;
    protected AudioClip[] attackSounds;
    protected AudioClip[] idleSounds; // don't know if we will need these ones but maybe like if they are fishing or something mumbling or something

    public void AddEffect(Fighter fighter, Effect effect)
    {
        effect.Affliction(fighter);
        currentEffects.Add(effect.GetKey(), effect);
        currentEffects.OrderByDescending(x => effect.duration);
    }
    public virtual void AddToHealth(int change, Fighter causer)
    {
        bool heal = Mathf.Sign(change) > 0 ? true : false;
        foreach(Func<int, Fighter, int> a in onHitEffects.Values)
        {
            change = a.Invoke(change, causer);
        }
        Debug.Log($"{this.name} has {onHitEffects.Count} onhit functions");
        if (!heal)
        {
            change = change + this.defense > 0 ? 0 : change + this.defense;
            foreach (Func<int, Fighter, int> a in causer.onAttackEffects.Values)
            {
                a.Invoke(change, causer);
            }
        }
        Debug.Log($"{this.name}'s health just got changed by {change}");
        if (change < 0)
        {
            this.PlayOnHitSound();
            StartCoroutine(GetHitJiggle());
        }
        this.health += change;
        this.DeathCheck(causer.gameObject);
    }
    public bool AddToOnAttackEffects(int key, Func<int, Fighter, int> method)
    {
        if (onAttackEffects.ContainsKey(key))
        {
            return false;
        }
        else
        {
            onAttackEffects.Add(key, method);
            return true;
        }
    }
    public bool AddToOnHitEffects(int key, Func<int, Fighter, int> method)
    {
        if (onHitEffects.ContainsKey(key))
        {
            return false;
        } else
        {
            onHitEffects.Add(key, method);
            return true;
        }
    }
    public virtual void BattleStart() { }
    public virtual void DeathCheck(GameObject causer)
    {
        if (this.health <= 0)
        {
            
            this.DeathTrigger(false);
            if (this.name == "Joey")
            {
                this.GetComponent<Joey>().RageMode();
                return;
            }
            BattleMaster battleMaster = this.transform.parent.GetComponentInParent<BattleMaster>();
            if (causer.tag == "Party")
            {
                battleMaster.AddToVirtue(Mathf.FloorToInt(Mathf.Abs(this.health)));
            }
            battleMaster.RemoveMemberByID(this.ID);
            battleMaster.BattleEndCheck();
            this.DestroyBeing();
            return;
        }
    }
    public IEnumerator MoveToBattleTarget(FighterAction action) {
        float distance = 0;
        Vector3 newPos;
        if (action.targets[0] != null)
        {
            if (this.transform.position.x > 0)
            {
                distance = -.25f;
            }
            else
            {
                distance = .25f;
            }
            if (action.IsActionAOE())
            {
                newPos = new Vector3(this.transform.position.x + distance, this.transform.position.y, this.transform.position.z);
            }
            else if (action.targets[0] == this.gameObject)
            {
                newPos = battlePosition;
            }
            else
            {
                newPos = Vector3.Lerp(this.transform.position, action.targets[0].GetComponent<Fighter>().battlePosition, .75f);
            }
            while (this.gameObject != null && this.transform.position != newPos)
            {
                this.transform.position = Vector3.MoveTowards(this.transform.position, newPos, 15 * Time.deltaTime);
                yield return new WaitForEndOfFrame();
            }
        }
        yield return true;
    }
    public IEnumerator MoveToBattlePosition()
    {
        while (this != null && this.transform.position != battlePosition )
        {
            this.transform.position = Vector3.MoveTowards(this.transform.position, this.battlePosition, 15 * Time.deltaTime);
            yield return new WaitForEndOfFrame();
        }
        yield return true;
    }
    
    public void TickEffects() // happens every fighters turn
    { // do we want effects to tick and to be removed?
        /*Dictionary<int, Effect> newEffects = new Dictionary<int, Effect>();
        for (int i = 0; i < currentEffects.Count; i++)
        {
            KeyValuePair<int, Effect> effect = currentEffects.ElementAt(i);
            effect.Value.OnTick(this);
            effect.Value.duration -= 1;
            if (effect.Value.duration > 0)
            {
                newEffects.Add(effect.Key, effect.Value);
            } else
            {
                effect.Value.Cleanse(this);
            }
        }
        currentEffects = newEffects;*/
    }
    public virtual GameObject ChooseTarget(ListBeingData allFighters) //chooses a target at random!
    {
        int targetIndex = UnityEngine.Random.Range(0, allFighters.BeingDatas.Count);
        return allFighters.BeingDatas[targetIndex].gameObject;
    }
    private static int CompareEffectsByDuration(Effect x, Effect y)
    {
        if (x.duration > y.duration)
        {
            return 1;
        } else if (x.duration < y.duration)
        {
            return -1;
        } else
        {
            return 0;
        }
    }
   
    public virtual void DeathTrigger(bool shadow)
    {
        //this is here so that it can be instantiated through general terms
    }
    public IEnumerator FlashWhite(float seconds)
    {
        Shader whiteGUI = Shader.Find("GUI/Text Shader");
        SpriteRenderer self = this.GetComponent<SpriteRenderer>();
        self.material.shader = whiteGUI;
        yield return new WaitForSeconds(seconds);
        self.material.shader = defaultShader;
    }

    public IEnumerator GetHitJiggle()
    {
        Vector3 pos = this.transform.position;
        float distance = .2f;
        StartCoroutine(FlashWhite(.4f));
        this.transform.position += new Vector3(distance, 0, 0);
        yield return new WaitForSeconds(.075f);
        this.transform.position -= new Vector3(2 * distance, 0, 0);
        yield return new WaitForSeconds(.075f);
        this.transform.position += new Vector3(distance, 0, 0);
    }
    public FighterAction GetIntention(ListBeingData allFighters)
    {
        
        if (this.isStunned)
        {
            this.TickEffects();
            FighterAction skp = new Skip(1, null);
            skp.originator = this.gameObject;
            Debug.Log("stunned!");
            return skp;
        }

        FighterAction action = this.TurnAction(allFighters);
        return action;
    }
    public string[] GetParty()
    {
        if (party != null)
        {
            return party;
        }
        else
        {
            string[] party = { this.gameObject.name };
            return party;
        }
    }
    public FighterShadow GetShadow()
    {
        return this.transform.GetComponentInChildren<FighterShadow>();
    }
    public bool HasEffect(string name)
    {
        foreach (Effect effect in currentEffects.Values)
        {
            if (effect.name.Contains(name))
            {
                return true;
            }
        }
        return false;
    }
    public virtual void InitializeBattle()
    {
        this.isBattle = true;
        this.battlePosition = this.transform.position;
        this.RemoveAllEffects();
        this.defaultShader = this.GetComponent<SpriteRenderer>().material.shader;
        this.LoadAttackSounds();
        this.LoadOnHitSounds();
        return; // this is for any enemy script to disable their movement script once the battle starts
    }
    public virtual void LoadOnHitSounds()
    {
        this.onHitSounds = new AudioClip[4]; // default sounds, the path of files might change
        this.onHitSounds[0] = Resources.Load($"Sounds/Scenes/Joey/Fighters/Grunt_1", typeof(AudioClip)) as AudioClip; 
        this.onHitSounds[1] = Resources.Load($"Sounds/Scenes/Joey/Fighters/Grunt_2", typeof(AudioClip)) as AudioClip;
        this.onHitSounds[2] = Resources.Load($"Sounds/Scenes/Joey/Fighters/Grunt_3", typeof(AudioClip)) as AudioClip;
        this.onHitSounds[3] = Resources.Load($"Sounds/Scenes/Joey/Fighters/Grunt_4", typeof(AudioClip)) as AudioClip;
    }
    public virtual void LoadAttackSounds()
    {

    }
    private new void OnGUI()
    {
        if (isBattle)
        {
            Rect rect;
            Vector3 fighterScreenPos = Camera.main.WorldToScreenPoint(battlePosition);
            float width = 150, height = 100;
            if (fighterScreenPos.x > (Screen.width / 2))
            {
                
                rect = new Rect(Screen.width - (Screen.width / 5), (Screen.height - fighterScreenPos.y) - height / 2, width, height);
            }
            else
            {
                rect = new Rect((Screen.width / 5) - width, (Screen.height - fighterScreenPos.y) - height / 2, width, height);
            }
            if (this.name == "Joey" && this.GetComponent<Joey>().rage)
            {
                string rName = null;
                for (int i = 0; i < 8; i++)
                {
                    rName += SYMBOLS.Substring(UnityEngine.Random.Range(0, SYMBOLS.Length), 1);
                }
                GUI.Box(rect, $"Name: {rName}\n HP: ???\n Damage: ???\n Defense: ???\n Action: Wrath");
            } else
            {
                GUI.Box(rect, $"Name: {name}\n HP: {health}\n Damage: {damage}\n Defense: {defense}\n Action: {(this.currentAction != null ? this.currentAction.name : string.Empty)}");
            }
            

            int effectIndex = 0;
            foreach (Effect effect in currentEffects.Values)
            {
                Rect effectRect = rect;
                effectRect.width = 25;
                effectRect.height = 25;
                effectRect.y -= 25;
                effectRect.x += 25 * effectIndex;
                effectIndex++;
                if (effect.duration == 1)
                {
                    if (battleMasterScript.isFlashing)
                        GUI.Box(effectRect, effect.name.Substring(0, 1));
                } else
                {
                    GUI.Box(effectRect, effect.name.Substring(0, 1));
                }
                //Debug.Log($"{effectRect.x} and {effectRect.y} and {Input.mousePosition} and {Screen.height - Input.mousePosition.y}");
                Vector2 mousePos = new Vector2(Input.mousePosition.x, Screen.height - Input.mousePosition.y);
                if (effectRect.Contains(mousePos))
                {
                    Rect descriptionRect = rect;
                    descriptionRect.width *= 2;
                    descriptionRect.height /= 2;
                    descriptionRect.x = (Screen.width / 2) - (descriptionRect.width / 2);
                    descriptionRect.y = Screen.height / 2;
                    GUI.Box(descriptionRect, $"{effect.name}\n {effect.description}");
                }
            }
        }
        base.OnGUI();
    }
    public void PlayOnHitSound()
    {
        soundMasterScript.PlaySound(this.onHitSounds[UnityEngine.Random.Range(0, this.onHitSounds.Length - 1)], 0);
    }
    public IEnumerator PlayWalkingSound (AudioClip sound)
    {
        if (!playingWalkingSound && sound)
        {
            playingWalkingSound = true;
            soundMasterScript.PlaySound(sound, 0);
            yield return new WaitForSeconds(sound.length);
            playingWalkingSound = false;
        }
        yield return true;
    }
    public virtual void RecalculateActions()
    {
        //Recalculating actions is important to re apply certain buffs (buffs of damage that other fighters apply to you before your turn starts)
        //(E.G) if you apply a damage buff we need to apply that affect to the persons attack damage
        if (this.currentAction != null)
        {
            if (this.currentAction.name == "Attack")
            {
                Attack atk = (Attack)this.currentAction;
                atk.damage = this.damage;
            }
            if (this.currentAction.name == "Double Attack")
            {
                DoubleAttack atk = (DoubleAttack)this.currentAction;
                atk.damage = this.damage;
            }
            if (this.currentAction.name == "Charged Stun Attack")
            {
                ChargedStunAttack atk = (ChargedStunAttack)this.currentAction;
                atk.damage = this.damage;
            }
        }
        
        return;
    }
    public void RemoveAllEffects()//strong cleanse (used after a battle is concluded?)
    {
        foreach (Effect effect in currentEffects.Values)
        {
            effect.Cleanse(this);
        }
        this.currentEffects = new Dictionary<int, Effect>();
    }
    public void RemoveAllEffectsOfName(string name)//might be used by actions to cleanse debuffs etc
    {
        for (int i = 0; i < currentEffects.Count; i++)
        {
            if (currentEffects[i].name == name)
            {
                currentEffects.Remove(currentEffects[i].GetKey());
            }
        }
    }
    public void SetAction(FighterAction action)
    {
        this.currentAction = action;
    }
    public bool RemoveEffect(int key)
    {
        if (currentEffects.Remove(key))
        {
            return true;
        }
        else
        {
            return false;
        }
    }
    public bool RemoveOnAttackEffect(int key)
    {
        if (onAttackEffects.Remove(key))
        {
            return true;
        }
        else
        {
            return false;
        }
    }
    public bool RemoveOnHitEffect(int key)
    {
        if (onHitEffects.Remove(key))
        {
            return true;
        } else
        {
            return false;
        }
    }
    public string TargetRelationToSelf(GameObject target)
    {
        string targetTag = target.tag;
        if (this.gameObject == target)
        {
            return "Self";
        } else if (this.gameObject.tag == "Party" && targetTag == "Player")
        {
            return "Friend";
        } else if (this.gameObject.tag == "Player" && targetTag == "Party")
        {
            return "Friend";
        }
        else if (targetTag == this.gameObject.tag)
        {
            return "Friend";
        } else
        {
            return "Foe";
        }
    }
    public virtual FighterAction TurnAction(ListBeingData allFighters)
    {
        //should 100% be instantiated by child
        return null;
    }
} 