﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RonnyJson
{
    public float speed, health;
}
public class Ronny : Being
{
    float speed, health;
    public override string CompactBeingDataIntoJson()
    {
        BeingData being = JsonUtility.FromJson<BeingData>(this.beingData);
        being.location = this.gameObject.transform.position;
        being.angle = this.gameObject.transform.rotation;
        being.scale = this.gameObject.transform.localScale;

        RonnyJson Ronnyj = new RonnyJson();
        //
        //! needs to assign RonnyJson values to updated values
        //
        being.jsonData = JsonUtility.ToJson(Ronnyj);

        return JsonUtility.ToJson(being);
    }
    public override void InjectData(string jsonData)
    {
        if (JsonUtility.FromJson<BeingData>(jsonData) != null)
        {

            BeingData being = JsonUtility.FromJson<BeingData>(jsonData);

            if (being.jsonData != null && being.jsonData != string.Empty)
            {
                RonnyJson Ronnyj = JsonUtility.FromJson<RonnyJson>(being.jsonData);

                this.health = Ronnyj.health;
                this.speed = Ronnyj.speed;
                this.ChangeSpeed(this.speed);
                this.SetHealth(this.health);
            }

            this.ID = being.objectID;
            this.beingData = jsonData;
        }

        return;
    }
    public override void Interact()
    {
        
    }
    public void ChangeSpeed(float newSpeed)
    {
        this.gameObject.GetComponent<playerMovement>().speed = newSpeed;
    }
    public void AddToHealth(float change)
    {
        this.health += change;
    }
    private void SetHealth(float health)
    {
        this.health = health;
    }
}
