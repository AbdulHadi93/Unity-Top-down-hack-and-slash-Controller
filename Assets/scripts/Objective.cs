using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Objective : MonoBehaviour {
    public bool isclear,isobjective = false;
    Finish finish;
    //public GameObject indicator;
    //public Transform canvas;
    indicator indicate;
    public GameObject poof;
	// Use this for initialization
	void Start () {
        finish = FindObjectOfType<Finish>();
        indicate = FindObjectOfType<indicator>();
        //GameObject indi=GameObject.Instantiate(indicator,canvas);
        //indi.GetComponent<indicator>().target=transform.GetChild(0).gameObject;
	}
	
	// Update is called once per frame
	void Update () {
        if(isobjective)
        transform.Rotate(transform.forward,20*Time.deltaTime);
	}
    void OnTriggerEnter(Collider hit)
    {
        if (hit.tag == "playerbone")
        {
            isclear = true;
            //indicate.points[indicate.i].gameObject.SetActive(false);
            //indicate.i++;
            if (isobjective)
            {
                gameObject.SetActive(false);
                GameObject.Instantiate(poof,transform.position,Quaternion.identity);
                finish.CheckLevelCompletion();
            }
        }
    }
}
