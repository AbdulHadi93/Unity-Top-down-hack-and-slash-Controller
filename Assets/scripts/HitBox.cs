using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HitBox : MonoBehaviour {
    public enum Layers{Player,Enemy};
    public Layers HitLayer;
    public int damage = 30;
    public Collider col;
	public Collider col2;
    //string layername;
	// Use this for initialization
	void Start () {
        //col = GetComponent<BoxCollider>();
        StartCoroutine(zerorotation());
	}
	
	// Update is called once per frame
	void Update () {
		
	}
    void OnCollisionEnter(Collision hit)
    {
        if (hit.gameObject.tag == HitLayer.ToString())
        {
            hit.gameObject.GetComponent<Health>().ApplyDamage(damage);
        }
    }
    IEnumerator zerorotation()
    {
        while (true)
        {
            transform.localPosition = Vector3.zero;
            transform.rotation = new Quaternion(0,0,0,0);
            yield return new WaitForSeconds(0.2f);
        }
    }
	public void enablecollider(int i)
    {
		if (i == 8)
			col2.enabled = true;
		else
        col.enabled = true;
    }
	public void disablecollider()
    {
		col2.enabled = false;
        col.enabled = false;
    }
}
