using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public enum Trap { controller,spikes,movingsaw,falltrigger,swingingsaw}
public class traps : MonoBehaviour {
    public Trap trap;
     Animator anim;
    public float delay = 0.35f;
	// Use this for initialization
	void Start () {
        anim = GetComponent<Animator>();
        if (trap == Trap.movingsaw)
        {
            anim=transform.parent.GetComponent<Animator>();
        }
        else { anim = GetComponent<Animator>(); }
	}
	
	// Update is called once per frame
	void Update () {
        if (trap == Trap.movingsaw)
        {
            transform.Rotate(Vector3.forward * 500 * Time.deltaTime);
            anim.speed = delay;
        }
	}
    void OnTriggerEnter(Collider hit)
    {
        if (hit.tag == "playerbone"&&hit as CapsuleCollider)
        {
            if (trap == Trap.controller)
                StartCoroutine(enableanim());
            else if (trap == Trap.spikes||trap==Trap.movingsaw||trap==Trap.swingingsaw)
            {
                hit.GetComponentInParent<TPlayerMovement>().Death();
            }
            else if(trap==Trap.falltrigger)
            {
                hit.GetComponentInParent<TPlayerMovement>().isfall = true;
                hit.GetComponentInParent<TPlayerMovement>().Death();
                hit.GetComponentInParent<CharacterController>().enabled = false;
            }
        }
    }
    IEnumerator enableanim()
    {
        yield return new WaitForSeconds(delay);
        anim.SetBool("enabled", true);
        yield return new WaitForSeconds(5);
        anim.SetBool("enabled", false);
    }
}
