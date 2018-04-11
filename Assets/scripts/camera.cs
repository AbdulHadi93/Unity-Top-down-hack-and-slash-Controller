using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class camera : MonoBehaviour {
    public Transform playerhead;
    public Vector3 offset;
    public float distance,zoomspeed,maxzoomdist = 1;
    private float zoomdistance=1;
    float newdistance;
    Animator anim;
    Quaternion newrotation;
    public Quaternion rotoffset;
	// Use this for initialization
	void Start () {
        newdistance = distance;
        anim = GetComponent<Animator>();
	}
	
	// Update is called once per frame
	void LateUpdate () {
        transform.position = playerhead.position;
        newdistance = Mathf.Lerp(newdistance, zoomdistance, zoomspeed * Time.deltaTime);

        newrotation = new Quaternion(playerhead.rotation.x + rotoffset.x, playerhead.rotation.y + rotoffset.y, playerhead.rotation.z + rotoffset.z, playerhead.rotation.w + rotoffset.w);
        transform.rotation = Quaternion.Slerp(transform.rotation,newrotation,1*Time.deltaTime);
        transform.position -=transform.forward * newdistance;
        transform.LookAt(playerhead);
	}
    public void zoomin()
    {
        zoomdistance = maxzoomdist;
        anim.enabled = true;
    }
    public void zoomout()
    {
        zoomdistance = distance;
        anim.enabled = false;
    }
}
