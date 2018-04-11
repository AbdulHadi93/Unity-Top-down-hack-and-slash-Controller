using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Health : MonoBehaviour {
    public int MaxHealth=100;
    public int currHealth;
    public Slider healthbar;
    TPlayerMovement player;
    EnemyMovement enemy;
    public HitBox box;
	// Use this for initialization
	void Start () {
        currHealth = MaxHealth;
        healthbar.maxValue = MaxHealth;
        healthbar.value = currHealth;
        player = GetComponent<TPlayerMovement>();
        enemy = GetComponent<EnemyMovement>();
	}
	
	// Update is called once per frame
	void Update () {
        healthbar.transform.parent.LookAt(Camera.main.transform.position);
	}
    public void ApplyDamage(int damage)
    {
        currHealth -= damage;
        healthbar.value = currHealth;
        Checkdeath();
        if (!Checkdeath())
        {
            if (enemy)
            {
                DisableSwordCollider();
                enemy.Hit();
            }
            else if (player)
                player.Hit();
        }
    }
    bool Checkdeath()
    {
        if (currHealth <= 0)
        {
            if (enemy)
            {
                DisableSwordCollider();
                StartCoroutine(enemy.Death());
                healthbar.gameObject.SetActive(false);
            }
            else if(player)
            {
                player.Death();
            }
            return true;
        }
        return false;
    }
	public void EnableSwordCollider(int i)
    {
        if (player)
        {
			player.PlaySlash(i);
            player.isattack = true;
        }
        else if (enemy)
        {
            enemy.PlaySlash();
        }
//		if (i == 0)
		box.enablecollider (i);
//		else {
//			box.disablecollider ();
//			print ("disableasdadas");
//		}
    }

    public void DisableSwordCollider()
    {
        box.disablecollider();
        if (player)
        {
            player.isattack = false;
            player.once = true;
        }
    }

    public void ComboAnimTrigger()
    {
        if(player)
        player.ComboTrigger();
    }
}
