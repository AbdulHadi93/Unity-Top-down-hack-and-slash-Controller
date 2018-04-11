using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class audiomanager : MonoBehaviour {
    public AudioSource audio;
    public AudioClip[] slashclips;
    public AudioClip[] Deathclips;
    public AudioClip[] Hitclips;
    TPlayerMovement player;
    EnemyMovement enemy;
	// Use this for initialization
	void Start () {
        player = GetComponent<TPlayerMovement>();
        enemy = GetComponent<EnemyMovement>();
	}
	
	// Update is called once per frame
	void Update () {
		
	}
	public void slashtrigger()
    {
        if (player)
        {
            int counter = player.getcombocounter;
            if (player.getcombocounter >= slashclips.Length)
                counter = 0;
            audio.PlayOneShot(slashclips[counter]);
        }
        else if (enemy)
        {
            audio.PlayOneShot(slashclips[enemy.getattackcount]);
        }
    }
    public void DeathSoundPlay()
    {
        audio.PlayOneShot(Deathclips[Random.Range(0,Deathclips.Length)]);
    }
    public void HitSoundPlay()
    {
        audio.PlayOneShot(Hitclips[Random.Range(0, Hitclips.Length)]);
    }
}
