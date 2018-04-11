using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class playermovement : MonoBehaviour {
    Animator anim;
    enum States { forward, idle, dodge, back,attack};
    States currstate,prevstate;
    public Transform[] checkpoint;
    public Transform checkpointcont;
    public int maxcomboanim;
    int combocount = 1;
    int checkcount = 1;
    bool candodge = true;
    CharacterController controller;
	// Use this for initialization
	void Start () {
        controller = GetComponent<CharacterController>();
        anim = GetComponent<Animator>();
        currstate = States.idle;
        checkpoint= new Transform[checkpointcont.childCount];
        for (int i = 0; i < checkpointcont.childCount; i++)
        {
            checkpoint[i] = checkpointcont.GetChild(i);
        }
	}
	
	// Update is called once per frame
	void Update () {
        controller.Move(Physics.gravity);
        if (currstate == States.forward)
        {
            walkforward();
            candodge = true;
        }
        else if (currstate == States.back)
        {
            walkback();
            candodge = true;
        }
        else if (currstate == States.idle)
        {
            anim.SetInteger("playeranim", 0);
            anim.SetInteger("combo",0);
            candodge = true;
        }
        else if (currstate == States.dodge&&candodge)
        {
            anim.SetInteger("playeranim", 2);
            currstate = prevstate;
            candodge = false;
        }
        else if (currstate == States.attack)
        {
            anim.SetInteger("combo", combocount);
        }
	}
    IEnumerator resetcombo()
    {
        yield return new WaitForSeconds(1);
        combocount = 0;
    }
    void walkforward()
    {
        if (Vector3.Distance(transform.position, checkpoint[checkpoint.Length-1].position) < 0.5f)
        {
            currstate = States.idle;
        }
        else
        {
            anim.SetInteger("playeranim", 1);
            transform.LookAt(new Vector3(checkpoint[checkcount].position.x, transform.position.y, checkpoint[checkcount].position.z));

            if (Vector3.Distance(transform.position, checkpoint[checkcount].position) < 0.5f && checkcount < checkpoint.Length)
                checkcount++;
        }
    }
    void walkback()
    {
        if (Vector3.Distance(transform.position, checkpoint[0].position) < 0.5f)
        {
            currstate = States.idle;
        }
        else
        {
            anim.SetInteger("playeranim", 1);
            transform.LookAt(new Vector3(checkpoint[checkcount - 1].position.x, transform.position.y, checkpoint[checkcount - 1].position.z));
            if (Vector3.Distance(transform.position, checkpoint[checkcount - 1].position) < 0.5f && checkcount > 0)
                checkcount--;
        }
    }
    public void Changestate(string state)
    {
        if (state == "forward")
        {
            currstate = States.forward;
            prevstate = currstate;
        }
        else if (state == "idle")
        {
            currstate = States.idle;
            prevstate = currstate;
        }
        else if (state == "back")
        {
            currstate = States.back;
            prevstate = currstate;
        }
        else if (state == "dodge")
        {
            currstate = States.dodge;
        }
        else if (state == "attack")
        {
            currstate = States.attack;
            if (combocount == 0)
                combocount = 1;
            if (maxcomboanim == combocount)
            {
                combocount = 0;
                StopAllCoroutines();
            }
            else
            {
                StopAllCoroutines();
                StartCoroutine(resetcombo());
            }
        }
    }
    public void IncComboCount()
    {
        combocount++;
        if (prevstate == States.forward)
        {
            currstate = States.forward;
            anim.SetInteger("combo",0);
        }
        else if (prevstate == States.back)
        {
            currstate = States.back;
            anim.SetInteger("combo",0);
        }
        else
            currstate = States.idle;
    }
}
