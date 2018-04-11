using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HitBox2 : MonoBehaviour {

	public enum Layers{Player,Enemy};
	public Layers HitLayer;
	public int damage = 100;
	BoxCollider col;
	//string layername;
	// Use this for initialization
	void Start () {
		col = GetComponent<BoxCollider>();
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
	public void enablecollider()
	{
		col.enabled = true;
	}
	public void disablecollider()
	{
		col.enabled = false;
	}
}

